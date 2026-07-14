using System;
using System.Collections.Generic;
using System.Text;

namespace TwfApp.Types.Config
{
    /// <summary>
    /// Struct representing data version
    /// </summary>
    /// <param name="MajorVersion">Major version number. Data across major versions is not compatible with each other</param>
    /// <param name="MinorVersion">Minor version number. Data within same <see cref="DataVersion.MajorVersion"/> is compatible backwards meaning that importing from older version to newer is guaranteed to succeed. It is not guaranteed to work the other way</param>
    public readonly record struct DataVersion(byte MajorVersion, byte MinorVersion)
    {
        public static bool IsCompatible(DataVersion appDataVersion, DataVersion importedDataVersion) => appDataVersion.MajorVersion == importedDataVersion.MajorVersion && appDataVersion.MinorVersion >= importedDataVersion.MinorVersion;
        public override string ToString() => $"{MajorVersion}.{MinorVersion}";
    }
}
