////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

import assert from "node:assert/strict";
import { execFileSync, spawn } from "node:child_process";
import { once } from "node:events";
import { mkdirSync } from "node:fs";
import { resolve, join } from "node:path";
import { setTimeout } from "node:timers/promises";

// Start capture before running this X11 test. PROCESS_ID is the application
// process (mono for net48), rather than the parent dotnet run process.
const [windowId, processId, outputPath] = process.argv.slice(2);
if (!windowId || !processId || !outputPath) {
  throw new Error("Usage: node scripts/test-avalonia-close.mjs WINDOW_ID PROCESS_ID OUTPUT_DIRECTORY");
}
const output = resolve(outputPath);
mkdirSync(output, { recursive: true });
const run = (command, args) => execFileSync(command, args, { encoding: "utf8" });
const geometry = run("xwininfo", ["-id", windowId]);
const left = Number(/Absolute upper-left X:\s+(-?\d+)/.exec(geometry)?.[1]);
const top = Number(/Absolute upper-left Y:\s+(-?\d+)/.exec(geometry)?.[1]);
assert.ok(Number.isFinite(left) && Number.isFinite(top));
assert.match(geometry, /Width:\s+800\b/);
assert.match(geometry, /Height:\s+450\b/);
const pid = Number(processId);
assert.ok(Number.isInteger(pid) && pid > 1);
const alive = () => {
  try { process.kill(pid, 0); return true; }
  catch (error) { if (error.code === "ESRCH") return false; throw error; }
};
assert.ok(alive(), "The sample must be running before the close request.");
run("xdotool", ["windowactivate", "--sync", windowId]);
const video = join(output, "close.mp4");
const recorder = spawn("ffmpeg", [
  "-hide_banner", "-loglevel", "error", "-y", "-f", "x11grab", "-framerate", "10",
  "-video_size", "828x516", "-i", `${process.env.DISPLAY}+${left - 14},${top - 49}`,
  "-an", "-c:v", "libx264", "-preset", "ultrafast", "-crf", "0", "-pix_fmt", "yuv420p", video,
], { stdio: ["pipe", "ignore", "inherit"] });
const recorded = once(recorder, "exit");
try {
  // Record the live preview before clicking the GNOME title-bar close button.
  await setTimeout(2000);
  run("import", ["-window", windowId, join(output, "before-close.png")]);
  run("xdotool", ["mousemove", String(left + 790), String(top - 25), "click", "1"]);
  const requested = Date.now();
  while (alive()) {
    assert.ok(Date.now() - requested < 30000, "Closing the sample must terminate its process.");
    await setTimeout(20);
  }
  process.stdout.write(`Sample exited ${Date.now() - requested} ms after clicking Close.\n`);
} finally {
  recorder.stdin.end("q\n");
  const [code] = await recorded;
  assert.equal(code, 0, "The recording must finish successfully.");
}
const hashes = run("ffmpeg", [
  "-hide_banner", "-loglevel", "error", "-i", video, "-t", "1.5",
  "-vf", "crop=600:250:114:199", "-f", "framemd5", "-",
]).split("\n").filter((line) => line && !line.startsWith("#")).map((line) => line.split(",").at(-1).trim());
assert.ok(hashes.length >= 10, "Record enough frames before the close request.");
assert.ok(new Set(hashes).size > 1, "Capture must be running when the window is closed.");
process.stdout.write(`Passed: ${hashes.length} preview frames and application shutdown.\n`);
