using System.Text.Json.JsonDiffPatch;
using System.Text.Json.JsonDiffPatch.Diffs;

namespace KubeUI.Core.Client
{
    public static class ObjectCompare
    {
        public static JsonDiffDelta CompareObjects(IKubernetesObject<V1ObjectMeta> left, IKubernetesObject<V1ObjectMeta> right)
        {
            //, new JsonPatchDeltaFormatter()
            var diff = JsonDiffPatcher.Diff(KubernetesJson.Serialize(left), KubernetesJson.Serialize(right), new JsonDiffOptions
            {
                JsonElementComparison = JsonElementComparison.RawText
            });
            return new JsonDiffDelta(diff);
        }
    }
}
