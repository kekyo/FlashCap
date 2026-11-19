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
const appId = "io.github.kekyo.FlashCap.PipeWireTest";
const scenarios = {
  both: { backends: "V4L2,PipeWire", permission: "yes", args: ["--device=all", "--filesystem=xdg-run/pipewire-0"] },
  pipewire: { backends: "PipeWire", permission: "yes", args: [] },
  v4l2: { backends: "V4L2", permission: "yes", args: ["--device=all"] },
  none: { backends: "none", permission: "yes", args: [] },
  denied: { backends: "none", permission: "no", args: [] },
};
const modes = process.argv.length > 2 ? process.argv.slice(2) : Object.keys(scenarios);
const run = (command, args) => {
  process.stdout.write(`\n> ${command} ${args.join(" ")}\n`);
  const result = spawnSync(command, args, { cwd: root, stdio: "inherit" });
  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error(`${command} exited with ${result.status}.`);
};

for (const mode of modes) {
  const scenario = scenarios[mode];
  if (!scenario) throw new Error(`Unknown scenario: ${mode}. Choose ${Object.keys(scenarios).join(", ")}.`);
  const output = join(root, "artifacts", "pipewire-flatpak", mode);
  mkdirSync(output, { recursive: true });
  // Only this dedicated test application's portal permission is changed.
  run("flatpak", ["permission-set", "devices", "camera", appId, scenario.permission]);
  try {
    // Flatpak always exposes portals and sets the bus address during startup.
    // Set the unavailable address inside the sandbox to test service absence.
    const environment = mode === "v4l2" || mode === "none" ?
      ["DBUS_SESSION_BUS_ADDRESS=unix:path=/run/flashcap-no-session-bus"] : [];
    run("flatpak", [
      "run", "--user", "--command=env",
      ...scenario.args, `--filesystem=${output}`, appId,
      ...environment, "FlashCap.PipeWire.Smoke",
      `--params=backends=${scenario.backends}`, `--work=${output}`,
      `--result=${join(output, "TestResult.xml")}`,
    ]);
  } finally {
    run("flatpak", ["permission-remove", "devices", "camera", appId]);
  }
}
