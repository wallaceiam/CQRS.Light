using CQRS.Light.Contracts;
using CQRS.Light.Core;
using DDD.Light.Realtor.API.Query;
using DDD.Light.Realtor.API.Query.Contract;
using DDD.Light.Realtor.API.Query.Model;
using CQRS.Light.Repository.MongoDB;
using StructureMap;

namespace DDD.Light.Realtor.REST.API.Bootstrap
{
    public static class StructureMapConfig
    {
        public static IContainer ConfigureDependencies()
        {
            var container = DependencyResolution.IoC.Initialize();
            //DependencyResolution.ObjectFactory.Container(x => x.Scan(scan =>
            //    {
            //        scan.TheCallingAssembly();
            //        scan.WithDefaultConventions();
            //    }));

            const string mongoConnectionString = "mongodb://localhost";
            const string realtorReadModel = "DDD_Light_Realtor_ReadModel";

            //IContainer container = DependencyResolution.ObjectFactory.Container;
            container.Configure(x => x.For<IRepository<Listing>>().Use<MongoRepository<Listing>>()
                                      .Ctor<string>("connectionString").Is(mongoConnectionString)
                                      .Ctor<string>("databaseName").Is(realtorReadModel)
                                      .Ctor<string>("collectionName").Is("Listings")
                );
            container.Configure(x => x.For<IActiveListings>().Use<ActiveListings>());
            container.Configure(x => x.For<ICommandBus>().Use(CommandBus.Instance));
            container.Configure(x => x.For<IEventStore>().Use(EventStore.Instance));
            return container;
        }
    }
}