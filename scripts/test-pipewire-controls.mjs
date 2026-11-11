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

// Start C930e PipeWire capture and open Camera controls before running this X11 test.
const [mainWindow, controlsWindow, nodeId, outputPath] = process.argv.slice(2);
if (!mainWindow || !controlsWindow || !nodeId || !outputPath) {
  throw new Error("Usage: node scripts/test-pipewire-controls.mjs MAIN_WINDOW CONTROLS_WINDOW PIPEWIRE_NODE OUTPUT_DIRECTORY");
}
const output = resolve(outputPath);
mkdirSync(output, { recursive: true });
const run = (command, args) => execFileSync(command, args, { encoding: "utf8" });
const geometry = (window) => {
  const info = run("xwininfo", ["-id", window]);
  const x = Number(/Absolute upper-left X:\s+(-?\d+)/.exec(info)?.[1]);
  const y = Number(/Absolute upper-left Y:\s+(-?\d+)/.exec(info)?.[1]);
  assert.ok(Number.isFinite(x) && Number.isFinite(y));
  return { x, y };
};
const main = geometry(mainWindow);
// Keep the controls beside the preview so both are visible in the recording.
run("xdotool", ["windowmove", controlsWindow, String(main.x + 820), String(main.y - 49)]);
const moveDeadline = Date.now() + 30000;
while (geometry(controlsWindow).x !== main.x + 820 || geometry(controlsWindow).y !== main.y) {
  assert.ok(Date.now() < moveDeadline, "Place the controls beside the preview before recording.");
  await setTimeout(10);
}
const controls = geometry(controlsWindow);
const left = Math.min(main.x, controls.x);
const top = Math.min(main.y, controls.y);
const width = Math.ceil((Math.max(main.x + 800, controls.x + 320) - left) / 2) * 2;
const height = Math.ceil((Math.max(main.y + 450, controls.y + 560) - top) / 2) * 2;
const brightness = () => {
  const props = run("pw-cli", ["enum-params", nodeId, "Props"]);
  const match = /Props:brightness[^\n]*\n\s+Int (\d+)/.exec(props);
  assert.ok(match, "The selected C930e must expose integer brightness through PipeWire.");
  return Number(match[1]);
};
const original = brightness();
const thumbPosition = () => {
  // Read a row above the slider track to find the blue thumb alone. Device
  // readback can arrive before Avalonia has rendered the corresponding position.
  const pixels = run("import", ["-window", controlsWindow, "-crop", "320x1+0+49", "-depth", "8", "txt:-"]);
  const blue = [...pixels.matchAll(/^(\d+),0: \((\d+),(\d+),(\d+)\)/gm)]
    .filter((pixel) => Number(pixel[4]) > 120 && Number(pixel[4]) > Number(pixel[2]) + 50)
    .map((pixel) => Number(pixel[1]));
  return blue.length ? (blue[0] + blue.at(-1)) / 2 : Number.NaN;
};
const waitForBrightness = async (expected) => {
  const deadline = Date.now() + 30000;
  const position = Math.round(22 + expected * 276 / 255);
  while (true) {
    if (brightness() === expected && Math.abs(thumbPosition() - position) <= 1) return;
    assert.ok(Date.now() < deadline, `Brightness must reach ${expected}.`);
    await setTimeout(10);
  }
};
const setBrightness = async (value) => {
  run("xdotool", ["windowactivate", "--sync", controlsWindow]);
  // Reset to the minimum to locate the thumb, then drag to the target value.
  run("xdotool", ["key", "--clearmodifiers", "Home"]);
  await waitForBrightness(0);
  run("xdotool", ["mousemove", "--window", controlsWindow, "22", "57", "mousedown", "1",
    "mousemove", "--window", controlsWindow, String(Math.round(22 + value * 276 / 255)), "57", "mouseup", "1"]);
  await waitForBrightness(value);
};
const video = join(output, "brightness-drag.mp4");
const recorder = spawn("ffmpeg", [
  "-hide_banner", "-loglevel", "error", "-y", "-f", "x11grab", "-framerate", "20",
  "-video_size", `${width}x${height}`, "-i", `${process.env.DISPLAY}+${left},${top}`,
  "-an", "-c:v", "libx264", "-preset", "ultrafast", "-crf", "0", "-pix_fmt", "yuv420p", video,
], { stdio: ["pipe", "ignore", "inherit"] });
const recorded = once(recorder, "exit");
try {
  await waitForBrightness(original);
  run("xdotool", ["windowactivate", "--sync", controlsWindow,
    "mousemove", "--window", controlsWindow, String(Math.round(22 + original * 276 / 255)), "57", "click", "1"]);
  await setBrightness(64);
  await setBrightness(192);
  await setBrightness(64);
  // Drag continuously: property-change callbacks must overlap asynchronous refreshes.
  run("xdotool", ["mousemove", "--window", controlsWindow, "91", "57", "mousedown", "1"]);
  try {
    for (let sweep = 0; sweep < 10; sweep++) {
      for (let step = 0; step < 61; step++) {
        const x = Math.round(91 + 139 * (sweep % 2 === 0 ? step : 60 - step) / 60);
        run("xdotool", ["mousemove", "--window", controlsWindow, String(x), "57"]);
        await setTimeout(8);
      }
    }
  } finally { run("xdotool", ["mouseup", "1"]); }
  await setBrightness(64);
  await setBrightness(192);
} finally {
  try {
    await setBrightness(original);
    // Record enough frames after all slider operations to verify capture continues.
    await setTimeout(1500);
  } finally {
    recorder.stdin.end("q\n");
    const [code] = await recorded;
    assert.equal(code, 0, "The UI recording must finish successfully.");
  }
}

geometry(mainWindow);
geometry(controlsWindow);
assert.equal(brightness(), original, "Restore the user's brightness after the test.");
const hashes = run("ffmpeg", [
  "-hide_banner", "-loglevel", "error", "-i", video,
  "-vf", `crop=600:250:${main.x - left + 100}:${main.y - top + 150}`, "-f", "framemd5", "-",
]).split("\n").filter((line) => line && !line.startsWith("#")).map((line) => line.split(",").at(-1).trim());
assert.ok(hashes.length >= 80, "Record the slider changes and the following preview frames.");
assert.ok(new Set(hashes.slice(-20)).size > 1, "The camera preview must keep updating after rapid changes.");
run("import", ["-window", mainWindow, join(output, "preview-after-drag.png")]);
run("import", ["-window", controlsWindow, join(output, "controls-after-drag.png")]);
process.stdout.write(`Passed: 610 drag moves, brightness 64/192 verified, ${hashes.length} video frames, restored brightness=${original}.\n`);
