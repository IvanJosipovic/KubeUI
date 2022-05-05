using Application = Microsoft.Maui.Controls.Application;

namespace KubeUI.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }
    }
}
