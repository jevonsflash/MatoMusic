using Abp.Modules;
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
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

        }
    }
}
