using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrino.Development.UI.ViewModels;

public interface IOverlay
{
    void HandleKeyPress(PhysicalKey key, string? symbol, KeyModifiers modifiers);
}
