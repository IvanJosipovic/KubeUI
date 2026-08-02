using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

internal static class ResourceReadiness
{
    public static bool IsNotReady(IKubernetesObject<V1ObjectMeta> resource)
    {
        return resource switch
        {
            V1DaemonSet daemonSet => HasNotReadyCondition(daemonSet.Status?.Conditions, condition => condition.Status),
            V1Deployment deployment => HasNotReadyCondition(deployment.Status?.Conditions, condition => condition.Status),
            V1FlowSchema flowSchema => HasNotReadyCondition(flowSchema.Status?.Conditions, condition => condition.Status),
            V1Job job => HasNotReadyCondition(job.Status?.Conditions, condition => condition.Status),
            V1Namespace namespaceResource => HasNotReadyCondition(namespaceResource.Status?.Conditions, condition => condition.Status),
            V1Node node => HasNotReadyCondition(node.Status?.Conditions, condition => condition.Status),
            V1PersistentVolumeClaim persistentVolumeClaim => HasNotReadyCondition(persistentVolumeClaim.Status?.Conditions, condition => condition.Status),
            V1Pod pod => HasNotReadyCondition(pod.Status?.Conditions, condition => condition.Status),
            V1PodDisruptionBudget podDisruptionBudget => HasNotReadyCondition(podDisruptionBudget.Status?.Conditions, condition => condition.Status),
            V1PriorityLevelConfiguration priorityLevelConfiguration => HasNotReadyCondition(priorityLevelConfiguration.Status?.Conditions, condition => condition.Status),
            V1ReplicaSet replicaSet => HasNotReadyCondition(replicaSet.Status?.Conditions, condition => condition.Status),
            V1ReplicationController replicationController => HasNotReadyCondition(replicationController.Status?.Conditions, condition => condition.Status),
            V1Service service => HasNotReadyCondition(service.Status?.Conditions, condition => condition.Status),
            V1ServiceCIDR serviceCidr => HasNotReadyCondition(serviceCidr.Status?.Conditions, condition => condition.Status),
            V1StatefulSet statefulSet => HasNotReadyCondition(statefulSet.Status?.Conditions, condition => condition.Status),
            V1ValidatingAdmissionPolicy validatingAdmissionPolicy => HasNotReadyCondition(validatingAdmissionPolicy.Status?.Conditions, condition => condition.Status),
            V2HorizontalPodAutoscaler horizontalPodAutoscaler => HasNotReadyCondition(horizontalPodAutoscaler.Status?.Conditions, condition => condition.Status),
            V1APIService apiService => HasNotReadyCondition(apiService.Status?.Conditions, condition => condition.Status),
            V1CertificateSigningRequest certificateSigningRequest => HasNotReadyCondition(certificateSigningRequest.Status?.Conditions, condition => condition.Status),
            V1CustomResourceDefinition customResourceDefinition => HasNotReadyCondition(customResourceDefinition.Status?.Conditions, condition => condition.Status),
            _ => IsCustomResourceNotReady(resource),
        };
    }

    private static bool HasNotReadyCondition<T>(IEnumerable<T>? conditions, Func<T, string?> statusSelector)
    {
        return conditions?.Any(condition => string.Equals(statusSelector(condition), "False", StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static bool IsCustomResourceNotReady(object resource)
    {
        var statusProperty = resource.GetType().GetProperty("Status");
        var status = statusProperty?.GetValue(resource);
        var conditionsProperty = status?.GetType().GetProperty("Conditions");
        if (conditionsProperty?.GetValue(status) is not IEnumerable conditions)
        {
            return false;
        }

        foreach (var condition in conditions)
        {
            var conditionStatusProperty = condition?.GetType().GetProperty("Status");
            if (conditionStatusProperty?.GetValue(condition) is string conditionStatus
                && string.Equals(conditionStatus, "False", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
