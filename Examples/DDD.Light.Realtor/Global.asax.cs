﻿using System;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;
using DDD.Light.Realtor.Application.Commands;
using DDD.Light.Realtor.Bootstrap;
using DDD.Light.Realtor.Resources;
using DDD.Light.Repo.Contracts;
using StructureMap;

namespace DDD.Light.Realtor
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801


    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            ConfigureMappings();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            SetUpIoC();
            InitApp();            
        }

        private static void InitApp()
        {
            CreateRealtorIfNoneExist();
            HandlerSubscribtions.SubscribeAllHandlers();
        }

        private static void ConfigureMappings()
        {
            Mapper.CreateMap<RealtorListingResource, PostListingCommand>()
                .ForMember(command => command.ListingId, mapper => mapper.MapFrom(resource => resource.Id));
        }

        private static void SetUpIoC()
        {
            var container = StructureMapConfig.ConfigureDependencies();
            GlobalConfiguration.Configuration.DependencyResolver = new StructureMapDependencyResolver(container);
        }

        private static void CreateRealtorIfNoneExist()
        {
            var realtorRepo = ObjectFactory.GetInstance<IRepository<Domain.Model.Realtor>>();
            if (!realtorRepo.Get().Any())
                realtorRepo.Save(new Domain.Model.Realtor {Id = Guid.Empty});
        }
    }
}