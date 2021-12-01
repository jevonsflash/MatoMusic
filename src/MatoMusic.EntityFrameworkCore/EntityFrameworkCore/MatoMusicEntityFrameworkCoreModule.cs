using System;
using System.Transactions;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Configuration;
using Abp.EntityFrameworkCore.Uow;
using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoMusic.Core;
using MatoMusic.EntityFrameworkCore.Seed;
using Microsoft.EntityFrameworkCore;

namespace MatoMusic.EntityFrameworkCore
{
    [DependsOn(
        typeof(MatoMusicCoreModule), 
        typeof(AbpEntityFrameworkCoreModule))]
    public class MatoMusicEntityFrameworkCoreModule : AbpModule
    {
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; }

        public override void PreInitialize()
        {
            if (!SkipDbContextRegistration)
            {
                Configuration.Modules.AbpEfCore().AddDbContext<MatoMusicDbContext>(options =>
                {
                    if (options.ExistingConnection != null)
                    {
                        DbContextOptionsConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                    }
                    else
                    {
                        DbContextOptionsConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                    }
                });
            }
        }
        public override void Initialize()
        {
 
            IocManager.RegisterAssemblyByConvention(typeof(MatoMusicEntityFrameworkCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {           
            WithDbContext<MatoMusicDbContext>(IocManager, RunMigrate);
            if (!SkipDbSeed)
            {
                SeedHelper.SeedHostDb(IocManager);
            }
        }

        public static void RunMigrate(MatoMusicDbContext dbContext)
        {
            dbContext.Database.Migrate();
        }

        private static void WithDbContext<TDbContext>(IIocResolver iocResolver, Action<TDbContext> contextAction)
            where TDbContext : DbContext
        {
            using (var uowManager = iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
            {
                using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
                {
                    var context = uowManager.Object.Current.GetDbContext<TDbContext>();

                    contextAction(context);

                    uow.Complete();
                }
            }
        }

    }
}