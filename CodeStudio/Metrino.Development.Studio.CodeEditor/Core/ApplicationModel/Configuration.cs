using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Metrino.Development.UI.Core;

public partial class Configuration : ObservableObject
{
    [ObservableProperty]
    string _otdrRepositoryPath = string.Empty;

    [ObservableProperty]
    string _otdrBuildPath = string.Empty;

    [ObservableProperty]
    string _otdrBuildConfiguration = string.Empty;

    [ObservableProperty]
    string _otdrTargetFramework = string.Empty;

    [ObservableProperty]
    string _localTestDataBank = string.Empty;

    [ObservableProperty]
    string _remoteTestDataBank = string.Empty;

    [ObservableProperty]
    string _unitTestXmlRoot = string.Empty;

    [ObservableProperty]
    string _localTestResult = string.Empty;

    public List<string> WorkspaceFolders { get; set; } = new List<string>();

    public void ReadFrom(string configurationPath)
    {
        using (var reader = new StreamReader(configurationPath))
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var config = deserializer.Deserialize<Configuration>(reader);

            OtdrBuildPath = config.OtdrBuildPath;
            OtdrBuildConfiguration = config.OtdrBuildConfiguration;
            OtdrRepositoryPath = config.OtdrRepositoryPath;
            OtdrTargetFramework = config.OtdrTargetFramework;

            LocalTestDataBank = config.LocalTestDataBank;
            RemoteTestDataBank = config.RemoteTestDataBank;
            UnitTestXmlRoot = config.UnitTestXmlRoot;

            WorkspaceFolders = config.WorkspaceFolders;
        }
    }

    public void WriteTo(string configurationPath)
    {
        var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        var yaml = serializer.Serialize(this);

        using (var writer = new StreamWriter(configurationPath))
        {
            writer.Write(yaml);
        }
    }
}
