using System;
using System.Diagnostics;
using System.IO;
using MatoMusic.Configuration;
using MatoMusic.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MatoMusic.EntityFrameworkCore
{
    /* This class is needed to run EF Core PMC commands. Not used anywhere else */
    public class MatoMusicDbContextFactory : IDesignTimeDbContextFactory<MatoMusicDbContext>
    {
        public MatoMusicDbContext CreateDbContext(string[] args)
        {
            //Debugger.Launch();
            //var sqliteFilename = "mato.db";
            //string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), sqliteFilename);
            //var builder = new DbContextOptionsBuilder<MatoMusicDbContext>();
            //var hostFolder = Path.Combine(Environment.CurrentDirectory, "bin", "Debug", "net6.0");

            //Debug.WriteLine("+++++++++" + hostFolder);
            //var configuration = AppConfigurations.Get(hostFolder);
            //Debug.WriteLine(configuration.GetConnectionString(MatoMusicConsts.ConnectionStringName));
            //DbContextOptionsConfigurer.Configure(
            //    builder,
            //    configuration.GetConnectionString(MatoMusicConsts.ConnectionStringName)
            //);

            //return new MatoMusicDbContext(builder.Options);
            var connectionString = @"Data Source=mato.db";
            var contextOptions = new DbContextOptionsBuilder<MatoMusicDbContext>()
                .UseSqlite(connectionString)
                .Options;

            return new MatoMusicDbContext(contextOptions);
        }
    }
}