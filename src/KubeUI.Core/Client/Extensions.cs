using k8s;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KubeUI.Core.Client
{
    internal static class Extensions
    {
        public static async Task<HttpResponseMessage> SendRequestRaw(this Kubernetes kubernetes, string? requestContent, HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            var type = kubernetes.GetType();
            var method = type.GetMethod("SendRequestRaw", BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(string), typeof(HttpRequestMessage), typeof(CancellationToken) });

            var response = await method.InvokeAsync(kubernetes, new object[] { requestContent, httpRequest, cancellationToken });

            return (HttpResponseMessage)response;
        }

        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            dynamic awaitable = @this.Invoke(obj, parameters);
            await awaitable;
            return awaitable.GetAwaiter().GetResult();
        }

        public static string AddSpacesBeforeCapitals(this string str)
        {
            return Regex.Replace(str, "([a-z])([A-Z])", "$1 $2");
        }
    }
}
