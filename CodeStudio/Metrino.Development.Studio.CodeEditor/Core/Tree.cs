using System;
using System.Collections.Generic;

namespace Metrino.Development.UI.Core;

public interface ITreeNode : ICloneable
{
    string Name { get; set; }
    object? Data { get; set; }

    ITreeNode? Parent { get; set; }
    IList<ITreeNode> Children { get; set; }

    bool IsLeaf();

    void UpdateChildren();
}
