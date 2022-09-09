using System.Text.Json.JsonDiffPatch;
using System.Text.Json.JsonDiffPatch.Diffs;

namespace KubeUI.Core.Client
{
    public static class ObjectCompare
    {
        public static JsonDiffDelta CompareObjects(IKubernetesObject<V1ObjectMeta> left, IKubernetesObject<V1ObjectMeta> right)
        {
            var leftJson = KubernetesJson.Serialize(CleanObject(left)); ;
            var rightJson = KubernetesJson.Serialize(CleanObject(right));

            var diff = JsonDiffPatcher.Diff(leftJson, rightJson, new JsonDiffOptions
            {
                JsonElementComparison = JsonElementComparison.Semantic,
                SuppressDetectArrayMove = true,
                ArrayObjectItemMatchByPosition = true
            });
            return new JsonDiffDelta(diff);
        }

        public static IKubernetesObject<V1ObjectMeta> CleanObject(IKubernetesObject<V1ObjectMeta> kubeObject)
        {
            if (kubeObject.Metadata != null)
            {
                kubeObject.Metadata.Generation = null;
                kubeObject.Metadata.CreationTimestamp = null;
                kubeObject.Metadata.Finalizers = null;
                kubeObject.Metadata.ManagedFields = null;
                kubeObject.Metadata.ResourceVersion = null;
                kubeObject.Metadata.SelfLink = null;
                kubeObject.Metadata.Uid = null;
            }

            var prop = kubeObject.GetType().GetProperty(nameof(V1Deployment.Status));

            if (prop != null)
            {
                prop.SetValue(kubeObject, null);
            }

            return kubeObject;
        }
    }
}
