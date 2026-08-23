////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

import { spawnSync } from "node:child_process";
import {
  copyFileSync,
  mkdirSync,
  mkdtempSync,
  readFileSync,
  readdirSync,
  rmSync,
  writeFileSync,
} from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const repositoryRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const temporaryRoot = mkdtempSync(join(tmpdir(), "flashcap-tests-"));
const packageDirectory = join(temporaryRoot, "packages");
const packageCache = join(temporaryRoot, "nuget-packages");
const httpCache = join(temporaryRoot, "nuget-http-cache");
const consumerNuGetConfig = join(temporaryRoot, "consumer.NuGet.Config");
const consumerSourceDirectory = join(
  repositoryRoot,
  "tests",
  "FlashCap.PackageConsumer",
);
const consumerDirectory = join(temporaryRoot, "consumer");
const consumerProject = join(consumerDirectory, "FlashCap.PackageConsumer.csproj");
const verifierProject = join(
  repositoryRoot,
  "tests",
  "FlashCap.PackageVerifier",
  "FlashCap.PackageVerifier.csproj",
);
const aotSourceDirectory = join(
  repositoryRoot,
  "tests",
  "FlashCap.Core.AotSmoke",
);
const aotDirectory = join(temporaryRoot, "aot-source");
const aotProject = join(aotDirectory, "FlashCap.Core.AotSmoke.csproj");
const legacyInteropDirectory = join(
  repositoryRoot,
  "tests",
  "FlashCap.Core.LegacyInteropSmoke",
);
const isolatedEnvironment = {
  ...process.env,
  DOTNET_CLI_TELEMETRY_OPTOUT: "1",
  NUGET_PACKAGES: packageCache,
  NUGET_HTTP_CACHE_PATH: httpCache,
};
const forbiddenPackageIds = new Set([
  "microsoft.windows.cswin32",
  "system.memory",
  "system.runtime.compilerservices.unsafe",
]);

const run = (command, args, environment = process.env) => {
  process.stdout.write(`\n> ${command} ${args.join(" ")}\n`);
  const result = spawnSync(command, args, {
    cwd: repositoryRoot,
    env: environment,
    stdio: "inherit",
  });
  if (result.error) {
    throw result.error;
  }
  if (result.status !== 0) {
    throw new Error(`${command} failed with exit code ${result.status}.`);
  }
};

const findCorePackage = () => {
  const candidates = readdirSync(packageDirectory).filter(
    (name) => /^FlashCap\.Core\..+\.nupkg$/.test(name) && !name.endsWith(".snupkg"),
  );
  if (candidates.length !== 1) {
    throw new Error(
      `Expected one FlashCap.Core package, found ${candidates.length}.`,
    );
  }
  const fileName = candidates[0];
  const version = fileName.slice("FlashCap.Core.".length, -".nupkg".length);
  return { fileName, version };
};

const verifyCoreRestoreGraph = () => {
  const assetsPath = join(
    repositoryRoot,
    "FlashCap.Core",
    "obj",
    "project.assets.json",
  );
  const assets = JSON.parse(readFileSync(assetsPath, "utf8"));
  const failures = [];
  for (const [targetName, target] of Object.entries(assets.targets ?? {})) {
    for (const libraryName of Object.keys(target)) {
      const separator = libraryName.lastIndexOf("/");
      const packageId = (
        separator < 0 ? libraryName : libraryName.slice(0, separator)
      ).toLowerCase();
      if (forbiddenPackageIds.has(packageId)) {
        failures.push(`${targetName}: ${packageId}`);
      }
    }
  }
  if (failures.length !== 0) {
    throw new Error(
      `FlashCap.Core restore graph contains forbidden packages:\n${failures.join("\n")}`,
    );
  }
};

const writeConsumerNuGetConfig = () => {
  const generatedSource = packageDirectory
    .replaceAll("&", "&amp;")
    .replaceAll('"', "&quot;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;");
  writeFileSync(
    consumerNuGetConfig,
    `<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="generated" value="${generatedSource}" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="generated">
      <package pattern="FlashCap" />
      <package pattern="FlashCap.Core" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
`,
    "utf8",
  );
};

