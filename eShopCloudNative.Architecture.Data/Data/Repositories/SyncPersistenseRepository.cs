using eShopCloudNative.Catalog.Entities;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NH = NHibernate;

namespace eShopCloudNative.Architecture.Data.Repositories;
public class SyncPersistenseRepository<TEntityBase> where TEntityBase : IEntity
{

    public SyncPersistenseRepository(ISession session)
    {
        this.Session = session;
    }

    protected ISession Session { get; }


    public virtual void Save(TEntityBase entity, bool flushImediate = false)
    {
        this.Session.Save(entity);
        if (flushImediate) this.Session.Flush();
    }

    public virtual void Update(TEntityBase entity, bool flushImediate = false)
    {
        this.Session.Update(entity);
        if (flushImediate) this.Session.Flush();
    }

    public virtual void Delete(TEntityBase entity, bool flushImediate = false)
    {
        this.Session.Delete(entity);
        if (flushImediate) this.Session.Flush();
    }

    public virtual void Flush() => this.Session.Flush();
}
