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
    }
}
