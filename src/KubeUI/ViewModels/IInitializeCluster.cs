using KubeUI.Client;

namespace KubeUI.ViewModels;

public interface IInitializeCluster
{
    void Initialize(ICluster cluster);
}
