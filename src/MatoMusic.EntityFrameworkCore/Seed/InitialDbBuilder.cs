using System;

namespace MatoMusic.EntityFrameworkCore.Seed
{
    internal class InitialDbBuilder
    {
        private MatoMusicDbContext context;

        public InitialDbBuilder(MatoMusicDbContext context)
        {
            this.context = context;
        }

        internal void Create()
        {
        }
    }
}