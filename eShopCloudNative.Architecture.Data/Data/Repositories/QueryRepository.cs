using eShopCloudNative.Catalog.Entities;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NH = NHibernate;


//where TEntityBase : IEntity

namespace eShopCloudNative.Architecture.Data.Repositories;

[ExcludeFromCodeCoverage]
public abstract class QueryRepository<TEntityBase>
    where TEntityBase : class, IEntity
{
    protected QueryRepository(ISession session)
    {
        this.Session = session;
    }

    protected ISession Session { get; }

    protected IQueryOver<TEntityBase, TEntityBase> QueryOver => this.Session.QueryOver<TEntityBase>();

    protected IQueryable<TEntityBase> Query => this.Session.Query<TEntityBase>();
    protected ICriteria Criteria => this.Session.CreateCriteria<TEntityBase>();
}
