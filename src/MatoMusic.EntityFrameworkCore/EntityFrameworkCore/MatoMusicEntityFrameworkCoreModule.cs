using Abp.EntityFrameworkCore;
using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoMusic.Core;

namespace MatoMusic.EntityFrameworkCore
{
    [DependsOn(
        typeof(MatoMusicCoreModule), 
        typeof(AbpEntityFrameworkCoreModule))]
    public class MatoMusicEntityFrameworkCoreModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MatoMusicEntityFrameworkCoreModule).GetAssembly());
        }
    }
}