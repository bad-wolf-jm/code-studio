using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace Metrino.Development.Studio.Library.ViewModels;

public class Utilities
{
    public static async Task<string> SelectFolder(TopLevel? toplevel)
    {
        if (toplevel == null)
            return string.Empty;

        var options = new FolderPickerOpenOptions { Title = "Add folder to workspace..." };

        var x = await toplevel.StorageProvider.OpenFolderPickerAsync(options);

        if (x.Count() > 0)
            return x.ElementAt(0).TryGetLocalPath() ?? string.Empty;

        return string.Empty;
    }

    public static async Task<string> SelectFile(TopLevel? toplevel)
    {
        if (toplevel == null)
            return string.Empty;

        var options = new FilePickerOpenOptions { Title = "Select file..." };

        var x = await toplevel.StorageProvider.OpenFilePickerAsync(options);

        if (x.Count() > 0)
            return x.ElementAt(0).TryGetLocalPath() ?? string.Empty;

        return string.Empty;
    }

    public static async Task<string> SelectFileName(TopLevel? toplevel)
    {
        if (toplevel == null)
            return string.Empty;

        var options = new FilePickerSaveOptions { Title = "Select file..." };

        var x = await toplevel.StorageProvider.SaveFilePickerAsync(options);

        return x.TryGetLocalPath() ?? string.Empty;
    }
}
