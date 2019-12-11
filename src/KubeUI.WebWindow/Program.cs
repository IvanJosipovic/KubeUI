using WebWindows.Blazor;

namespace KubeUI.WebWindow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ComponentsDesktop.Run<Startup>("KubeUI", "wwwroot/index.html");
        }
    }
}
