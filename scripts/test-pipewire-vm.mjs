// FlashCap - Independent camera capture library. Licensed under Apache-2.0.
import { execFileSync, spawnSync } from "node:child_process";
import { createHash } from "node:crypto";
import {
  accessSync, chmodSync, constants, existsSync, mkdirSync, mkdtempSync,
  readFileSync, renameSync, rmSync, writeFileSync,
} from "node:fs";
import { basename, dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const root = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const guest = join(root, "tests/FlashCap.PipeWire.Flatpak/vm");
const work = resolve(process.env.FLASHCAP_VM_DIR || join(root, "artifacts/pipewire-vm"));
const cloudURL = "https://cloud-images.ubuntu.com/releases/24.04/release-20261017/ubuntu-24.04-server-cloudimg-amd64.img";
const cloudHash = "04e8f2461a16a44e78a370284471f29616cdd9baf1893df25807b71f985b6c94";
const machine = "pc-q35-8.2";
const cpu = "Nehalem-v1";
const seedFiles = ["user-data", "provision.sh", "runtimes.lock"];
const recipe = createHash("sha256").update(cloudHash).update(machine).update(cpu);
for (const name of seedFiles) recipe.update(readFileSync(join(guest, name)));
const baseDirectory = join(work, `gnome-46-${recipe.digest("hex").slice(0, 16)}`);
const base = join(baseDirectory, "base.qcow2");
const bundle = join(root, "artifacts/pipewire-flatpak/io.github.kekyo.FlashCap.PipeWireTest.flatpak");

const run = (command, args) => execFileSync(command, args, { stdio: "inherit" });
const checksum = (file) => execFileSync("sha256sum", [file], { encoding: "utf8" }).split(" ")[0];
const drivePath = (path) => path.replaceAll(",", ",,");
const verify = (file, expected) => {
  if (checksum(file) !== expected) throw new Error(`SHA-256 mismatch: ${file}`);
};
const runtimeBundles = (directory) => {
  const cache = join(work, "runtime");
  mkdirSync(cache, { recursive: true });
  const lock = readFileSync(join(guest, "runtimes.lock"), "utf8").trim().split("\n");
  return lock.map(line => {
    const [ref, commit] = line.split(" ");
    const [, id, arch, branch] = ref.split("/");
    const bundle = join(cache, commit + ".flatpak");
    if (existsSync(bundle)) return bundle;
    const installed = spawnSync("flatpak", ["info", "--show-location", ref], { encoding: "utf8" });
    let repo;
    if (installed.status === 0 && basename(installed.stdout.trim()) === commit) {
      repo = resolve(installed.stdout.trim(), "../../../../..", "repo");
    } else {
      const installation = join(directory, "flatpak");
      const flatpak = (args) => execFileSync("flatpak", args, {
        env: { ...process.env, FLATPAK_USER_DIR: installation }, stdio: "inherit",
      });
      flatpak(["remote-add", "--user", "--if-not-exists", "flathub", "https://dl.flathub.org/repo/flathub.flatpakrepo"]);
      flatpak(["install", "--user", "--noninteractive", "--no-related", "--no-deps", "flathub", ref]);
      flatpak(["update", "--user", "--noninteractive", "--no-related", "--no-deps", "--commit=" + commit, ref]);
      repo = join(installation, "repo");
    }
    const revision = execFileSync("ostree", ["--repo=" + repo, "rev-parse", ref], { encoding: "utf8" }).trim();
    if (revision !== commit) throw new Error(`Expected ${ref} at ${commit}, found ${revision}`);
    const temporary = join(directory, commit + ".flatpak");
    run("flatpak", ["build-bundle", "--runtime", "--arch=" + arch, repo, temporary, id, branch]);
    renameSync(temporary, bundle);
    return bundle;
  });
};
const selectAccelerator = () => {
  const requested = process.env.FLASHCAP_VM_ACCEL;
  if (requested !== undefined) {
    if (requested === "kvm" || requested === "tcg") return requested;
    throw new Error("FLASHCAP_VM_ACCEL must be kvm or tcg.");
  }
  try { accessSync("/dev/kvm", constants.R_OK | constants.W_OK); return "kvm"; }
  catch (error) {
    if (["EACCES", "ENOENT"].includes(error.code)) return "tcg";
    throw error;
  }
};
const accel = selectAccelerator();
const boot = (disk, directory, devices, minutes) => {
  const serial = join(directory, "serial.log");
  console.log(`Guest console: ${serial}`);
  const result = spawnSync("qemu-system-x86_64", [
    "-machine", machine + ",accel=" + accel, "-cpu", cpu,
    "-smp", "4", "-m", "4096", "-vga", "virtio",
    "-device", "qemu-xhci", "-device", "usb-tablet", "-display", "none",
    "-serial", `file:${serial}`, "-monitor", "unix:monitor.sock,server=on,wait=off", "-no-reboot",
    "-drive", `file=${drivePath(disk)},format=qcow2,if=virtio`, ...devices,
  ], { cwd: directory, stdio: "inherit", timeout: minutes * (accel === "kvm" ? 1 : 4) * 60000 });
  if (result.error || result.status !== 0)
    throw new Error(`QEMU failed (${result.error?.message ?? result.status}). See ${serial}`);
};
const verifyBase = () => {
  if (!existsSync(base)) throw new Error("Run node scripts/test-pipewire-vm.mjs prepare to build the guest first.");
  verify(base, readFileSync(base + ".sha256", "utf8").trim());
};

const prepare = () => {
  if (existsSync(base)) {
    verifyBase();
    console.log(`Verified ${base}`);
    return;
  }
  const directory = mkdtempSync(join(work, "prepare-"));
  const cloud = join(work, `noble-${cloudHash}.img`);
  if (!existsSync(cloud)) {
    const download = join(directory, "download.img");
    run("curl", ["--fail", "--location", "--retry", "3", "--output", download, cloudURL]);
    verify(download, cloudHash);
    renameSync(download, cloud);
  } else verify(cloud, cloudHash);
  const seed = join(directory, "seed.iso");
  writeFileSync(join(directory, "meta-data"), "instance-id: flashcap-portal-base\nlocal-hostname: flashcap-test\n");
  run("genisoimage", ["-quiet", "-output", seed, "-volid", "cidata", "-rock", "-iso-level", "3",
    join(directory, "meta-data"), ...seedFiles.map(name => join(guest, name)), ...runtimeBundles(directory)]);
  rmSync(join(directory, "flatpak"), { recursive: true, force: true });
  const disk = join(directory, "building.qcow2");
  run("qemu-img", ["convert", "-O", "qcow2", cloud, disk]);
  run("qemu-img", ["resize", disk, "18G"]);
  const log = join(directory, "provision.log");
  boot(disk, directory, ["-serial", "file:" + log, "-cdrom", seed, "-nic", "user,model=virtio-net-pci"], 90);
  if (!readFileSync(log, "utf8").split(/\r?\n/).includes("FLASHCAP_BASE_READY"))
    throw new Error(`Guest preparation failed. See ${log}`);
  const hash = checksum(disk);
  chmodSync(disk, 0o444);
  const staged = join(directory, "base");
  mkdirSync(staged);
  renameSync(disk, join(staged, "base.qcow2"));
  writeFileSync(join(staged, "base.qcow2.sha256"), hash + "\n");
  try { renameSync(staged, baseDirectory); }
  catch (error) {
    if (error.code !== "ENOTEMPTY" && error.code !== "EEXIST") throw error;
    verifyBase();
    rmSync(staged, { recursive: true });
  }
  rmSync(seed);
  console.log(`Prepared ${base}\nSHA-256 ${readFileSync(base + ".sha256", "utf8").trim()}`);
};

const scenario = (name, files, inputs) => {
  const directory = mkdtempSync(join(work, name + "-"));
  const input = join(directory, "input");
  const results = join(directory, "results");
  mkdirSync(input);
  for (const [file, contents] of files) writeFileSync(join(input, file), contents);
  writeFileSync(join(input, "scenario"), name + "\n");
  const iso = join(directory, "input.iso");
  run("genisoimage", ["-quiet", "-output", iso, "-volid", "FLASHCAPINPUT", "-r", input]);
  rmSync(input, { recursive: true });
  const resultDisk = join(directory, "results.ext4");
  run("truncate", ["-s", "256M", resultDisk]);
  run("mkfs.ext4", ["-q", "-F", "-L", "FLASHCAPRESULT", "-E", "root_owner=1000:1000", resultDisk]);
  const overlay = join(directory, "overlay.qcow2");
  run("qemu-img", ["create", "-f", "qcow2", "-F", "qcow2", "-b", base, overlay]);
  try {
    boot(overlay, directory, ["-nic", "none", "-cdrom", iso,
      "-drive", `file=${drivePath(resultDisk)},format=raw,if=virtio`], 8);
  } finally {
    execFileSync("debugfs", ["-R", "rdump /results .", resultDisk], { cwd: directory, stdio: "inherit" });
    mkdirSync(results, { recursive: true });
    writeFileSync(join(results, "inputs.txt"), inputs);
    rmSync(iso, { force: true });
    rmSync(overlay, { force: true });
  }
  const outcome = join(results, "result");
  if (!existsSync(outcome) || readFileSync(outcome, "utf8").trim() !== `PASS ${name}`)
    throw new Error(`Scenario failed. See ${directory}`);
  rmSync(resultDisk);
  console.log(`Passed ${name}: ${results}`);
};

const mode = process.argv[2];
mkdirSync(work, { recursive: true });
if (mode === "prepare") prepare();
else if (["allow", "deny", "test"].includes(mode)) {
  verifyBase();
  // Both guests must run the same application and tests even if the checkout changes.
  const scripts = [join(guest, "session.sh"), join(guest, "test-desktop.mjs"), join(root, "scripts/test-pipewire-preview.mjs")];
  const files = scripts.map(path => [basename(path), readFileSync(path)]);
  const testHash = createHash("sha256");
  for (const [file, contents] of files) testHash.update(file).update(contents);
  const application = readFileSync(bundle);
  files.push(["flashcap.flatpak", application]);
  const qemu = execFileSync("qemu-system-x86_64", ["--version"], { encoding: "utf8" }).split("\n")[0];
  const inputs = [
    `base=${basename(baseDirectory)}`, `base.sha256=${readFileSync(base + ".sha256", "utf8").trim()}`,
    `bundle.sha256=${createHash("sha256").update(application).digest("hex")}`,
    `tests.sha256=${testHash.digest("hex")}`, `qemu=${qemu}`,
    `machine=${machine}`, `cpu=${cpu}`, `accel=${accel}`, "",
  ].join("\n");
  for (const name of mode === "test" ? ["allow", "deny"] : [mode]) scenario(name, files, inputs);
} else {
  console.error("Usage: node scripts/test-pipewire-vm.mjs prepare|test|allow|deny");
  process.exitCode = 2;
}
