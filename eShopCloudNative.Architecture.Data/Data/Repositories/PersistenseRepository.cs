using eShopCloudNative.Catalog.Entities;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NH = NHibernate;

namespace eShopCloudNative.Architecture.Data.Repositories;
public class PersistenseRepository<TEntityBase> where TEntityBase : IEntity
{

    public PersistenseRepository(ISession session)
    {
        this.Session = session;
    }

    protected ISession Session { get; }


    public async Task SaveAsync(TEntityBase entity)
    {
        await this.Session.SaveAsync(entity);
    }

    public async Task UpdateAsync(TEntityBase entity)
    {
        await this.Session.UpdateAsync(entity);
    }

    public async Task DeleteAsync(TEntityBase entity)
    {
        await this.Session.DeleteAsync(entity);
    }

}
