using Abp.Modules;
using MatoMusic.Core;
using MatoMusic.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MatoMusic
{
    [DependsOn(typeof(MatoMusicDataModule), typeof(MatoMusicCoreModule))]
    public class MatoMusicModule : AbpModule
    {
        public MatoMusicModule()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

        }
    }
}
