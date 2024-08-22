using KubeUI.Client;

namespace KubeUI.ViewModels;

internal interface IInitializeCluster
{
    void Initialize(ICluster cluster);
}
