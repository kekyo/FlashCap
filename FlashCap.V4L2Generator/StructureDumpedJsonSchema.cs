////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlashCap
{
    public sealed class StructureMember
    {
        public readonly int Offset;
        public readonly string Type;

        [JsonConstructor]
        public StructureMember(int offset, string type)
        {
            this.Offset = offset;
            this.Type = type;
        }
    }
    
    public sealed class StructureType
    {
        public readonly int Size;
        public readonly IReadOnlyDictionary<string, StructureMember> Members;

        [JsonConstructor]
        public StructureType(
            int size,
            Dictionary<string, StructureMember> members)
        {
            this.Size = size;
            this.Members = members;
        }
    }
    
    public sealed class StructureDumpedJsonRoot
    {
        public readonly string VersionLabel;
        public readonly string Architecture;
        public readonly IReadOnlyDictionary<string, uint> Definitions;
        public readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> Enums;
        public readonly IReadOnlyDictionary<string, StructureType> Structures;

        [JsonConstructor]
        public StructureDumpedJsonRoot(
            string versionLabel, string architecture,
            Dictionary<string, uint> definitions,
            Dictionary<string, JToken> enums,
            Dictionary<string, StructureType> structures)
        {
            this.VersionLabel = versionLabel;
            this.Architecture = architecture;
            this.Definitions = definitions;
            this.Enums = enums.ToDictionary(
                kv => kv.Key,
                kv => (IReadOnlyDictionary<string, int>)kv.Value.
                    OfType<JProperty>().
                    ToDictionary(p => p.Name, p => p.Value.ToObject<int>()));
            this.Structures = structures;
        }
    }
}
