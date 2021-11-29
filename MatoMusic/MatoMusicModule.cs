using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoMusic.Core;
using MatoMusic.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MatoMusic
{
    [DependsOn(typeof(MatoMusicEntityFrameworkCoreModule), typeof(MatoMusicCoreModule))]
    public class MatoMusicModule : AbpModule
    {
        public MatoMusicModule()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MatoMusicModule).GetAssembly());
        }
    }
}
