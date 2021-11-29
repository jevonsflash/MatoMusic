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
            var sqliteFilename = "MatoPlayerDB.db3";
            string documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), sqliteFilename);
            var path = Path.Combine(documentsPath, sqliteFilename);
            var builder = new DbContextOptionsBuilder<MatoMusicDbContext>();
            Debug.WriteLine("+++++++++"+documentsPath);
            var configuration = AppConfigurations.Get(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            DbContextOptionsConfigurer.Configure(
                builder,
                configuration.GetConnectionString(MatoMusicConsts.ConnectionStringName)
            );

            return new MatoMusicDbContext(builder.Options);
        }
    }
}