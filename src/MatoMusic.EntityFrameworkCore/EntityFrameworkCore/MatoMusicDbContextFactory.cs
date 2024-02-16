using System;
using System.Diagnostics;
using System.IO;
using MatoMusic.Core;
using MatoMusic.Core.Configuration;
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
            var sqliteFilename = "mato.db";
            string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), sqliteFilename);
            var builder = new DbContextOptionsBuilder<MatoMusicDbContext>();
            var hostFolder = Path.Combine(Environment.CurrentDirectory, "bin", "Debug", "net8.0");

            var configuration = AppConfigurations.Get(hostFolder);
            DbContextOptionsConfigurer.Configure(
                builder,
                configuration.GetConnectionString(MatoMusicConsts.ConnectionStringName)
            );

            return new MatoMusicDbContext(builder.Options);

        }
    }
}