using System.Collections.Generic;
using System.IO;

namespace Metrino.Development.Builder;

public class PackageReferences
{
    static string NugetPackageRoot = @"D:\Work\.nuget\packages";
    static string ExfoOpticalPlatformPackageRoot = Path.Combine(NugetPackageRoot, "exfo.optical.platform/1.6.19091/lib/net40");

    public static NugetPackageReference[] NugetPackageReferences =
    {
        new NugetPackageReference { 
            Include="Newtonsoft.Json", 
            Version = "13.0.3"
        },

        new NugetPackageReference { 
            Include="YamlDotNet", 
            Version = "13.1.1"
        },

        new NugetPackageReference { 
            Include="CommandLineParser", 
            Version = "2.9.1"
        },

        new NugetPackageReference { 
            Include="EPPlus", 
            Version = "6.2.6"
        },

        new NugetPackageReference { 
            Include="Microsoft.Extensions.Primitives", 
            Version = "3.1.4"
        },

        new NugetPackageReference { 
            Include="Microsoft.Extensions.Configuration",
            Version = "3.1.4"
        },

        new NugetPackageReference { 
            Include="MSTest.TestFramework", 
            Version = "3.0.4"
        },

        new NugetPackageReference { 
            Include="System.Diagnostics.PerformanceCounter", 
            Version = "7.0.0"
        },

        new NugetPackageReference { 
            Include="NLua", 
            Version = "1.6.3"
        },

        new NugetPackageReference {
            Include="StreamJsonRpc",
            Version = "2.17.8"
        },
    };

    public static IEnumerable<DllReference> DirectDllReferences(string metrinoBuildOutputPath)
    {
        return new DllReference[] {
            new DllReference { 
                Include = "Metrino.Otdr", 
                DllPath = Path.Combine(metrinoBuildOutputPath, "Metrino.Otdr/Metrino.Otdr.dll") 
            },

            new DllReference { 
                Include = "Metrino.Otdr.SignalProcessing", 
                DllPath = Path.Combine(metrinoBuildOutputPath, "Metrino.Otdr.SignalProcessing/Metrino.Otdr.SignalProcessing.dll") 
            },

            new DllReference { 
                Include = "Metrino.Otdr.Simulation", 
                DllPath = Path.Combine(metrinoBuildOutputPath, "Metrino.Otdr.Simulation/Metrino.Otdr.Simulation.dll") 
            },

            new DllReference { 
                Include = "Metrino.Otdr.Instrument", 
                DllPath = Path.Combine(metrinoBuildOutputPath, "Metrino.Otdr.Instrument/Metrino.Otdr.Instrument.dll") 
            },

            new DllReference { 
                Include = "Metrino.Otdr.FileConverter", 
                DllPath = Path.Combine(metrinoBuildOutputPath, "Metrino.Otdr.FileConverter/Metrino.Otdr.FileConverter.dll") 
            },

            new DllReference { 
                Include = "Metrino.Olm", 
                DllPath = Path.Combine(metrinoBuildOutputPath, "Metrino.Olm/Metrino.Olm.dll") 
            },

            new DllReference { 
                Include = "Metrino.Olm.Instrument", 
                DllPath = Path.Combine(metrinoBuildOutputPath, "Metrino.Olm.Instrument/Metrino.Olm.Instrument.dll") 
            },

            new DllReference { 
                Include = "Metrino.Olm.SignalProcessing", 
                DllPath = Path.Combine(metrinoBuildOutputPath, "Metrino.Olm.SignalProcessing/Metrino.Olm.SignalProcessing.dll") 
            },

            new DllReference { 
                Include = "UnitTestOlm", 
                DllPath = Path.Combine(metrinoBuildOutputPath, "UnitTestOlm/UnitTestOlm.dll") 
            },

            new DllReference { 
                Include = "Metrino.Kernos", 
                DllPath = Path.Combine( ExfoOpticalPlatformPackageRoot, "Metrino.Kernos.dll")
            },

            new DllReference { 
                Include = "Metrino.Kernos.Instrument", 
                DllPath = Path.Combine( ExfoOpticalPlatformPackageRoot, "Metrino.Kernos.Instrument.dll")
            },

            new DllReference { 
                Include = "Metrino.Kernos.IO.ExfoBus", 
                DllPath = Path.Combine( ExfoOpticalPlatformPackageRoot, "Metrino.Kernos.IO.ExfoBus.dll")
            },

            new DllReference { 
                Include = "Metrino.Kernos.Platform", 
                DllPath = Path.Combine( ExfoOpticalPlatformPackageRoot, "Metrino.Kernos.Platform.dll")
            },

            new DllReference { 
                Include = "Metrino.Kernos.Windows.Forms", 
                DllPath = Path.Combine( ExfoOpticalPlatformPackageRoot, "Metrino.Kernos.Windows.Forms.dll")
            },

            new DllReference { 
                Include = "Metrino.Platform.Client", 
                DllPath = Path.Combine( ExfoOpticalPlatformPackageRoot, "Metrino.Platform.Client.dll")
            },

            new DllReference { 
                Include = "Metrino.Platform.Definitions", 
                DllPath = Path.Combine( ExfoOpticalPlatformPackageRoot, "Metrino.Platform.Definitions.dll")
            },

            new DllReference { 
                Include = "Metrino.EepromSections",
                DllPath = Path.Combine( ExfoOpticalPlatformPackageRoot, "Metrino.EepromSections.dll")
            },

            new DllReference { 
                Include = "MongoDB.Bson", 
                DllPath = Path.Combine( NugetPackageRoot, "mongocsharpdriver/1.9.2/lib/net35/MongoDB.Bson.dll")
            },

            new DllReference { 
                Include = "MongoDB.Driver", 
                DllPath = Path.Combine( NugetPackageRoot, "mongocsharpdriver/1.9.2/lib/net35/MongoDB.Driver.dll")
            },

            new DllReference { 
                Include = "Gat.Controls.OpenDialog", 
                DllPath = Path.Combine( NugetPackageRoot, "opendialog/1.1.1/lib/net45/Gat.Controls.OpenDialog.dll")
            },

            new DllReference { Include = "Microsoft.CSharp" },
            new DllReference { Include = "PresentationCore" },
            new DllReference { Include = "PresentationFramework" },
            new DllReference { Include = "System" },
            new DllReference { Include = "System.Configuration" },
            new DllReference { Include = "System.Core" },
            new DllReference { Include = "System.Data" },
            new DllReference { Include = "System.Data.DataSetExtensions" },
            new DllReference { Include = "System.Management" },
            new DllReference { Include = "System.Numerics" },
            new DllReference { Include = "System.ServiceProcess" },
            new DllReference { Include = "System.ServiceModel" },
            new DllReference { Include = "System.ServiceModel.Primitives" },
            new DllReference { Include = "System.Windows" },
            new DllReference { Include = "System.Windows.Forms" },
            new DllReference { Include = "System.Xml" },
            new DllReference { Include = "System.Xml.Serialization" },
            new DllReference { Include = "System.Xml.Linq" },
            new DllReference { Include = "System.Windows.Forms" },
            new DllReference { Include = "System.Xaml" },
            new DllReference { Include = "System.Xml" },
            new DllReference { Include = "System.Xml.Linq" },
            new DllReference { Include = "WindowsBase" },
        };
    }


}
