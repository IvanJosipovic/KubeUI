using Irihi.Lingua;

namespace KubeUI.App.Tests.Foundation.Localization;

/// <summary>
/// Lingua manager fixture for localization tests. Source files live next to this declaration
/// because the <see cref="LinguaManagerAttribute"/> path is resolved relative to it.
/// </summary>
[LinguaManager("./TestLocalizationResources.json")]
internal sealed partial class TestLanguageManager;
