////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

import { spawnSync } from "node:child_process";
import { mkdirSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const root = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const output = join(root, "artifacts", "pipewire-flatpak");
const appId = "io.github.kekyo.FlashCap.PipeWireTest";
const bundleOnly = process.argv.length === 3 && process.argv[2] === "--bundle-only";
if (process.argv.length > 2 && !bundleOnly)
  throw new Error("Usage: node scripts/build-pipewire-flatpak.mjs [--bundle-only]");
const run = (command, args) => {
  process.stdout.write(`\n> ${command} ${args.join(" ")}\n`);
  const result = spawnSync(command, args, { cwd: root, stdio: "inherit" });
  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error(`${command} exited with ${result.status}.`);
};

mkdirSync(output, { recursive: true });
for (const [name, project] of [
  ["avalonia", "samples/FlashCap.Avalonia/FlashCap.Avalonia"],
  ["smoke", "tests/FlashCap.PipeWire.Smoke"],
]) {
  // Rebuild every referenced TFM together so cached assemblies cannot retain
  // different RelaxVersioner versions when packaging after a commit.
  run("dotnet", [
    "build", project, "-f", "net10.0", "-r", "linux-x64", "-c", "Release",
    "--self-contained", "true", "--no-incremental",
  ]);
  run("dotnet", [
    "publish", project, "-f", "net10.0", "-r", "linux-x64", "-c", "Release",
    "--no-build", "--no-restore",
    "--self-contained", "true", "-p:PublishTrimmed=false",
    "-o", join(output, "publish", name),
  ]);
}
run("flatpak-builder", [
  "--user", "--force-clean", `--state-dir=${join(output, "builder-state")}`,
  `--repo=${join(output, "repo")}`, join(output, "build"),
  join(root, "tests", "FlashCap.PipeWire.Flatpak", `${appId}.json`),
]);
const bundle = join(output, `${appId}.flatpak`);
run("flatpak", [
  "build-bundle", ...(bundleOnly ? [] : ["--runtime-repo=https://dl.flathub.org/repo/flathub.flatpakrepo"]),
  join(output, "repo"), bundle, appId, "test",
]);
if (bundleOnly) console.log(`Built test bundle: ${bundle}`);
else {
  run("flatpak", ["install", "--user", "--noninteractive", "-y", "--reinstall", bundle]);
  console.log(`Installed test bundle: ${bundle}`);
}
