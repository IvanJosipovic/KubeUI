using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Documentation.Tests.Infra;
using k8s.Models;

namespace KubeUI.Documentation.Tests.Features.Resources.Yaml;

public sealed class RecordingTests
{
    [DocumentationVideoTheory]
    public Task Inspect_pod_yaml_video(string theme)
    {
        return KubeUIWalkthrough.Create(theme, "inspect-pod-yaml")
            .Intro(
                "Edit and validate YAML",
                "In this walkthrough, I'll edit a Pod manifest, use schema completion, and validate it with the cluster.")
            .LoadView<ResourceYamlView, ResourceYamlViewModel>(x =>
            {
                x.ViewModel.Initialize(x.Workspace, new V1Pod
                {
                    ApiVersion = "v1",
                    Kind = V1Pod.KubeKind,
                    Metadata = new V1ObjectMeta
                    {
                        Name = "temp",
                        NamespaceProperty = "default",
                    },
                });
            })
            .Speak("This new Pod has a name and namespace, but no spec yet. I'll switch the editor into edit mode before adding the rest.")
            .ClickYamlEditMode()
            .MoveCursorBelowYamlMetadata()
            .Speak("The insertion point is below metadata. I can press Ctrl+Space, move through the schema suggestions, and press Enter to add a field.")
            .TypeYamlText("\n")
            .ExploreYamlCompletion("spec")
            .ExploreYamlCompletion("restartPolicy")
            .ExploreYamlCompletion("Always")
            .TypeYamlText("\n  ")
            .Speak("Now I'll add a container and choose each key from completion. I only need to type the image and field values.")
            .ExploreYamlCompletion("containers")
            .ExploreYamlCompletion("name")
            .TypeYamlText("web\n      ")
            .ExploreYamlCompletion("image")
            .TypeYamlText("nginx:1.27\n      ")
            .ExploreYamlCompletion("ports")
            .ExploreYamlCompletion("containerPort")
            .TypeYamlText("80\n      ")
            .Speak("A readiness probe tells Kubernetes when this container can receive traffic. I'll select its keys from completion, then enter the path and port values.")
            .ExploreYamlCompletion("readinessProbe")
            .ExploreYamlCompletion("httpGet")
            .ExploreYamlCompletion("path")
            .TypeYamlText("/healthz\n          ")
            .ExploreYamlCompletion("port")
            .TypeYamlText("80\n      ")
            .Speak("I can explore deeper settings in the same way. Under the security context, I'll choose a seccomp profile and select RuntimeDefault.")
            .ExploreYamlCompletion("securityContext")
            .ExploreYamlCompletion("seccompProfile")
            .ExploreYamlCompletion("type")
            .ExploreYamlCompletion("RuntimeDefault")
            .TypeYamlText("\n      ")
            .Speak("I'll add an image pull policy. OnFailure sounds reasonable, but it is not one of the values this field accepts.")
            .ExploreYamlCompletion("imagePullPolicy")
            .TypeYamlText("OnFailure")
            .Speak("A server dry-run checks the finished Pod without saving it. The cluster flags the invalid value, and the editor underlines the exact spot.")
            .DryRunYaml()
            .Speak("Hovering over imagePullPolicy gives me context for the field. To see the values I can use, I'll open completion after clearing the mistake.")
            .HoverYamlHeader("imagePullPolicy")
            .Speak("I remove OnFailure, open completion, and choose Always. With the value corrected, I can save the Pod.")
            .BackspaceYamlText("OnFailure")
            .ExploreYamlCompletion("Always")
            .SaveYaml()
            .RecordAsync();
    }
}
