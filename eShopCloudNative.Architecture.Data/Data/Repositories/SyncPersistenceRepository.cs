using eShopCloudNative.Architecture.Entities;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NH = NHibernate;

namespace eShopCloudNative.Architecture.Data.Repositories;
public class SyncPersistenceRepository<TEntityBase> where TEntityBase : IEntity
{

    public SyncPersistenceRepository(ISession session)
    {
        this.Session = session;
    }

    protected ISession Session { get; }


    public virtual void Save(TEntityBase entity, bool flushImmediate = false)
    {
        this.Session.Save(entity);
        if (flushImmediate) this.Session.Flush();
    }

    public virtual void SaveOrUpdate(TEntityBase entity, bool flushImmediate = false)
    {
        this.Session.SaveOrUpdate(entity);
        if (flushImmediate) this.Session.Flush();
    }

    public virtual void Update(TEntityBase entity, bool flushImmediate = false)
    {
        this.Session.Update(entity);
        if (flushImmediate) this.Session.Flush();
    }

    public virtual void Delete(TEntityBase entity, bool flushImmediate = false)
    {
        this.Session.Delete(entity);
        if (flushImmediate) this.Session.Flush();
    }

    public virtual void Flush() => this.Session.Flush();
}
