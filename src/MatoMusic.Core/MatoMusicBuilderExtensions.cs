using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Castle.Logging.Log4Net;
using Abp.Dependency;
using Abp.Modules;
using Castle.Facilities.Logging;
using Castle.Windsor.MsDependencyInjection;
using MatoMusic.Infrastructure.Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace MatoMusic.Core
{

    public static class MatoMusicBuilderExtensions
    {

        public static MauiAppBuilder UseMatoMusic<TStartupModule>(this MauiAppBuilder builder) where TStartupModule : AbpModule
        {
            var logCfgName = "log4net.config";
            var appCfgName = "appsettings.json";

            string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), MatoMusicConsts.LocalizationSourceName, logCfgName);
            string documentsPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), MatoMusicConsts.LocalizationSourceName, appCfgName);

            InitConfig(logCfgName, documentsPath);
            InitConfig(appCfgName, documentsPath2);

            var _bootstrapper = AbpBootstrapper.Create<TStartupModule>(options =>
            {
                options.IocManager = new IocManager();
            });
            _bootstrapper.IocManager.IocContainer.AddFacility<LoggingFacility>(f => f.UseAbpLog4Net().WithConfig(documentsPath));

            builder.Services.AddSingleton(_bootstrapper);
            WindsorRegistrationHelper.CreateServiceProvider(_bootstrapper.IocManager.IocContainer, builder.Services);

            return builder;
        }

        private static void InitConfig(string logCfgName, string documentsPath)
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MatoMusicBuilderExtensions)).Assembly;

            Stream stream = assembly.GetManifestResourceStream($"MatoMusic.Core.{logCfgName}");
            string text = "";
            using (var reader = new System.IO.StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }
            DirFileHelper.CreateFile(documentsPath, text);
        }

        private static void InitDataBase(string documentsPath)
        {
        }
    }

}
