using System.Data.Entity;
using Abp.EntityFrameworkCore;
using MatoMusic.Core.MusicSystem;
using Microsoft.EntityFrameworkCore;

namespace MatoMusic.EntityFramework.EntityFramework
{
    public class MatoMusicDbContext : AbpDbContext
    {
        public virtual DbSet<Queue> Queue { get; set; }

        /* NOTE: 
         *   Setting "Default" to base class helps us when working migration commands on Package Manager Console.
         *   But it may cause problems when working Migrate.exe of EF. If you will apply migrations on command line, do not
         *   pass connection string name to base classes. ABP works either way.
         */
        public MatoMusicDbContext()
            : base("Default")
        {

        }

        /* NOTE:
         *   This constructor is used by ABP to pass connection string defined in AbpWpfDemoDataModule.PreInitialize.
         *   Notice that, actually you will not directly create an instance of AbpWpfDemoDbContext since ABP automatically handles it.
         */
        public MatoMusicDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {

        }
    }
}
