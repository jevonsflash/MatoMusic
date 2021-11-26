using System.Reflection;
using Abp.EntityFramework;
using Abp.EntityFrameworkCore;
using Abp.Modules;
using MatoMusic.Core;
using MatoMusic.EntityFramework.EntityFramework;
using Microsoft.EntityFrameworkCore.Storage;

namespace MatoMusic.EntityFramework
{
    [DependsOn(typeof(AbpEntityFrameworkCoreModule), typeof(MatoMusicCoreModule))]
    public class MatoMusicDataModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = "Default";
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
            Database.SetInitializer<MatoMusicDbContext>(null);
        }
    }
}
