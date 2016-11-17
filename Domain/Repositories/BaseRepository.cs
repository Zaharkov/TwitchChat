using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Domain.Repositories
{
    public class BaseRepository<T> where T: class
    {
        private IDbSet<T> _entities;

        private DbSet<T> Entities
        {
            get
            {
                if (_entities == null)
                {
                    _entities = Context.Set<T>();
                }
                return _entities as DbSet<T>;
            }
        }

        private AppContext Context { get; }

        protected IQueryable<T> Table => Entities;

        protected BaseRepository()
        {
            Context = new AppContext();
        }

        protected T GetById(object id)
        {
            return Entities.Find(id);
        }

        protected void UpdateRange(List<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            Entities.AddOrUpdate(entities.ToArray());
            Context.SaveChanges();
        }

        protected void AddRange(List<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            Entities.AddRange(entities);
            Context.SaveChanges();
        }

        protected void AddOrUpdate(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            Entities.AddOrUpdate(entity);
            Context.SaveChanges();
        }

        protected void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (Context.Entry(entity).State == EntityState.Detached)
                Entities.Attach(entity);

            Entities.Remove(entity);
            Context.SaveChanges();
        }

        protected void DeleteRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            Entities.RemoveRange(entities);
            Context.SaveChanges();
        }

        protected void Sql(string sql)
        {
            Context.Database.ExecuteSqlCommand(sql);
        }
    }
}
