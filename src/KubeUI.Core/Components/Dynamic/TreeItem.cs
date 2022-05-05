﻿using System.Collections.Generic;
using System.ComponentModel;

namespace KubeUI.Core.Components.Dynamic;

public class TreeItem
{
    public string Name { get; set; }

    public object Object { get; set; }

    public string Summary { get; set; }

    public bool IsCollection { get; set; }

    public bool IsCollectionItem { get; set; }

    public bool HideLink { get; set; }

    public bool IsExpanded { get; set; }

    public HashSet<TreeItem> TreeItems { get; set; } = new HashSet<TreeItem>();
}