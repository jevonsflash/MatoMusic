using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Castle.Logging.Log4Net;
using Abp.Dependency;
using Abp.Modules;
using Castle.Facilities.Logging;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace MatoMusic.Core
{

    public static class MatoMusicBuilderExtensions
    {

        public static MauiAppBuilder UseMatoMusic<TStartupModule>(this MauiAppBuilder builder) where TStartupModule : AbpModule
        {
            var _bootstrapper = AbpBootstrapper.Create<TStartupModule>(options =>
            {
                options.IocManager = new IocManager();
            });
            var logCfgName = "log4net.config";
            string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), MatoMusicConsts.LocalizationSourceName, logCfgName);
            _bootstrapper.IocManager.IocContainer.AddFacility<LoggingFacility>(f => f.UseAbpLog4Net().WithConfig(documentsPath));

            builder.Services.AddSingleton(_bootstrapper);
            WindsorRegistrationHelper.CreateServiceProvider(_bootstrapper.IocManager.IocContainer, builder.Services);


            _bootstrapper.Initialize();
            return builder;
        }

    }

}
