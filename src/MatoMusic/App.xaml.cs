using System;
using System.IO;
using Abp;
using Abp.Castle.Logging.Log4Net;
using Castle.Facilities.Logging;
using MatoMusic.Core;
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
			var logCfgName = "log4net.config";
			_bootstrapper = AbpBootstrapper.Create<MatoMusicModule>();
			string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),MatoMusicConsts.LocalizationSourceName, logCfgName);

			_bootstrapper.IocManager.IocContainer.AddFacility<LoggingFacility>(f => f.UseAbpLog4Net().WithConfig(documentsPath));
			InitializeComponent();
			MainPage = new MainPage();
		}
    }
}
