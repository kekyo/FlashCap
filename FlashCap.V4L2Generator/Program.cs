////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FlashCap
{
    public static class Program
    {
        private static readonly char[] separators = new[] { ' ', '\t' };

        private struct SymbolName
        {
            public readonly string Name;
            public readonly bool WithComment;

            public SymbolName(string name, bool withComment = false)
            {
                this.Name = name;
                this.WithComment = withComment;
            }
        }

        private static readonly Dictionary<string, SymbolName> typeAliases =
            new Dictionary<string, SymbolName>()
        {
            { "__u8", new SymbolName("byte") },
            { "__s8", new SymbolName("sbyte") },
            { "__s16", new SymbolName("short") },
            { "__u16", new SymbolName("ushort") },
            { "__s32", new SymbolName("int") },
            { "__u32", new SymbolName("uint") },
            { "__s64", new SymbolName("long") },
            { "__u64", new SymbolName("ulong") },
            { "__le32", new SymbolName("uint") },
            { "unsigned long long", new SymbolName("ulong") },
            { "long long", new SymbolName("long") },
            { "unsigned int", new SymbolName("uint") },
            { "int", new SymbolName("int") },
            { "unsigned short", new SymbolName("ushort") },
            { "short", new SymbolName("short") },
            { "unsigned char", new SymbolName("byte") },
            { "char", new SymbolName("byte", true) },
            { "unsigned long", new SymbolName("UIntPtr", true) },
            { "long", new SymbolName("IntPtr", true) },
        };

        private static readonly HashSet<string> symbolBlackList =
            new HashSet<string>()
        {
            "string", "base",
        };

        private static readonly HashSet<string> symbolExactIncludeList =
            new HashSet<string>()
        {
            "v4l2_buf_type",
            "v4l2_field",
            "v4l2_frmivaltypes",
            "v4l2_frmsizetypes",
            "v4l2_memory",
            "timespec",
            "timeval",
            "v4l2_fmtdesc",
            "v4l2_buf_type",
            "v4l2_frmsizeenum",
            "v4l2_frmsize_stepwise",
            "v4l2_frmsizetypes",
            "v4l2_frmivalenum",
            "v4l2_frmival_stepwise",
            "v4l2_frmivaltypes",
            "v4l2_field",
            "v4l2_buf_type",
            "v4l2_memory",
            "v4l2_buf_type",
            "v4l2_fract",
            "v4l2_frmsize_discrete",
            "v4l2_pix_format",
            "v4l2_format",
            "v4l2_requestbuffers",
            "v4l2_capability",
            "v4l2_pix_format_mplane",
            "v4l2_window",
            "v4l2_vbi_format",
            "v4l2_sliced_vbi_format",
            "v4l2_sdr_format",
            "v4l2_meta_format",
            "v4l2_plane_pix_format",
            "v4l2_rect",
            "v4l2_clip",
            "v4l2_buffer",
            "v4l2_timecode",
            "v4l2_plane",
        };

        private static IEnumerable<KeyValuePair<string, string>> LoadSourceHeader(string headerPath)
        {
            using (var tr = File.OpenText(headerPath))
            {
                while (true)
                {
                    var line = tr.ReadLine()?.Trim();
                    if (line == null)
                    {
                        break;
                    }
                    if (!line.StartsWith("#define"))
                    {
                        continue;
                    }

                    var definition = line.Substring(8).Trim();
                    var keyValues = definition.Split(
                        separators, StringSplitOptions.RemoveEmptyEntries);
                    if (keyValues.Length == 1)
                    {
                        continue;
                    }
                    if (keyValues[0].Contains('('))
                    {
                        continue;
                    }

                    var index = definition.IndexOfAny(separators);
                    var key = definition.Substring(0, index);
                    var value = definition.Substring(index + 1).Trim();

                    yield return new KeyValuePair<string, string>(key, value);
                }
            }
        }

        private static ClangASTJsonRoot LoadClangAstJson(string jsonPath)
        {
            using (var tr = File.OpenText(jsonPath))
            {
                var jr = new JsonTextReader(tr);
                var s = new JsonSerializer();

                return s.Deserialize<ClangASTJsonRoot>(jr);
            }
        }

        private static string LoadVersionLabel()
        {
#if DEBUG
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return "TEST on Windows";
            }
            else
#endif
            {
                using (var tr = File.OpenText("/proc/version"))
                {
                    return tr.ReadToEnd().Replace("\n", "").Trim();
                }
            }
        }

        private static void GenerateStructureDumper(
            string sourceHeaderFileName,
            string clangAstJsonFileName,
            string dumperFileName,
            string architecture,
            string clangVersion,
            string gccVersion,
            string dateTime)
        {
            var versionLabel = LoadVersionLabel();
            
            var definitions = LoadSourceHeader(sourceHeaderFileName).
                OrderBy(entry => entry.Key).
                ToArray();
            
            var root = LoadClangAstJson(clangAstJsonFileName);

            var typedefDecls = root?.inner.
                Where(e => e.kind == "TypedefDecl").
                ToArray() ?? new Inner[0];

            var enumDecls = root?.inner.
                Where(e => e.kind == "EnumDecl").
                ToArray() ?? new Inner[0];

            var recordDecls = root?.inner.
                Where(e => e.kind == "RecordDecl").
                ToArray() ?? new Inner[0];

            var innerUnionDecls = root?.inner.
                SelectMany(inner => inner.
                    TraverseMany(i => i.inner ?? new Inner[0]).
                    Where(e =>
                        e.kind == "RecordDecl" &&
                        e.tagUsed == "union" &&
                        e.completeDefinition &&
                        e.name == null)).
                ToArray() ?? new Inner[0];

            var innerStructDecls = root?.inner.
                SelectMany(inner => inner.
                    TraverseMany(i => i.inner ?? new Inner[0]).
                    Where(e =>
                        e.kind == "RecordDecl" &&
                        e.tagUsed == "struct" &&
                        e.completeDefinition &&
                        e.name == null)).
                ToArray() ?? new Inner[0];

            //////////////////////////////////////////

            using (var tw = File.CreateText(dumperFileName))
            {
                tw.WriteLine($"// This is auto generated code by FlashCap.V4L2Generator. Do not edit.");
                tw.WriteLine($"// {versionLabel}");
                tw.WriteLine($"// {clangVersion.Replace("\r", "").Replace("\n", "")}");
                tw.WriteLine($"// {gccVersion.Replace("\r", "").Replace("\n", "")}");
                tw.WriteLine($"// {dateTime}");
                tw.WriteLine();
                
                tw.WriteLine("#include <stdio.h>");
                tw.WriteLine("#include <stddef.h>");
                tw.WriteLine("#include <linux/videodev2.h>");
                tw.WriteLine();
                tw.WriteLine("template<typename T> unsigned int touint32(T value) { return static_cast<unsigned int>(value); }");
                tw.WriteLine("template<> unsigned int touint32<const char*>(const char* value) { return *reinterpret_cast<const unsigned int*>(value); }");
                tw.WriteLine();
                tw.WriteLine("int main() {");
                tw.WriteLine();

                tw.WriteLine("  printf(\"{\\n\");");
                tw.WriteLine();
                tw.WriteLine("  printf(\"  \\\"label\\\": \\\"{0}\\\",\\n\");", versionLabel);
                tw.WriteLine("  printf(\"  \\\"architecture\\\": \\\"{0}\\\",\\n\");", architecture);
                tw.WriteLine("  printf(\"  \\\"clangVersion\\\": \\\"{0}\\\",\\n\");", clangVersion);
                tw.WriteLine("  printf(\"  \\\"gccVersion\\\": \\\"{0}\\\",\\n\");", gccVersion);
                tw.WriteLine("  printf(\"  \\\"sizeof_size_t\\\": %d,\\n\", (int)sizeof(size_t));");
                tw.WriteLine("  printf(\"  \\\"sizeof_off_t\\\": %d,\\n\", (int)sizeof(off_t));");
                tw.WriteLine();

                // Generate definition information.
                tw.WriteLine("  printf(\"  \\\"definitions\\\": {\\n\");");
                foreach (var definition in definitions)
                {
                    tw.WriteLine("  // #define {0} {1}", definition.Key, definition.Value.TrimEnd('\\'));
                    tw.WriteLine("  printf(\"    \\\"{0}\\\": %u,\\n\", touint32({0}));", definition.Key);
                }
                tw.WriteLine("  printf(\"    \\\"__dummy__\\\": 0\\n\");");
                tw.WriteLine("  printf(\"  },\\n\");");
                tw.WriteLine();

                // Generate enum information.
                tw.WriteLine("  printf(\"  \\\"enums\\\": {\\n\");");
                foreach (var enumDecl in enumDecls)
                {
                    tw.WriteLine("  // {0}: enum {1}", enumDecl.loc, enumDecl.name);
                    tw.WriteLine("  printf(\"    \\\"{0}\\\": {{\\n\");", enumDecl.name);

                    foreach (var inner in enumDecl.inner)
                    {
                        tw.WriteLine("  printf(\"      \\\"{0}\\\": %d,\\n\", (int){0});", inner.name);
                    }
 
                    tw.WriteLine("  printf(\"      \\\"__dummy__\\\": 0\\n\");");
                    tw.WriteLine("  printf(\"    },\\n\");");
                    tw.WriteLine();
                }
                tw.WriteLine("  printf(\"    \\\"__dummy__\\\": 0\\n\");");
                tw.WriteLine("  printf(\"  },\\n\");");
                tw.WriteLine();

                // Generate structure information.
                tw.WriteLine("  printf(\"  \\\"structures\\\": {\\n\");");
                foreach (var recordDecl in recordDecls.
                    Where(recordDecl => !string.IsNullOrWhiteSpace(recordDecl.name)))
                {
                    tw.WriteLine("  // {0}: {1}", recordDecl.loc, recordDecl.name);
                    tw.WriteLine("  printf(\"    \\\"{0}\\\": {{\\n\");", recordDecl.name);
                    tw.WriteLine("  printf(\"      \\\"size\\\": %d,\\n\", (int)sizeof(struct {0}));", recordDecl.name);

                    tw.WriteLine("  printf(\"      \\\"members\\\": {\\n\");");
                        
                    Action<string, IEnumerable<Inner>> dumpInner = null;
                    dumpInner = (baseName, inners) =>
                    {
                        foreach (var inner in inners.Where(i => i.type != null))
                        {
                            var typeName = inner.type.qualType;
                            var typedefDecl = typedefDecls.FirstOrDefault(td => td.name == typeName);
                            if (typedefDecl != null)
                            {
                                typeName = typedefDecl.type.qualType;
                            }

                            if (inner.type.qualType?.StartsWith("union ") ?? false)
                            {
                                // Both implicitly union and named union are collect (maybe name == null)
                                var bn = inner.name != null ? (inner.name + ".") : "";

                                var union = innerUnionDecls.FirstOrDefault(ud => ud.IsIn(inner.loc, inner.range));
                                if (union != null)
                                {
                                    dumpInner(bn, union.inner.Where(i => i.kind == "FieldDecl"));
                                }
                                else
                                {
                                    tw.WriteLine(
                                        "  printf(\"        \\\"{0}\\\": {{ \\\"offset\\\": %d, \\\"type\\\": \\\"{2}\\\" }},\\n\", (int)offsetof(struct {1}, {0}));",
                                        baseName + inner.name, recordDecl.name, typeName.Replace("struct ", ""));
                                }
                            }
                            else if (inner.type.qualType?.StartsWith("struct ") ?? false)
                            {
                                // Both implicitly struct and named struct are collect (maybe name == null)
                                var bn = inner.name != null ? (inner.name + ".") : "";

                                var struct_ = innerStructDecls.FirstOrDefault(sd => sd.IsIn(inner.loc, inner.range));
                                if (struct_ != null)
                                {
                                    dumpInner(bn, struct_.inner.Where(i => i.kind == "FieldDecl"));
                                }
                                else
                                {
                                    tw.WriteLine(
                                        "  printf(\"        \\\"{0}\\\": {{ \\\"offset\\\": %d, \\\"type\\\": \\\"{2}\\\" }},\\n\", (int)offsetof(struct {1}, {0}));",
                                        baseName + inner.name, recordDecl.name, typeName.Replace("struct ", ""));
                                }
                            }
                            else
                            {
                                tw.WriteLine(
                                    "  printf(\"        \\\"{0}\\\": {{ \\\"offset\\\": %d, \\\"type\\\": \\\"{2}\\\" }},\\n\", (int)offsetof(struct {1}, {0}));",
                                    baseName + inner.name, recordDecl.name, typeName.Replace("struct ", ""));
                            }
                        }
                    };


                    dumpInner("", recordDecl.inner.Where(i => i.kind == "FieldDecl"));

                    tw.WriteLine("  printf(\"        \\\"__dummy__\\\": null\\n\");");
                    tw.WriteLine("  printf(\"      }\\n\");");

                    tw.WriteLine("  printf(\"    },\\n\");");
                    tw.WriteLine();
                }

                tw.WriteLine("  printf(\"    \\\"__dummy__\\\": null\\n\");");
                tw.WriteLine("  printf(\"  }\\n\");");
                tw.WriteLine("  printf(\"}\\n\");");

                tw.WriteLine("  return 0;");
                tw.WriteLine("}");

                tw.Flush();
            }
        }
        
        ///////////////////////////////////////////////////////////////////////////////////////

        private static StructureDumpedJsonRoot LoadMembersJson(string jsonPath)
        {
            using (var tr = File.OpenText(jsonPath))
            {
                var jr = new JsonTextReader(tr);
                var s = new JsonSerializer();

                return s.Deserialize<StructureDumpedJsonRoot>(jr);
            }
        }

        private enum FieldTypes
        {
            Other,
            PrimitiveArray,
            Array,
            Pointer,
        }

        private static void GenerateInteropCode(
            string structureDumperJsonFileName,
            string basePath,
            bool isBase,
            string dateTime)
        {
            var root = LoadMembersJson(structureDumperJsonFileName);
            if (root == null)
            {
                return;
            }
            
            var outputSourceFileName = Path.Combine(
                basePath,
                isBase ?
                    "NativeMethods_V4L2_Interop.cs" :
                    "NativeMethods_V4L2_Interop_" + root.Architecture + ".cs");
            using (var tw = File.CreateText(outputSourceFileName))
            {
                tw.WriteLine($"// This is auto generated code by FlashCap.V4L2Generator. Do not edit.");
                tw.WriteLine($"// {root.Label}");
                tw.WriteLine($"// {root.ClangVersion}");
                tw.WriteLine($"// {root.GccVersion}");
                tw.WriteLine($"// {dateTime}");
                tw.WriteLine();

                tw.WriteLine("using System;");
                tw.WriteLine("using System.Runtime.InteropServices;");
                tw.WriteLine();
                
                tw.WriteLine("namespace FlashCap.Internal.V4L2");
                tw.WriteLine("{");
                if (isBase)
                {
                    tw.WriteLine("    internal abstract partial class NativeMethods_V4L2_Interop");
                }
                else
                {
                    tw.WriteLine("    internal sealed class NativeMethods_V4L2_Interop_{0} : NativeMethods_V4L2_Interop", root.Architecture);
                }
                tw.WriteLine("    {");

                tw.WriteLine("        // Common");
                if (isBase)
                {
                    tw.WriteLine("        public abstract string Label { get; }");
                    tw.WriteLine("        public abstract string Architecture { get; }");
                    tw.WriteLine("        public virtual string ClangVersion => throw new NotImplementedException();");
                    tw.WriteLine("        public virtual string GccVersion => throw new NotImplementedException();");
                    tw.WriteLine("        public abstract int sizeof_size_t { get; }");
                    tw.WriteLine("        public abstract int sizeof_off_t { get; }");
                }
                else
                {
                    tw.WriteLine("        public override string Label => \"{0}\";", root.Label);
                    tw.WriteLine("        public override string Architecture => \"{0}\";", root.Architecture);
                    tw.WriteLine("        public override string ClangVersion => \"{0}\";", root.ClangVersion);
                    tw.WriteLine("        public override string GccVersion => \"{0}\";", root.GccVersion);
                    tw.WriteLine("        public override int sizeof_size_t => {0};", root.sizeof_size_t);
                    tw.WriteLine("        public override int sizeof_off_t => {0};", root.sizeof_off_t);
                }
                tw.WriteLine();

                tw.WriteLine("        // Definitions");
                foreach (var definition in root.Definitions.
                    Where(d => symbolExactIncludeList.Contains(d.Key) ||
                       d.Key.StartsWith("V4L2_CAP_") ||
                       d.Key.StartsWith("V4L2_PIX_FMT_") ||
                       d.Key.StartsWith("VIDIOC_")).
                    OrderBy(d => d.Key))
                {
                    if (isBase)
                    {
                        tw.WriteLine("        public virtual uint {0} => throw new NotImplementedException();", definition.Key);
                    }
                    else
                    {
                        tw.WriteLine("        public override uint {0} => {1}U;", definition.Key, definition.Value);
                    }
                }
                tw.WriteLine();

                if (isBase)
                {
                    tw.WriteLine("        // Enums");
                    foreach (var enumDecl in root.Enums.
                         Where(d => symbolExactIncludeList.Contains(d.Key)).
                         OrderBy(d => d.Key))
                    {
                        tw.WriteLine("        public enum {0}", enumDecl.Key);
                        tw.WriteLine("        {");
                        foreach (var entry in enumDecl.Value.
                            Where(d => d.Key != "__dummy__").
                            OrderBy(d => d.Value).
                            MakeShorterSnakeCase(d => d.Key).
                            Select(d => new KeyValuePair<string, int>(d.Value, d.Key.Value)))
                        {
                            tw.WriteLine("            {0} = {1},", entry.Key, entry.Value);
                        }
                        tw.WriteLine("        }");
                        tw.WriteLine();
                    }
                    tw.WriteLine();
                }

                tw.WriteLine("        // Structures");
                foreach (var structureDecl in root.Structures.
                    Where(s => symbolExactIncludeList.Contains(s.Key)).
                    OrderBy(s => s.Key))
                {
                    if (isBase)
                    {
                        tw.WriteLine("        public interface {0}", structureDecl.Key);
                    }
                    else
                    {
                        tw.WriteLine("        [StructLayout(LayoutKind.Explicit, Size={0})]", structureDecl.Value.Size);
                        tw.WriteLine("        private new unsafe struct {0} : NativeMethods_V4L2_Interop.{0}", structureDecl.Key);
                    }
                    tw.WriteLine("        {");

                    foreach (var member in structureDecl.Value.Members.
                        Where(m => m.Key != "__dummy__").
                        OrderBy(m => m.Value.Offset))
                    {
                        var name = member.Key.Replace(".", "_");
                        var typeName = member.Value.Type;
                        var typePrefix = "";
                        var typePostfix = "";
                        var namePostfix = "";
                        var structureTypeName = "";
                        var structureSize = -1;
                        var elementLengths = new int[0];
                        var publicTypeName = typeName;
                        var publicTypePostfix = "";
                        var comment = "";
                        var fieldType = FieldTypes.Other;

                        if (symbolBlackList.Contains(name))
                        {
                            name += "_";
                        }

                        var typeElements = typeName.Split(' ');
                        if (typeElements.Length >= 2)
                        {
                            var lastTypeElement = typeElements.Last();
                            if (lastTypeElement.StartsWith("[") && lastTypeElement.EndsWith("]"))
                            {
                                elementLengths = lastTypeElement.
                                    Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries).
                                    Select(sizeString => int.Parse(sizeString)).
                                    ToArray();

                                typeName = string.Join(" ", typeElements.Take(typeElements.Length - 1));
                                typePrefix = "fixed ";

                                StructureType structureType;
                                if (root.Structures.TryGetValue(typeName, out structureType))
                                {
                                    structureTypeName = typeName;
                                    structureSize = structureType.Size;

                                    namePostfix = string.Format("[{0} * {1}]",
                                        structureType.Size,
                                        string.Join(" * ", elementLengths));

                                    comment = string.Format("   // sizeof({0}): {1}", structureTypeName, structureSize);
                                    publicTypeName = typeName;
                                    typeName = "byte";
                                    fieldType = FieldTypes.Array;
                                }
                                else
                                {
                                    namePostfix = string.Format("[{0}]", string.Join(" * ", elementLengths));
                                    publicTypeName = typeName;
                                    fieldType = FieldTypes.PrimitiveArray;
                                }

                                publicTypePostfix =
                                    string.Join("", Enumerable.Repeat("[]", elementLengths.Length));
                            }
                            else if (typeElements.Last() == "*")
                            {
                                typeName = string.Join(" ", typeElements.Take(typeElements.Length - 1));
                                typePostfix = "*";
                                publicTypeName = "IntPtr";
                                fieldType = FieldTypes.Pointer;
                            }
                        }

                        SymbolName symbolName;
                        if (typeAliases.TryGetValue(typeName, out symbolName))
                        {
                            if (symbolName.WithComment)
                            {
                                comment = "   // " + typeName;
                            }
                            typeName = symbolName.Name;
                            if (fieldType != FieldTypes.Pointer)
                            {
                                publicTypeName = typeName;
                            }
                        }
                        else if (fieldType != FieldTypes.Pointer)
                        {
                            if (!isBase)
                            {
                                publicTypeName = "NativeMethods_V4L2_Interop." + publicTypeName;
                            }
                        }

                        if (isBase)
                        {
                            tw.WriteLine("            {0}{1} {2}",
                                publicTypeName, publicTypePostfix, name);
                            tw.WriteLine("            {");
                            tw.WriteLine("                get;");
                            tw.WriteLine("                set;");
                            tw.WriteLine("            }");
                        }
                        else
                        {
                            tw.WriteLine("            [FieldOffset({0})] private {1}{2}{3} {4}_{5};{6}",
                                member.Value.Offset, typePrefix, typeName, typePostfix, name, namePostfix, comment);
                            tw.WriteLine("            public {0}{1} {2}",
                                publicTypeName, publicTypePostfix, name);
                            tw.WriteLine("            {");

                            switch (fieldType)
                            {
                                case FieldTypes.PrimitiveArray:
                                    tw.WriteLine("                get {{ fixed ({0}* p = this.{1}_) {{ return get(p, {2}); }} }}", typeName, name, string.Join(",", elementLengths));
                                    tw.WriteLine("                set {{ fixed ({0}* p = this.{1}_) {{ set(p, value, {2}); }} }}", typeName, name, string.Join(",", elementLengths));
                                    break;
                                case FieldTypes.Array:
                                    tw.WriteLine("                get {{ fixed ({0}* p = this.{1}_) {{ return get<{2}, NativeMethods_V4L2_Interop.{2}>(p, {3}, {4}); }} }}", typeName, name, structureTypeName, structureSize, string.Join(",", elementLengths));
                                    tw.WriteLine("                set {{ fixed ({0}* p = this.{1}_) {{ set<{2}, NativeMethods_V4L2_Interop.{2}>(p, value, {3}, {4}); }} }}", typeName, name, structureTypeName, structureSize, string.Join(",", elementLengths));
                                    break;
                                case FieldTypes.Pointer:
                                    tw.WriteLine("                get => ({0})this.{1}_;", publicTypeName, name);
                                    tw.WriteLine("                set => this.{0}_ = ({1}*)value.ToPointer();", name, typeName);
                                    break;
                                default:
                                    tw.WriteLine("                get => this.{0}_;", name);
                                    tw.WriteLine("                set => this.{0}_ = ({1})value;", name, typeName);
                                    break;
                            }

                            tw.WriteLine("            }");
                        }
                        tw.WriteLine();
                    }
                    
                    tw.WriteLine("        }");
                    if (isBase)
                    {
                        tw.WriteLine("        public virtual {0} Create_{0}() => throw new NotImplementedException();", structureDecl.Key);
                    }
                    else
                    {
                        tw.WriteLine("        public override NativeMethods_V4L2_Interop.{0} Create_{0}() => new {0}();", structureDecl.Key);
                    }
                    tw.WriteLine();
                }
                tw.WriteLine();

                tw.WriteLine("    }");
                tw.WriteLine("}");
                tw.WriteLine();
            
                tw.Flush();
            }
        }

        public static void Main(string[] args)
        {
            int mode;
            int.TryParse(args[0], out mode);
            switch (mode)
            {
                case 1:
                    Console.Write("  Generating dumper source code ...");
                    GenerateStructureDumper(args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                    Console.WriteLine(" done.");
                    break;
                case 2:
                    Console.Write("  Generating C# source code ...");
                    GenerateInteropCode(args[1], args[2], false, args[3]);
                    Console.WriteLine(" done.");
                    break;
                case 3:
                    Console.Write("  Generating C# base class source code ...");
                    GenerateInteropCode(args[1], args[2], true, args[3]);
                    Console.WriteLine(" done.");
                    break;
            }
        }
    }
}
