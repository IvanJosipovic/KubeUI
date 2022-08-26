using System.Text.Json.JsonDiffPatch;
using System.Text.Json.JsonDiffPatch.Diffs;

namespace KubeUI.Core.Client
{
    public static class ObjectCompare
    {
        public static JsonDiffDelta CompareObjects(IKubernetesObject<V1ObjectMeta> left, IKubernetesObject<V1ObjectMeta> right)
        {
            var diff = JsonDiffPatcher.Diff(KubernetesJson.Serialize(left), KubernetesJson.Serialize(right), new JsonDiffOptions
            {
                JsonElementComparison = JsonElementComparison.Semantic,
                SuppressDetectArrayMove = true,
                ArrayObjectItemMatchByPosition = true
            });
            return new JsonDiffDelta(diff);
        }
    }
}
