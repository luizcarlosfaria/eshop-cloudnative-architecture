using eShopCloudNative.Catalog.Entities;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NH = NHibernate;

namespace eShopCloudNative.Architecture.Data.Repositories;
public class AsyncPersistenseRepository<TEntityBase> where TEntityBase : IEntity
{

    public AsyncPersistenseRepository(ISession session)
    {
        this.Session = session;
    }

    protected ISession Session { get; }


    public async Task SaveAsync(TEntityBase entity, bool flushImediate = false)
    {
        await this.Session.SaveAsync(entity);
        if (flushImediate) await this.Session.FlushAsync();
    }

    public async Task UpdateAsync(TEntityBase entity, bool flushImediate = false)
    {
        await this.Session.UpdateAsync(entity); 
        if (flushImediate) await this.Session.FlushAsync();
    }

    public async Task DeleteAsync(TEntityBase entity, bool flushImediate = false)
    {
        await this.Session.DeleteAsync(entity);
        if (flushImediate) await this.Session.FlushAsync();
    }

    public async Task Flush() => await this.Session.FlushAsync();
}
