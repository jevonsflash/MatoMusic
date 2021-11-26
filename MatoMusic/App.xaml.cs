using Abp;
using Abp.Castle.Logging.Log4Net;
using Castle.Facilities.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace MatoMusic
{
	public partial class App : Application
	{
        private readonly AbpBootstrapper _bootstrapper;
		public App()
		{
			_bootstrapper = AbpBootstrapper.Create<MatoMusicModule>();
			_bootstrapper.IocManager.IocContainer.AddFacility<LoggingFacility>(f => f.UseAbpLog4Net().WithConfig("log4net.config"));
			InitializeComponent();
			MainPage = new MainPage();
		}
    }
}
