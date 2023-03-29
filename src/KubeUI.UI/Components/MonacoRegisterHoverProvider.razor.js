export function registerHoverProvider(languageId, hoverProvider) {
    monaco.languages.registerHoverProvider(languageId, {
        provideHover: function (model, position) {
            const yamlContent = model.getValueInRange({
                startLineNumber: 1,
                endLineNumber: 3,
            });

            return hoverProvider.invokeMethodAsync("ProvideHoverAsync", yamlContent, position);
        }
    });
}