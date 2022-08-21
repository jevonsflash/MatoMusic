using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoMusic.Core;
using MatoMusic.Core.Services;
using MatoMusic.EntityFrameworkCore;
using MatoMusic.ViewModels;

namespace MatoMusic
{
    [DependsOn(
        typeof(MatoMusicCoreModule),
        typeof(MatoMusicEntityFrameworkCoreModule))]
    public class MatoMusicModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MatoMusicModule).GetAssembly());
        }

    }
}
