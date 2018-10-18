using Daemon.EntityFramework.Core.EntityTracker;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Daemon.EntityFramework.Core
{
    public class DBView<TEntity> : DBTable<TEntity> where TEntity : class
    {
        public override EntityEntry<TEntity> Add(TEntity t)
        {
            throw new InvalidOperationException("You can't do this operation to a view");
        }

        public override List<EntityEntry<TEntity>> AddRange(IEnumerable<TEntity> ts)
        {
            throw new InvalidOperationException("You can't do this operation to a view");
        }

        public override EntityEntry<TEntity> Delete(TEntity t)
        {
            throw new InvalidOperationException("You can't do this operation to a view");
        }

        public override List<EntityEntry<TEntity>> DeleteRange(IEnumerable<TEntity> ts)
        {
            throw new InvalidOperationException("You can't do this operation to a view");
        }

        public override IEnumerator<TEntity> GetEnumerator()
        {
            var result = this.provider.Execute<List<TEntity>>(expression);
            if (result == null)
            {
                yield break;
            }
            foreach (var item in result)
            {
                yield return item;
            }
        }

        public override void SaveChanges()
        {
            throw new InvalidOperationException("You can't do this operation to a view");
        }

        public override EntityEntry<TEntity> Update(TEntity t)
        {
            throw new InvalidOperationException("You can't do this operation to a view");
        }

        public override List<EntityEntry<TEntity>> UpdateRange(IEnumerable<TEntity> ts)
        {
            throw new InvalidOperationException("You can't do this operation to a view");
        }

        protected override PropertyInfo GetPKProperty()
        {
            return null;
        }
    }
}
