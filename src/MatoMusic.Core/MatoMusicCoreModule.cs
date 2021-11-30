using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoMusic.Core.Localization;
using MatoMusic.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MatoMusic.Core
{
    public class MatoMusicCoreModule : AbpModule
    {
        public MatoMusicCoreModule()
        {

        }
        public override void PreInitialize()
        {
           LocalizationConfigurer.Configure(Configuration.Localization);

            Configuration.Settings.Providers.Add<CommonSettingProvider>();
            base.PreInitialize();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MatoMusicCoreModule).GetAssembly());
        }
    }
}
