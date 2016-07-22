using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using DDD.Light.Core;
using DDD.Light.CQRS;
using DDD.Light.Contracts.EventStore;
using DDD.Light.Realtor.API.Command.Realtor;
using DDD.Light.Realtor.REST.API.Bootstrap;
using DDD.Light.Repo.MongoDB;
using StructureMap;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.REST.API
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801


    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            SetUpIoC();

            EventStore.Instance.Configure(new MongoRepository<AggregateEvent>("mongodb://localhost", "DDD_Light_Realtor", "EventStore"), new JsonEventSerializationStrategy());
            EventBus.Instance.Configure(EventStore.Instance, new JsonEventSerializationStrategy(), false);

            AggregateCache.Instance.Configure(EventStore.Instance, DependencyResolution.ObjectFactory.Container.GetInstance);
            AggregateBus.Instance.Configure(EventBus.Instance, AggregateCache.Instance);

            InitApp(EventStore.Instance);
        }

        private static void InitApp(IEventStore eventStore)
        {
            HandlerSubscribtions.SubscribeAllHandlers(DependencyResolution.ObjectFactory.Container.GetInstance);
            CreateRealtorIfNoneExist(eventStore).ConfigureAwait(false);
        }

        private static void SetUpIoC()
        {
            var container = StructureMapConfig.ConfigureDependencies();
            GlobalConfiguration.Configuration.DependencyResolver = new StructureMapDependencyResolver(container);
        }

        private static async Task CreateRealtorIfNoneExist(IEventStore eventStore)
        {
            var realtorId = Guid.Parse("10000000-0000-0000-0000-000000000000");
            if (await eventStore.GetByIdAsync(realtorId) == null)
                await CommandBus.Instance.DispatchAsync(new SetUpRealtor(realtorId));
        }
    }
}