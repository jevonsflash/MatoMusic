using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoMusic.Core.Configuration;
using MatoMusic.Core.Localization;
using MatoMusic.Core.Services;
using MatoMusic.Core.Settings;
using MatoMusic.Core.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MatoMusic.Core
{
    [DependsOn(
       typeof(AbpAutoMapperModule))]
    public class MatoMusicCoreModule : AbpModule
    {
        private readonly string development;

        public MatoMusicCoreModule()
        {
            development = EnvironmentName.Development;

        }
        public override void PreInitialize()
        {
            LocalizationConfigurer.Configure(Configuration.Localization);

            Configuration.Settings.Providers.Add<CommonSettingProvider>();

            string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), MatoMusicConsts.LocalizationSourceName);

            var configuration = AppConfigurations.Get(documentsPath, development);
            var connectionString = configuration.GetConnectionString(MatoMusicConsts.ConnectionStringName);

            var dbName = "mato.db";
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), MatoMusicConsts.LocalizationSourceName, dbName);

            Configuration.DefaultNameOrConnectionString = String.Format(connectionString, dbPath);
            base.PreInitialize();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MatoMusicCoreModule).GetAssembly());
        }
    }
}
