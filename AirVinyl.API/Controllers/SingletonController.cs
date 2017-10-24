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
    public class SingletonController:ODataController
    {
        private AirVinylDbContext _ctx = new AirVinylDbContext();

        #region GET SINGLETON
        /// <summary>
        /// Http GET: localhost/odata/GetTim
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ODataRoute("GetTim")]
        public IHttpActionResult GetTim() {

            var person = _ctx.People.FirstOrDefault( GetTim => GetTim.PersonId == 6);
            return Ok(person);
        }
        #endregion

        #region UPDATE SINGLETON PARTIALLY (PATCH)
        /// <summary>
        /// Http call (PATCH): localhost/odata/GetTim 
        /// </summary>
        /// <param name="key">3</param>
        /// <param name="patch">
        ///     {
        ///	        "FirstName": "Nickolson",
        ///	        "LastName": "Jack"
        ///     }
        ///</param>
        /// <returns></returns>
        [HttpPatch]
        [ODataRoute("GetTim")]
        public IHttpActionResult Patch(Delta<Person> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            patch.Patch(currentPerson);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion

        //POST and DELTE are not implemented in SINGLETON

        #region GET SINGLETON PROPERTIES (GET)
        /// <summary>
        /// Http GET: localhost/odata/GetTim/Email
        /// </summary>
        /// <param name="key">1</param>
        /// <returns></returns>
        [HttpGet]
        [ODataRoute("GetTim/Email")]
        [ODataRoute("GetTim/FirstName")]
        [ODataRoute("GetTim/LastName")]
        [ODataRoute("GetTim/DateOfBirth")]
        [ODataRoute("GetTim/Gender")]
        public IHttpActionResult GetTimProperty()
        {

            var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            if (person == null)
                return NotFound();

            //check if person obj has property
            var propertyToGet = Url.Request.RequestUri.Segments.Last();
            if (!person.HasProperty(propertyToGet))
                return NotFound();

            //check if person obj property has value
            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null)
            {
                //return 204 error
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue);
        }
        #endregion

        #region GET SINGLETON PROPERTY RAW VALUE
        //Call not supported for clollections:
        /// <summary>
        /// Http call (GET): localhost/odata/People(7)/FirstName/$value
        /// </summary>
        /// <param name="key">7</param>
        /// <returns>"Nele"</returns>
        [HttpGet]
        [ODataRoute("GetTim/Email/$value")]
        [ODataRoute("GetTim/FirstName/$value")]
        [ODataRoute("GetTim/LastName/$value")]
        [ODataRoute("GetTim/DateOfBirth/$value")]
        [ODataRoute("GetTim/Gender/$value")]
        public IHttpActionResult GetPersonPropertyRawValue()
        {

            var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            if (person == null)
                return NotFound();

            //check if person obj has property
            var propertyToGet = Url.Request.RequestUri.Segments[Url.Request.RequestUri.Segments.Length - 2].TrimEnd('/');
            if (!person.HasProperty(propertyToGet))
                return NotFound();

            //check if person obj property has value
            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null)
            {
                //return 204 error
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue.ToString());
        }
        #endregion

        #region GET SINGLETON DATA COLLECTION PROPERTIES
        /// <summary>
        /// Http GET: "localhost/odata/GetTim/Friends"
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [ODataRoute("GetTim/Friends")]
        public IHttpActionResult GetPersonCollectionProperty()
        {
            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            var person = _ctx.People.Include(collectionPropertyToGet).FirstOrDefault(p => p.PersonId == 6);
            if (person == null)
                return NotFound();
            //check if person obj property has value
            var collectionPropertyValue = person.GetValue(collectionPropertyToGet);
            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        /// <summary>
        /// Http GET: "localhost/odata/GetTim/VinylRecords"
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult GetVinylRecordsCollectionProperty()
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            if (person == null)
                return NotFound();

            var result = _ctx.VinylRecords.Where(v => v.Person.PersonId == 6);
            return Ok(result);
        }

        #endregion

        protected override void Dispose(bool disposing) {
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}