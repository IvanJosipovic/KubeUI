using Irihi.Lingua;

namespace KubeUI.Foundation.Assets;

/// <summary>
/// The Lingua manager for the shared foundation resource files. This is the only public
/// manager; feature managers stay internal to their feature assemblies.
/// </summary>
[LinguaManager("./Resources.json")]
public sealed partial class FoundationLanguageManager;
