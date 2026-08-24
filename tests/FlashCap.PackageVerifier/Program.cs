////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Xml.Linq;

namespace FlashCap.PackageVerifier;

internal static class Program
{
    private static readonly string[] ExpectedFrameworks =
    [
        "net35",
        "net40",
        "net45",
        "net461",
        "net48",
        "netstandard1.3",
        "netstandard2.0",
        "netstandard2.1",
        "netcoreapp2.0",
        "netcoreapp2.1",
        "netcoreapp2.2",
        "netcoreapp3.0",
        "netcoreapp3.1",
        "net5.0",
        "net6.0",
        "net7.0",
        "net8.0",
        "net9.0",
        "net10.0",
    ];

    private static readonly HashSet<string> LegacyMemoryFrameworks =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "net35",
            "net40",
            "net45",
            "net461",
            "net48",
            "netstandard1.3",
            "netstandard2.0",
            "netstandard2.1",
            "netcoreapp2.0",
            "netcoreapp2.1",
            "netcoreapp2.2",
        };

    private static readonly HashSet<string> ModernMarshallingFrameworks =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "net8.0",
            "net9.0",
            "net10.0",
        };

    private static readonly HashSet<string> ForbiddenPackageIds =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Microsoft.Windows.CsWin32",
            "System.Memory",
            "System.Runtime.CompilerServices.Unsafe",
        };

    private static readonly string[] RequiredPublicTypes =
    [
        "FlashCap.Devices.MediaFoundationDevices",
        "FlashCap.Devices.MediaFoundationDeviceDescriptor",
        "FlashCap.Devices.MediaFoundationDevice",
        "FlashCap.Internal.IMFSourceReaderCallbackInterop",
    ];

    private readonly record struct PackageIdentity(string Version);

    private static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine(
                "Usage: FlashCap.PackageVerifier <FlashCap.Core.nupkg> <consumer-project.assets.json>");
            return 2;
        }

        try
        {
            var failures = new List<string>();
            var package = VerifyPackage(Path.GetFullPath(args[0]), failures);
            VerifyConsumerAssets(Path.GetFullPath(args[1]), package, failures);

            if (failures.Count != 0)
            {
                foreach (var failure in failures)
                {
                    Console.Error.WriteLine($"ERROR: {failure}");
                }
                Console.Error.WriteLine($"Package verification failed with {failures.Count} violation(s).");
                return 1;
            }

            Console.WriteLine(
                $"Verified {ExpectedFrameworks.Length} FlashCap.Core package assets and all consumer targets.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"ERROR: Package verification could not be completed: {exception.Message}");
            return 2;
        }
    }

    private static PackageIdentity VerifyPackage(string packagePath, List<string> failures)
    {
        if (!File.Exists(packagePath))
        {
            throw new FileNotFoundException("The package does not exist.", packagePath);
        }

        using var stream = File.OpenRead(packagePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var version = VerifyNuspec(archive, failures);

        var assemblyEntries = archive.Entries
            .Where(entry =>
                entry.FullName.StartsWith("lib/", StringComparison.OrdinalIgnoreCase) &&
                entry.FullName.EndsWith("/FlashCap.Core.dll", StringComparison.OrdinalIgnoreCase))
            .GroupBy(GetFrameworkFromLibraryPath, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase);

        foreach (var framework in ExpectedFrameworks)
        {
            if (!assemblyEntries.TryGetValue(framework, out var entries))
            {
                failures.Add($"The package is missing lib/{framework}/FlashCap.Core.dll.");
                continue;
            }
            if (entries.Length != 1)
            {
                failures.Add(
                    $"The package contains {entries.Length} FlashCap.Core assemblies for {framework}; expected one.");
                continue;
            }

            VerifyAssembly(entries[0], framework, failures);
        }

        foreach (var framework in assemblyEntries.Keys.Except(ExpectedFrameworks, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add($"The package contains an unexpected FlashCap.Core lib asset for {framework}.");
        }

        return new PackageIdentity(version);
    }

    private static string GetFrameworkFromLibraryPath(ZipArchiveEntry entry)
    {
        var segments = entry.FullName.Split('/');
        return segments.Length == 3 ? segments[1] : entry.FullName;
    }

    private static string VerifyNuspec(ZipArchive archive, List<string> failures)
    {
        var nuspecEntries = archive.Entries
            .Where(entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (nuspecEntries.Length != 1)
        {
            throw new InvalidDataException(
                $"The package must contain exactly one nuspec, but contains {nuspecEntries.Length}.");
        }

        using var stream = nuspecEntries[0].Open();
        var document = XDocument.Load(stream, LoadOptions.None);
        var metadata = document.Descendants().FirstOrDefault(
            element => element.Name.LocalName == "metadata") ??
            throw new InvalidDataException("The nuspec does not contain metadata.");
        var packageId = metadata.Elements().FirstOrDefault(
            element => element.Name.LocalName == "id")?.Value;
        var version = metadata.Elements().FirstOrDefault(
            element => element.Name.LocalName == "version")?.Value;
        if (!string.Equals(packageId, "FlashCap.Core", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidDataException("The package identity is not FlashCap.Core with a version.");
        }
        foreach (var dependency in document.Descendants().Where(
                     element => element.Name.LocalName == "dependency"))
        {
            var id = dependency.Attribute("id")?.Value;
            if (id is not null && ForbiddenPackageIds.Contains(id))
            {
                failures.Add($"The nuspec declares the forbidden dependency {id}.");
            }
        }
        return version;
    }

    private static void VerifyAssembly(
        ZipArchiveEntry assemblyEntry,
        string framework,
        List<string> failures)
    {
        using var assemblyImage = new MemoryStream(checked((int)assemblyEntry.Length));
        using (var entryStream = assemblyEntry.Open())
        {
            entryStream.CopyTo(assemblyImage);
        }
        assemblyImage.Position = 0;

        using var peReader = new PEReader(assemblyImage, PEStreamOptions.LeaveOpen);
        if (!peReader.HasMetadata)
        {
            failures.Add($"lib/{framework}/FlashCap.Core.dll does not contain managed metadata.");
            return;
        }

        var metadata = peReader.GetMetadataReader();
        if (!metadata.IsAssembly)
        {
            failures.Add($"lib/{framework}/FlashCap.Core.dll is not an assembly.");
            return;
        }

        VerifyPublicTypes(metadata, framework, failures);
        VerifyAssemblyReferences(metadata, framework, failures);
        if (!ModernMarshallingFrameworks.Contains(framework))
        {
            VerifyLegacyTypeReferences(metadata, framework, failures);
        }
    }

    private static void VerifyPublicTypes(
        MetadataReader metadata,
        string framework,
        List<string> failures)
    {
        var publicTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var handle in metadata.TypeDefinitions)
        {
            var definition = metadata.GetTypeDefinition(handle);
            if ((definition.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.Public)
            {
                continue;
            }

            var typeNamespace = metadata.GetString(definition.Namespace);
            var typeName = metadata.GetString(definition.Name);
            publicTypes.Add(string.IsNullOrEmpty(typeNamespace) ? typeName : $"{typeNamespace}.{typeName}");
        }

        foreach (var requiredType in RequiredPublicTypes)
        {
            if (!publicTypes.Contains(requiredType))
            {
                failures.Add($"lib/{framework}/FlashCap.Core.dll is missing public type {requiredType}.");
            }
        }
    }

    private static void VerifyAssemblyReferences(
        MetadataReader metadata,
        string framework,
        List<string> failures)
    {
        var references = metadata.AssemblyReferences
            .Select(handle => metadata.GetString(metadata.GetAssemblyReference(handle).Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (references.Contains("System.Runtime.CompilerServices.Unsafe"))
        {
            failures.Add(
                $"lib/{framework}/FlashCap.Core.dll references System.Runtime.CompilerServices.Unsafe.");
        }
        if (LegacyMemoryFrameworks.Contains(framework) && references.Contains("System.Memory"))
        {
            failures.Add($"lib/{framework}/FlashCap.Core.dll references System.Memory.");
        }
    }

    private static void VerifyLegacyTypeReferences(
        MetadataReader metadata,
        string framework,
        List<string> failures)
    {
        foreach (var handle in metadata.TypeReferences)
        {
            var reference = metadata.GetTypeReference(handle);
            var typeNamespace = metadata.GetString(reference.Namespace);
            var typeName = metadata.GetString(reference.Name);
            if (typeNamespace.Equals(
                    "System.Runtime.InteropServices.Marshalling",
                    StringComparison.Ordinal) ||
                typeName is "GeneratedComInterfaceAttribute" or "GeneratedComClassAttribute")
            {
                failures.Add(
                    $"lib/{framework}/FlashCap.Core.dll contains legacy-incompatible TypeRef " +
                    $"{typeNamespace}.{typeName}.");
            }
        }
    }

    private static void VerifyConsumerAssets(
        string assetsPath,
        PackageIdentity package,
        List<string> failures)
    {
        if (!File.Exists(assetsPath))
        {
            throw new FileNotFoundException("The consumer assets file does not exist.", assetsPath);
        }

        using var stream = File.OpenRead(assetsPath);
        using var document = JsonDocument.Parse(stream);
        if (!document.RootElement.TryGetProperty("targets", out var targets) ||
            targets.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException("The consumer assets file does not contain a targets object.");
        }

        var baseTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var target in targets.EnumerateObject())
        {
            var baseTarget = target.Name.Split('/', 2)[0];
            baseTargets.Add(baseTarget);

            if (target.Value.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException($"Consumer target {target.Name} is not an object.");
            }

            var hasFlashCapCore = false;
            foreach (var library in target.Value.EnumerateObject())
            {
                var packageId = GetPackageId(library.Name);
                if (packageId.Equals("FlashCap.Core", StringComparison.OrdinalIgnoreCase))
                {
                    hasFlashCapCore = true;
                    var separator = library.Name.LastIndexOf('/');
                    var version = separator < 0 ? string.Empty : library.Name[(separator + 1)..];
                    if (!version.Equals(package.Version, StringComparison.OrdinalIgnoreCase))
                    {
                        failures.Add(
                            $"Consumer target {target.Name} resolved FlashCap.Core {version}; " +
                            $"expected {package.Version}.");
                    }
                }
                if (ForbiddenPackageIds.Contains(packageId))
                {
                    failures.Add(
                        $"Consumer target {target.Name} contains the forbidden package {packageId}.");
                }
            }

            if (!hasFlashCapCore)
            {
                failures.Add($"Consumer target {target.Name} does not contain FlashCap.Core.");
            }
        }

        foreach (var framework in ExpectedFrameworks)
        {
            if (!baseTargets.Contains(framework))
            {
                failures.Add($"The consumer assets file is missing target {framework}.");
            }
        }
    }

    private static string GetPackageId(string libraryName)
    {
        var separator = libraryName.LastIndexOf('/');
        return separator < 0 ? libraryName : libraryName[..separator];
    }
}
