using Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MatoMusic.Core.Theme;
using MatoMusic.Core.Models.Entities;

namespace MatoMusic.EntityFrameworkCore
{
    public class MatoMusicDbContext : AbpDbContext
    {
        //Add DbSet properties for your entities...

        public DbSet<Queue> Queue { get; set; }
        public DbSet<Playlist> Playlist { get; set; }
        public DbSet<PlaylistItem> PlaylistItem { get; set; }
        public DbSet<Theme> Theme { get; set; }
        public MatoMusicDbContext(DbContextOptions<MatoMusicDbContext> options) 
            : base(options)
        {

        }
    }
}
