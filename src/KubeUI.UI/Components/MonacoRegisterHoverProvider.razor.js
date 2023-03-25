import { getDocumentSymbols } from 'monaco-editor/esm/vs/editor/contrib/documentSymbols/documentSymbols';

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