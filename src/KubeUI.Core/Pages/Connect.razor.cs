#nullable enable
using System.Reflection;
using System;

namespace KubeUI.Core.Pages
{
    public partial class Connect
    {
        string version = "error";

        protected override void OnInitialized()
        {
            var assembly = Type.GetType("KubeUI.Wasm.Startup, KubeUI.Wasm")?.Assembly ?? Type.GetType("KubeUI.WebWindow.Startup, KubeUI.WebWindow")?.Assembly;

            version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }
    }
}
