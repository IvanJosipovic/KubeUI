using System.IO;
using WebWindows.Blazor;

namespace KubeUI.WebWindow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var executingDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            ComponentsDesktop.Run<Startup>("KubeUI", Path.Combine(executingDir, "wwwroot/index.html"));
        }
    }
}
