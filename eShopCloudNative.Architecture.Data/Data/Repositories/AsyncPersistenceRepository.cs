using eShopCloudNative.Architecture.Entities;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NH = NHibernate;

namespace eShopCloudNative.Architecture.Data.Repositories;
public class AsyncPersistenceRepository<TEntityBase> where TEntityBase : IEntity
{

    public AsyncPersistenceRepository(ISession session)
    {
        this.Session = session;
    }

    protected ISession Session { get; }


    public virtual async Task SaveAsync(TEntityBase entity, bool flushImmediate = false)
    {
        await this.Session.SaveAsync(entity);
        if (flushImmediate) await this.Session.FlushAsync();
    }

    public virtual async Task SaveOrUpdateAsync(TEntityBase entity, bool flushImmediate = false)
    {
        await this.Session.SaveOrUpdateAsync(entity);
        if (flushImmediate) await this.Session.FlushAsync();
    }


    public virtual async Task UpdateAsync(TEntityBase entity, bool flushImmediate = false)
    {
        await this.Session.UpdateAsync(entity); 
        if (flushImmediate) await this.Session.FlushAsync();
    }

    public virtual async Task DeleteAsync(TEntityBase entity, bool flushImmediate = false)
    {
        await this.Session.DeleteAsync(entity);
        if (flushImmediate) await this.Session.FlushAsync();
    }

    public virtual async Task FlushAsync() => await this.Session.FlushAsync();
}
