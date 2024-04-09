using System;
using System.Threading.Tasks;

namespace Metrino.Development.UI.ViewModels;

public interface IDocument: IDisposable
{
    Task Save();
    Task SaveAs(string fileName);
    Task Run(string branch);
    Task Info();

    bool HandleKeySequence(string keySequence);
}
