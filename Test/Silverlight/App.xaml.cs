using System.Diagnostics;
using System.Windows;
using Microsoft.Silverlight.Testing;

namespace PixelLab.Test
{
    public partial class App : Application
    {
        public App()
        {
            this.Startup += this.Application_Startup;
            this.UnhandledException += (sender, args) => Debugger.Break();

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            RootVisual = UnitTestSystem.CreateTestPage();
        }
    }
}