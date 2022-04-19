// This is auto-generated version information attributes by RelaxVersioner.2.5.5.0, Do not edit.
// Generated date: Thu, 14 Apr 2022 09:35:36 GMT

using System.Reflection;

[assembly: AssemblyVersion(@"0.14.6")]
[assembly: AssemblyFileVersion(@"2022.4.14.548")]
[assembly: AssemblyInformationalVersion(@"0.14.6-3453c71263c007d91e320a8d88494033b7a5f7e4")]
[assembly: AssemblyMetadata(@"Date",@"Wed, 13 Apr 2022 15:18:17 GMT")]
[assembly: AssemblyMetadata(@"Branch",@"feature/v4l2-interop")]
[assembly: AssemblyMetadata(@"Tags",@"")]
[assembly: AssemblyMetadata(@"Author",@"Kouji Matsui <k@kekyo.net>")]
[assembly: AssemblyMetadata(@"Committer",@"Kouji Matsui <k@kekyo.net>")]
[assembly: AssemblyMetadata(@"Message",@"Updated readme.")]
[assembly: AssemblyMetadata(@"Build",@"")]
[assembly: AssemblyMetadata(@"Generated",@"Thu, 14 Apr 2022 09:35:36 GMT")]
[assembly: AssemblyMetadata(@"TargetFramework",@"net45")]
[assembly: AssemblyMetadata(@"Platform",@"AnyCPU")]
[assembly: AssemblyMetadata(@"BuildOn",@"Unix")]
[assembly: AssemblyMetadata(@"SdkVersion",@"6.0.201")]

namespace FlashCap
{
    internal static class ThisAssembly
    {
        public const string @AssemblyVersion = @"0.14.6";
        public const string @AssemblyFileVersion = @"2022.4.14.548";
        public const string @AssemblyInformationalVersion = @"0.14.6-3453c71263c007d91e320a8d88494033b7a5f7e4";
        public static class AssemblyMetadata
        {
            public const string @Date = @"Wed, 13 Apr 2022 15:18:17 GMT";
            public const string @Branch = @"feature/v4l2-interop";
            public const string @Tags = @"";
            public const string @Author = @"Kouji Matsui <k@kekyo.net>";
            public const string @Committer = @"Kouji Matsui <k@kekyo.net>";
            public const string @Message = @"Updated readme.";
            public const string @Build = @"";
            public const string @Generated = @"Thu, 14 Apr 2022 09:35:36 GMT";
            public const string @TargetFramework = @"net45";
            public const string @Platform = @"AnyCPU";
            public const string @BuildOn = @"Unix";
            public const string @SdkVersion = @"6.0.201";
        }
    }
}

#if NET10 || NET11 || NET20 || NET30 || NET35 || NET40

namespace System.Reflection
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    internal sealed class AssemblyMetadataAttribute : Attribute
    {
        public AssemblyMetadataAttribute(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}

#endif

