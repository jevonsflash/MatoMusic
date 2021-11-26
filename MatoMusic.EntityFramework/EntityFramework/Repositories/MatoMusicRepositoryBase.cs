using Abp.Domain.Entities;
using Abp.EntityFramework;
using Abp.EntityFramework.Repositories;
using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Repositories;

namespace MatoMusic.EntityFramework.EntityFramework.Repositories
{
    public abstract class MatoMusicRepositoryBase<TEntity, TPrimaryKey> : EfCoreRepositoryBase<MatoMusicDbContext, TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        protected MatoMusicRepositoryBase(IDbContextProvider<MatoMusicDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }

    public abstract class MatoMusicRepositoryBase<TEntity> : MatoMusicRepositoryBase<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        protected MatoMusicRepositoryBase(IDbContextProvider<MatoMusicDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }
}
