using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoMusic.Core;
using MatoMusic.EntityFrameworkCore;
using MatoMusic.ViewModels;

namespace MatoMusic
{
    [DependsOn(typeof(MatoMusicEntityFrameworkCoreModule), typeof(MatoMusicCoreModule))]
    public class MatoMusicModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MatoMusicModule).GetAssembly());
        }

    }
}
