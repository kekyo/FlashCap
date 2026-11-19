// FlashCap - Independent camera capture library. Licensed under Apache-2.0.
// Run inside the guest's GNOME session. All input goes to its virtual X11 display.
import assert from "node:assert/strict";
import { execFileSync, spawnSync } from "node:child_process";
import { writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { setTimeout } from "node:timers/promises";
import { fileURLToPath } from "node:url";

const [scenario, results] = process.argv.slice(2);
assert.ok(["allow", "deny"].includes(scenario));
const app = "io.github.kekyo.FlashCap.PipeWireTest";
const commands = dirname(fileURLToPath(import.meta.url));
const run = (command, args) => execFileSync(command, args, { encoding: "utf8", timeout: 90000 });
const waitFor = async (description, check) => {
  console.log(`Waiting for ${description}`);
  const deadline = Date.now() + 60000;
  do {
    const value = check();
    if (value) return value;
    await setTimeout(250);
  } while (Date.now() < deadline);
  throw new Error(`Timed out waiting for ${description}`);
};
const windowNamed = (name) => {
  const result = spawnSync("xdotool", ["search", "--onlyvisible", "--name", name], { encoding: "utf8" });
  return result.status === 0 && result.stdout.trim().split("\n").at(-1);
};
const wordsOnScreen = (window, name) => {
  const path = join(results, name + ".png");
  run("import", ["-window", window, path]);
  // Isolate the light text on the dimmed GNOME desktop and the dark preview.
  // Disabled buttons use gray text. The control editors use a light background.
  const input = name !== "controls" ?
    execFileSync("convert", [path, "-colorspace", "Gray", "-threshold", "53%", "-negate", "png:-"]) : undefined;
  const tsv = execFileSync("tesseract", [input ? "stdin" : path, "stdout", "--psm", "11", "tsv", "quiet"],
    { input, encoding: "utf8", stdio: ["pipe", "pipe", "pipe"] });
  return tsv.trimEnd().split("\n").slice(1).map(row => row.split("\t"))
    .filter(columns => columns[0] === "5" && columns[11]?.trim())
    .map(columns => ({
      text: columns[11].toLowerCase(),
      x: Math.round(Number(columns[6]) + Number(columns[8]) / 2),
      y: Math.round(Number(columns[7]) + Number(columns[9]) / 2),
    }));
};
const cameraPermission = () => {
  const permissions = run("flatpak", ["permission-show", app]);
  return permissions.split("\n").map(line => line.split(/\s+/))
    .find(([table, id, application]) => table === "devices" && id === "camera" && application === app)?.[3];
};
const busProperty = (destination, path, iface, property) => {
  const query = spawnSync("gdbus", ["call", "--session", "--timeout", "5", "--dest", destination,
    "--object-path", path, "--method", "org.freedesktop.DBus.Properties.Get", iface, property], { encoding: "utf8" });
  return query.status === 0 ? query.stdout.trim() : "";
};

assert.equal(cameraPermission(), undefined, "Each case must begin with an undecided camera permission.");
console.log("Camera access was undecided before launching the app.");
await waitFor("WirePlumber to select the test camera", () => {
  const info = spawnSync("wpctl", ["inspect", "@DEFAULT_VIDEO_SOURCE@"], { encoding: "utf8", timeout: 5000 });
  return info.status === 0 && info.stdout.includes('api.v4l2.path = "/dev/video0"');
});
await waitFor("the desktop portal to discover the test camera", () =>
  busProperty("org.freedesktop.portal.Desktop", "/org/freedesktop/portal/desktop",
    "org.freedesktop.portal.Camera", "IsCameraPresent").includes("true"));
// Select the registered desktop entry in GNOME's application search, as a user would.
// The shell grants camera requests only to the foreground application.
const overviewActive = () => busProperty("org.gnome.Shell", "/org/gnome/Shell", "org.gnome.Shell", "OverviewActive");
if (!(await waitFor("the GNOME Shell overview", overviewActive)).includes("true")) {
  run("xdotool", ["key", "Super_L"]);
  await waitFor("the GNOME Shell overview to open", () => overviewActive().includes("true"));
}
const width = Number(run("xdotool", ["getdisplaygeometry"]).split(" ")[0]);
run("xdotool", ["mousemove", String(Math.floor(width / 2)), "62", "click", "1"]);
run("xdotool", ["type", "--clearmodifiers", "--delay", "50", "FlashCap"]);
const entry = await waitFor("the Flatpak desktop entry in the GNOME search results", () =>
  wordsOnScreen("root", "launcher").find(word => word.y > 100 && /flashcap|pipew/.test(word.text)));
run("xdotool", ["mousemove", String(entry.x), String(entry.y), "click", "1"]);
const main = await waitFor("the Avalonia window", () => windowNamed("^FlashCap\\.Avalonia$"));
const button = await waitFor("the initial GNOME camera permission dialog", () => {
  const words = wordsOnScreen("root", "first-permission");
  const text = words.map(word => word.text).join(" ");
  if (!text.includes("turn on camera") || !text.includes("flashcap pipewire test") ||
      !words.some(word => word.text === "allow") || !words.some(word => word.text === "deny")) return false;
  return words.find(word => word.text === scenario);
});
run("xdotool", ["mousemove", String(button.x), String(button.y), "click", "1"]);
const decision = scenario === "allow" ? "yes" : "no";
await waitFor(`the portal to record ${decision} for ${app}`, () => cameraPermission() === decision);

if (scenario === "allow") {
  await waitFor("the selected PipeWire camera to enable Start capture", () => {
    const words = wordsOnScreen(main, "device");
    return words.some(word => /pipewire/.test(word.text)) &&
      words.some(word => word.text === "start" && word.x < 110 && word.y > 30 && word.y < 62);
  });
  const preview = run("node", [join(commands, "test-pipewire-preview.mjs"), main, results]);
  console.log(preview);
  writeFileSync(join(results, "preview.txt"), preview);
  run("xdotool", ["mousemove", "--window", main, "50", "48", "click", "1"]);
  await waitFor("the camera controls window during capture", () => {
    run("xdotool", ["mousemove", "--window", main, "380", "48", "click", "1"]);
    return windowNamed("^Camera controls$");
  });
  const controls = windowNamed("^Camera controls$");
  await waitFor("the brightness editor", () =>
    wordsOnScreen(controls, "controls").some(word => word.text.includes("brightness")));
  run("xdotool", ["windowactivate", "--sync", controls, "key", "alt+F4"]);
} else {
  // Read the public enumeration API again using the same sandbox identity.
  const smoke = run("flatpak", ["run", "--command=FlashCap.PipeWire.Smoke", app,
    "--test=FlashCap.PipeWire.Smoke.C930eCaptureTests.EnumeratesOnlyAvailableBackends",
    "--params=backends=none", "--noresult"]);
  writeFileSync(join(results, "enumeration.txt"), smoke);
  assert.match(smoke, /Test Count: 1, Passed: 1, Failed: 0/);
}
run("xdotool", ["windowactivate", "--sync", main, "key", "alt+F4"]);
await waitFor("the sample to close", () => !windowNamed("^FlashCap\\.Avalonia$"));
console.log(`Initial dialog recorded ${decision} and the sample closed.`);
