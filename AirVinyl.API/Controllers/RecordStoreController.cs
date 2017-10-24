using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace AirVinyl.API.Controllers
{
    /// <summary>
    /// 
    /// Here are Examples Of: 
    /// - Custom Functions
    /// - Custom Actions
    /// - Manipulating Derrived Data
    /// 
    /// Remember to add entity sets, functions, actions, singletons
    /// to edm model in WebApiConfig.cs
    /// 
    ///     builder.EntitySet<RecordStore>("RecordStores");
    /// 
    /// </summary>

    public class RecordStoreController : ODataController
    {
        private AirVinylDbContext _ctx = new AirVinylDbContext();

        #region GET DATA
        [EnableQuery()]
        [HttpGet]
        [ODataRoute("RecordStores")]
        public IHttpActionResult Get()
        {
            return Ok(_ctx.RecordStores);
        }

        public IHttpActionResult Get([FromODataUri]int key)
        {
            var recordStores = _ctx.RecordStores.Where(p => p.RecordStoreId == key);

            if (!recordStores.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(recordStores));
        }

        [HttpGet]
        [ODataRoute("RecordStores({key})/Tags")]
        [EnableQuery()]
        public IHttpActionResult GetRecordStoresTagProperty([FromODataUri]int key)
        {

            var recordStores = _ctx.RecordStores.FirstOrDefault(p => p.RecordStoreId == key);
            if (recordStores == null)
                return NotFound();

            //check if obj has property
            var propertyToGet = Url.Request.RequestUri.Segments.Last();
            if (!recordStores.HasProperty(propertyToGet))
                return NotFound();

            //check if person obj property has value
            var propertyValue = recordStores.GetValue(propertyToGet);
            if (propertyValue == null)
            {
                //return 204 error
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue);
        }
        #endregion

        #region CREATE DATA
        [HttpPost]
        [ODataRoute("RecordStores")]
        public IHttpActionResult CreateRecordStore(RecordStore recordStore)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _ctx.RecordStores.Add(recordStore);
            _ctx.SaveChanges();

            return Created(recordStore);
        }

        #endregion

        #region UPDATE DATA
        [HttpPatch]
        [ODataRoute("RecordStores({key})")]
        [ODataRoute("RecordStores({key})/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult UpdateRecordStore([FromODataUri]int key, Delta<RecordStore> updates)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentStore = _ctx.RecordStores.FirstOrDefault( s => s.RecordStoreId == key);
            if (currentStore == null)
                return NotFound();

            updates.Patch(currentStore);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        #endregion

        #region DELETE DATA
        [HttpDelete]
        [ODataRoute("RecordStores({key})")]
        [ODataRoute("RecordStores({key})/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult DeleteRecordStore([FromODataUri]int key)
        {
          
            var currentStore = _ctx.RecordStores.Include("Ratings").FirstOrDefault(s => s.RecordStoreId == key);
            if (currentStore == null)
                return NotFound();

            currentStore.Ratings.Clear();
            _ctx.RecordStores.Remove(currentStore);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        #endregion

        #region GET DERRIVED DATA
        /// <summary>
        /// http: localhost/odata/RecordStores/AirVinyl.Model.SpecializedRecordStore
        /// </summary>
        /// <returns></returns>
        [EnableQuery]
        [HttpGet]
        [ODataRoute("RecordStores/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult GetSpecializedStores()
        {
            var specializedStores = _ctx.RecordStores.Where(s => s is SpecializedRecordStore);

            return Ok(specializedStores.Select(s => s as SpecializedRecordStore));
        }

        /// <summary>
        /// <para>
        /// http GET: localhost/odata/RecordStores(1)/AirVinyl.Model.SpecializedRecordStore
        /// </para>
        /// <para>
        /// http GET: localhost/odata/RecordStores(1)/AirVinyl.Model.SpecializedRecordStore?$select=Specialization
        /// </para>
        /// http GET: localhost/odata/RecordStores(1)?$select=AirVinyl.Model.SpecializedRecordStore/Specialization 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery]
        [HttpGet]
        [ODataRoute("RecordStores({key})/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult GetSpecializedStore([FromODataUri] int key)
        {
            var specializedStore = _ctx.RecordStores.Where(r => r is SpecializedRecordStore && r.RecordStoreId == key);

            if (!specializedStore.Any())
                return NotFound();

            return Ok(SingleResult.Create(specializedStore.Select(s => s as SpecializedRecordStore)));
        }

        #endregion

        #region CUSTOM FUNCTIONS

        //bound Fn (check WebApiConfig)
        [HttpGet]
        [ODataRoute("RecordStores({key})/AirVinyl.Functions.IsHighRatedFn(minimumRating={minimumRating})")]
        public bool IsHighRatedFn([FromODataUri]int key, int minimumRating)
        {

            //get RecordStore
            var recordStore = _ctx.RecordStores
                .FirstOrDefault(p => p.RecordStoreId == key
                   && p.Ratings.Any()
                   && (p.Ratings.Sum(r => r.Value) / p.Ratings.Count) >= minimumRating);

            return (recordStore != null);
        }

        //bound Fn (check WebApiConfig)
        [HttpGet]
        [ODataRoute("RecordStores/AirVinyl.Functions.AreRatedByFn(personIds={personIds})")]
        public IHttpActionResult AreRatedByFn([FromODataUri] IEnumerable<int> personIds)
        {

            var recordStores = _ctx.RecordStores
                .Where(r => r.Ratings.Any(p => personIds.Contains(p.RatedBy.PersonId)));

            return this.CreateOKHttpActionResult(recordStores);
        }

        //Unbound Fn (check WebApiConfig)
        [HttpGet]
        [ODataRoute("GetHighRatedRecordStoresFn(minimumRating={minimumRating})")]
        public IHttpActionResult GetHighRatedRecordStoresFn([FromODataUri] int minimumRating)
        {
            var recordStores = _ctx.RecordStores
                .Where(r => r.Ratings.Any()
                            && r.Ratings.Sum(s => s.Value / r.Ratings.Count) >= minimumRating);

            return this.CreateOKHttpActionResult(recordStores);
        }

        #endregion CUSTOM FUNCTIONS

        #region ACTIONS

        //http: localhost/odata/RecordStores(1)/AirVinyl.Actions.Rate
        [HttpPost]
        [ODataRoute("RecordStores({key})/AirVinyl.Actions.Rate")]
        public IHttpActionResult Rate([FromODataUri] int key, ODataActionParameters parameters)
        {
            //find record store
            var recordStore = _ctx.RecordStores.FirstOrDefault(r => r.RecordStoreId == key);
            if (recordStore == null)
                return NotFound();

            // from the param dictionary get rating and personId
            int rating;
            int personId;
            object outputFromFictionary;

            //find rating key
            if (!parameters.TryGetValue("rating", out outputFromFictionary))
                return NotFound();
            //parse it int
            if (!int.TryParse(outputFromFictionary.ToString(), out rating))
                return NotFound();

            //find personId key
            if (!parameters.TryGetValue("personId", out outputFromFictionary))
                return NotFound();
            //parse it int
            if (!int.TryParse(outputFromFictionary.ToString(), out personId))
                return NotFound();

            //find person
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == personId);

            //everything checks out, add the rating
            recordStore.Ratings.Add(new Model.Rating() { RatedBy = person, Value = rating });

            //save
            if (_ctx.SaveChanges() > -1)
            {
                //if everything went ok return true
                return this.CreateOKHttpActionResult(true);
            }
            else
            {
                //if something went wrong return false
                //the request is still successful, false is a valid response  
                return this.CreateOKHttpActionResult(false);
            }
        }

        //http: localhost/odata/RecordStores/AirVinyl.Actions.RemoveRatings
        [HttpPost]
        [ODataRoute("RecordStores/AirVinyl.Actions.RemoveRatings")]
        public IHttpActionResult RemoveRatings(ODataActionParameters parameters)
        {
            // from the param dictionary get rating and personId
            int personId;
            object outputFromFictionary;

            //find personId key
            if (!parameters.TryGetValue("personId", out outputFromFictionary))
                return NotFound();
            //parse it int
            if (!int.TryParse(outputFromFictionary.ToString(), out personId))
                return NotFound();

            //find person
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == personId);

            //get record stores rated by person wit personId
            var recordStoreRatedByCurrentPerson = _ctx.RecordStores
                .Include("Ratings")
                .Include("Ratings.RatedBy")
                .Where(r => r.Ratings.Any(p => p.RatedBy.PersonId == personId))
                .ToList();

            //remove those ratings
            foreach (var store in recordStoreRatedByCurrentPerson)
            {
                var ratingsByCurrentPerson = store.Ratings
                    .Where(p => p.RatedBy.PersonId == personId)
                    .ToList();

                for (int i = 0; i < ratingsByCurrentPerson.Count(); i++)
                {
                    store.Ratings.Remove(ratingsByCurrentPerson[i]);
                }
            }

            //save
            if (_ctx.SaveChanges() > -1)
            {
                //if everything went ok return true
                return this.CreateOKHttpActionResult(true);
            }
            else
            {
                //if something went wrong return false
                //the request is still successful, false is a valid response  
                return this.CreateOKHttpActionResult(false);
            }
        }

        //http: localhost:64951/odata/RemoveRecordStoreRatings
        [HttpPost]
        [ODataRoute("RemoveRecordStoreRatings")]
        public IHttpActionResult RemoveRecordStoreRatings(ODataActionParameters parameters)
        {
            // from the param dictionary get rating and personId
            int personId;
            object outputFromFictionary;

            //find personId key
            if (!parameters.TryGetValue("personId", out outputFromFictionary))
                return NotFound();
            //parse it int
            if (!int.TryParse(outputFromFictionary.ToString(), out personId))
                return NotFound();

            //find person
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == personId);

            //get record stores rated by person wit personId
            var recordStoreRatedByCurrentPerson = _ctx.RecordStores
                .Include("Ratings")
                .Include("Ratings.RatedBy")
                .Where(r => r.Ratings.Any(p => p.RatedBy.PersonId == personId))
                .ToList();

            //remove those ratings
            foreach (var store in recordStoreRatedByCurrentPerson)
            {
                var ratingsByCurrentPerson = store.Ratings
                    .Where(p => p.RatedBy.PersonId == personId)
                    .ToList();

                for (int i = 0; i < ratingsByCurrentPerson.Count(); i++)
                {
                    store.Ratings.Remove(ratingsByCurrentPerson[i]);
                }
            }

            //save
            if (_ctx.SaveChanges() > -1)
            {
                //if everything went ok return true
                return StatusCode(HttpStatusCode.NoContent);
            }
            else
            {
                //if something went wrong return false
                //the request is still successful, false is a valid response  
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        #endregion ACTIONS

        protected override void Dispose(bool disposing)
        {
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}