try {
  mkdirSync(consumerDirectory);
  for (const fileName of ["FlashCap.PackageConsumer.csproj", "PackageSurface.cs"]) {
    copyFileSync(
      join(consumerSourceDirectory, fileName),
      join(consumerDirectory, fileName),
    );
  }
  mkdirSync(aotDirectory);
  for (const fileName of ["FlashCap.Core.AotSmoke.csproj", "Program.cs"]) {
    copyFileSync(
      join(aotSourceDirectory, fileName),
      join(aotDirectory, fileName),
    );
  }

  run("dotnet", ["restore", "FlashCap.sln", "--force", "--no-cache"]);
  verifyCoreRestoreGraph();
  run("dotnet", [
    "build",
    "FlashCap.sln",
    "--configuration",
    "Release",
    "--no-restore",
  ]);
  run("dotnet", [
    "test",
    "--solution",
    "FlashCap.sln",
    "--configuration",
    "Release",
    "--no-build",
    "--no-restore",
    "--minimum-expected-tests",
    "134",
  ]);
  for (const framework of ["net35", "net40", "net45", "net461", "net48"]) {
    run("mono", [
      join(
        legacyInteropDirectory,
        "bin",
        "Release",
        framework,
        "FlashCap.Core.Tests.exe",
      ),
    ]);
  }
  run("dotnet", [
    "pack",
    "FlashCap.sln",
    "--configuration",
    "Release",
    "--no-build",
    "--no-restore",
    "--output",
    packageDirectory,
  ]);

  const corePackage = findCorePackage();
  writeConsumerNuGetConfig();
  run(
    "dotnet",
    [
      "restore",
      consumerProject,
      "--force",
      "--no-cache",
      "--configfile",
      consumerNuGetConfig,
      `-p:FlashCapPackageVersion=${corePackage.version}`,
    ],
    isolatedEnvironment,
  );
  run(
    "dotnet",
    [
      "build",
      consumerProject,
      "--configuration",
      "Release",
      "--no-restore",
      `-p:FlashCapPackageVersion=${corePackage.version}`,
    ],
    isolatedEnvironment,
  );
  for (const framework of ["net35", "net40", "net45", "net461", "net48"]) {
    run(
      "mono",
      [
        join(
          dirname(consumerProject),
          "bin",
          "Release",
          framework,
          "FlashCap.PackageConsumer.exe",
        ),
      ],
      isolatedEnvironment,
    );
  }
  run("dotnet", [
    "run",
    "--project",
    verifierProject,
    "--configuration",
    "Release",
    "--",
    join(packageDirectory, corePackage.fileName),
    join(dirname(consumerProject), "obj", "project.assets.json"),
  ]);

  run(
    "dotnet",
    [
      "restore",
      aotProject,
      "--force",
      "--no-cache",
      "--runtime",
      "linux-x64",
      "--configfile",
      consumerNuGetConfig,
      `-p:FlashCapPackageVersion=${corePackage.version}`,
    ],
    isolatedEnvironment,
  );

  for (const framework of ["net8.0", "net10.0"]) {
    const outputDirectory = join(temporaryRoot, "aot", framework);
    run("dotnet", [
      "publish",
      aotProject,
      "--configuration",
      "Release",
      "--framework",
      framework,
      "--runtime",
      "linux-x64",
      "--self-contained",
      "true",
      "--no-restore",
      "-p:PublishAot=true",
      "-p:StripSymbols=true",
      `-p:FlashCapPackageVersion=${corePackage.version}`,
      "--output",
      outputDirectory,
    ], isolatedEnvironment);
    run(join(outputDirectory, "FlashCap.Core.Tests"), []);
  }

  process.stdout.write("\nAll FlashCap verification steps passed.\n");
} finally {
  rmSync(temporaryRoot, { recursive: true, force: true });
}
