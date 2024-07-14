using System;
using System.Collections.Generic;
namespace KubeUI.ViewModels;

internal interface IInitalizeCluster
{
    void Initialize(Client.Cluster cluster);
}
