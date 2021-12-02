using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoMusic.Core.Configuration;
using MatoMusic.Core.Localization;
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
            Configuration.DefaultNameOrConnectionString = configuration.GetConnectionString(MatoMusicConsts.ConnectionStringName);

            base.PreInitialize();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MatoMusicCoreModule).GetAssembly());
        }

        public override async void PostInitialize()
        {
            var musicRelatedViewModel = IocManager.Resolve<MusicRelatedViewModel>();
            await musicRelatedViewModel.init();
            base.PostInitialize();
        }
    }
}
