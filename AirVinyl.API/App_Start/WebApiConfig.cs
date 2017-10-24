using AirVinyl.Model;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData.Batch;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

namespace AirVinyl.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Web API routes
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);


            config.MapHttpAttributeRoutes();
            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel(), new DefaultODataBatchHandler(GlobalConfiguration.DefaultServer));

            //var cors = new EnableCorsAttribute("*", "*", "*");
            //config.EnableCors(cors);
            config.EnableCors();

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {

            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "AirVinyl";
            builder.ContainerName = "AirVinylContainer";

            #region ENTITY SETS

            // http: localhost/odata/People 
            builder.EntitySet<Person>("People");
            // http: localhost/odata/RecordStores
            builder.EntitySet<RecordStore>("RecordStores");
            // http: localhost/odata/VinylRecords
            //builder.EntitySet<VinylRecord>("VinylRecords");

            #endregion ENTITY SETS

            #region CUSTOM FUNCTIONS

            //Create bound function: public bool isHighRatedFn(int minimumRating){...}
            //http: localhost/odata/RecordStores(1)/AirVinyl.Functions.IsHighRatedFn(minimumRating=1)
            var isHighRatedFn = builder.EntityType<RecordStore>().Function("IsHighRatedFn");
            isHighRatedFn.Returns<bool>();
            isHighRatedFn.Parameter<int>("minimumRating");
            isHighRatedFn.Namespace = "AirVinyl.Functions";

            //Create bound function: public RecordStore[] AreRatedByFn(int[] personIds){...}
            //http: localhost/odata/RecordStores(1)/AirVinyl.Functions.AreRatedByFn(personIds=[1,7])
            var areRatedFn = builder.EntityType<RecordStore>().Collection.Function("AreRatedByFn");
            areRatedFn.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");
            areRatedFn.CollectionParameter<int>("personIds");
            areRatedFn.Namespace = "AirVinyl.Functions";

            //Unbound Function
            //http: localhost/odata/GetHighRatedRecordStoresFn(minimumRating = 3)
            var getHighRatedStoresFn = builder.Function("GetHighRatedRecordStoresFn");
            getHighRatedStoresFn.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");
            getHighRatedStoresFn.Parameter<int>("minimumRating");
            getHighRatedStoresFn.Namespace = "AirVinyl.Functions";

            #endregion CUSTOM FUNCTIONS

            #region ACTIONS   
            //Bound ACtion
            //http: localhost/odata/RecordStores(1)/AirVinyl.Actions.Rate
            var rateAction = builder.EntityType<RecordStore>().Action("Rate");
            rateAction.Returns<bool>();
            rateAction.Parameter<int>("rating");
            rateAction.Parameter<int>("personId");
            rateAction.Namespace = "AirVinyl.Actions";

            //Bound ACtion
            //http: localhost/odata/RecordStores/AirVinyl.Actions.RemoveRatings
            var removeRatingsAction = builder.EntityType<RecordStore>().Collection.Action("RemoveRatings");
            removeRatingsAction.Returns<bool>();
            removeRatingsAction.Parameter<int>("personId");
            removeRatingsAction.Namespace = "AirVinyl.Actions";

            //Unbound ACtion
            //http: localhost:64951/odata/RemoveRecordStoreRatings
            var removeRecordStoreRatingsAction = builder.Action("RemoveRecordStoreRatings");
            removeRecordStoreRatingsAction.Parameter<int>("personId");
            removeRecordStoreRatingsAction.Namespace = "AirVinyl.Actions";

            #endregion ACTIONS

            #region SINGLETONS
            var timSingelton = builder.Singleton<Person>("GetTim");
            #endregion

            return builder.GetEdmModel();
        }
    }
}
