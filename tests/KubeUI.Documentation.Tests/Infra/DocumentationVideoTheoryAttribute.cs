using System.Reflection;
using Avalonia.Headless.XUnit;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace KubeUI.Documentation.Tests.Infra;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(AvaloniaTheoryDiscoverer))]
internal sealed class DocumentationVideoTheoryAttribute : TheoryAttribute, IDataAttribute
{
    public DocumentationVideoTheoryAttribute()
    {
        if (!KubeUIWalkthroughRecorder.IsRecordingEnabled)
        {
            Skip = "Set KUBEUI_DOCS_VIDEO_DIR to record this clip.";
        }
    }

    bool? IDataAttribute.Explicit => null;

    string? IDataAttribute.Label => null;

    string? IDataAttribute.Skip => null;

    Type? IDataAttribute.SkipType => null;

    string? IDataAttribute.SkipUnless => null;

    string? IDataAttribute.SkipWhen => null;

    string? IDataAttribute.TestDisplayName => null;

    int? IDataAttribute.Timeout => null;

    string[]? IDataAttribute.Traits => null;

    public ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        IReadOnlyCollection<ITheoryDataRow> themes =
        [
            new TheoryDataRow<string>("light"),
            new TheoryDataRow<string>("dark"),
        ];
        return ValueTask.FromResult(themes);
    }

    public bool SupportsDiscoveryEnumeration() => true;
}
