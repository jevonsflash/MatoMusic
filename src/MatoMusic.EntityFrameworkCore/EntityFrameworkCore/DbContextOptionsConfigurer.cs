using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace MatoMusic.EntityFrameworkCore
{
    public static class DbContextOptionsConfigurer
    {
        public static void Configure(
            DbContextOptionsBuilder<MatoMusicDbContext> dbContextOptions, 
            string connectionString
            )
        {
            /* This is the single point to configure DbContextOptions for MatoMusicDbContext */
            dbContextOptions.UseSqlite(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<MatoMusicDbContext> builder, DbConnection connection)
        {
            builder.UseSqlite(connection);
        }
    }
}
