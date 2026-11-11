////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { mkdirSync } from "node:fs";
import { resolve, join } from "node:path";

// Select the C930e and a format in the sample before running this X11 test.
const [windowId, outputPath] = process.argv.slice(2);
if (!windowId || !outputPath) throw new Error("Usage: node scripts/test-pipewire-preview.mjs WINDOW_ID OUTPUT_DIRECTORY");
const output = resolve(outputPath);
mkdirSync(output, { recursive: true });
const run = (command, args) => execFileSync(command, args, { encoding: "utf8" });
const geometry = run("xwininfo", ["-id", windowId]);
const left = Number(/Absolute upper-left X:\s+(\d+)/.exec(geometry)?.[1]);
const top = Number(/Absolute upper-left Y:\s+(\d+)/.exec(geometry)?.[1]);
assert.ok(Number.isFinite(left) && Number.isFinite(top), "The sample window must be on screen.");
assert.match(geometry, /Width:\s+800\b/);
assert.match(geometry, /Height:\s+450\b/);
run("xdotool", ["windowactivate", "--sync", windowId]);
const click = (x) => run("xdotool", ["mousemove", String(left + x), String(top + 48), "click", "1"]);
const capture = (name, updating) => {
  const video = join(output, `${name}.mp4`);
  run("ffmpeg", [
    "-hide_banner", "-loglevel", "error", "-y", "-f", "x11grab", "-framerate", "10",
    "-window_id", windowId, "-video_size", "800x450", "-i", process.env.DISPLAY,
    "-t", "3", "-an", "-c:v", "libx264", "-preset", "ultrafast", "-crf", "0", "-pix_fmt", "yuv420p", video,
  ]);
  // Exclude controls, counters, and the initial transition. Inspect only the image.
  const hashes = run("ffmpeg", [
    "-hide_banner", "-loglevel", "error", "-ss", "1", "-i", video,
    "-vf", "crop=600:250:100:150", "-f", "framemd5", "-",
  ]).split("\n").filter((line) => line && !line.startsWith("#")).map((line) => line.split(",").at(-1).trim());
  assert.ok(hashes.length >= 15, `${name}: enough frames must be recorded.`);
  const unique = new Set(hashes).size;
  if (updating) assert.ok(unique > 1, `${name}: the camera image must update.`);
  else assert.equal(unique, 1, `${name}: the camera image must remain still after stopping.`);
  run("ffmpeg", [
    "-hide_banner", "-loglevel", "error", "-y", "-ss", "2", "-i", video,
    "-frames:v", "1", join(output, `${name}.png`),
  ]);
  process.stdout.write(`${name}: ${hashes.length} frames, ${unique} distinct images\n`);
};

try {
  click(50);
  capture("running", true);
  click(150);
  capture("stopped", false);
  click(50);
  capture("restarted", true);
} finally {
  click(150);
}
