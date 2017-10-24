using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData;
using System.Web.OData.Routing;

namespace AirVinyl.API.Controllers
{
    /// <summary>
    /// 
    /// Here are examples of:
    /// - Get, Create, Update (Partial, Full) and Delete Data
    /// - Get Data Single Property
    /// - Get Data Single Property Value
    /// - Get, Create, Update and Delete Data Collection Item/s
    /// - Get Contained (not accesible from root) Data Collection
    /// - Create, Update, Delete Association Links 
    /// 
    /// Remember to add entity sets, functions, actions, singletons
    /// to edm model in WebApiConfig.cs
    /// </summary>

    /* [EnableQuery(
     *      MaxExpansionDepth = 3       => enables url filters and level of filter depth
     *      MaxTop = 10                 => maximum shown records from top
     *      MaxSkip = 15                => maximum records to be skiped
     *       PageSize = 4               => maximum records on page
     * )] */
    [EnableQuery(MaxExpansionDepth = 3)]
    /*Enable Cross-Origin Resource Sharing for http:\\localhost:8080 */
    [EnableCors(origins: "http://localhost:56831", headers: "*", methods: "*")]
    public class PeopleController : ODataController
    {
        private AirVinylDbContext _ctx = new AirVinylDbContext();

        //CREATE RESOURCE START
        #region GET DATA (GET)
        /// <summary>
        /// Http GET: localhost/odata/People
        /// </summary>
        /// <returns></returns>
        [EnableQuery(MaxTop = 10, MaxSkip = 15, PageSize = 4)]
        public IHttpActionResult Get()
        {
            return Ok(_ctx.People);
        }

        /// <summary>
        /// Http GET: localhost/odata/People(1)
        /// <para>Http filter GET: localhost/odata/People(1)?$select=Email</para>
        /// </summary>
        /// <param name="key">1</param>
        /// <returns></returns>
        public IHttpActionResult Get([FromODataUri]int key)
        {
            var people = _ctx.People.Where(p => p.PersonId == key);

            if (!people.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(people));
        }

        //public IHttpActionResult Get([FromODataUri]int key)
        //{
        //    var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
        //    if (person == null)
        //        return NotFound();
        //    else
        //        return Ok(person);
        //}
        #endregion

        #region GET DATA PROPERTIES (GET)
        /// <summary>
        /// Http GET: localhost/odata/People(1)/Email
        /// </summary>
        /// <param name="key">1</param>
        /// <returns></returns>
        [HttpGet]
        [ODataRoute("People({key})/Email")]
        [ODataRoute("People({key})/FirstName")]
        [ODataRoute("People({key})/LastName")]
        [ODataRoute("People({key})/DateOfBirth")]
        [ODataRoute("People({key})/Gender")]
        public IHttpActionResult GetPersonProperty([FromODataUri]int key)
        {

            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
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

        #region GET DATA PROPERTY RAW VALUE
        //Call not supported for clollections:
        /// <summary>
        /// Http call (GET): http://localhost:64951/odata/People(7)/FirstName/$value
        /// </summary>
        /// <param name="key">7</param>
        /// <returns>"Nele"</returns>
        [HttpGet]
        [ODataRoute("People({key})/Email/$value")]
        [ODataRoute("People({key})/FirstName/$value")]
        [ODataRoute("People({key})/LastName/$value")]
        [ODataRoute("People({key})/DateOfBirth/$value")]
        [ODataRoute("People({key})/Gender/$value")]
        public IHttpActionResult GetPersonPropertyRawValue([FromODataUri]int key)
        {

            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
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

        #region CREATE DATA (POST)
        /// <summary>
        /// Http call (POST): localhost/odata/People
        /// </summary>
        /// <param name="person">
        ///     {   "@odata.context": "ocalhost/odata/$metadata#People/$entity",
        ///         "Email": "emma@smith.com",
        ///         "FirstName": "Emma",
        ///         "LastName": "Smith",
        ///         "DateOfBirth": "1981-01-03",
        ///         "Gender": "Male",
        ///         "VinylRecords": [{"Title": "Nana","Artist": "Lady Punk","CatalogNumber": "PL/114"}]
        ///     }
        ///</param>
        /// <returns></returns>
        public IHttpActionResult Post(Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _ctx.People.Add(person);
            _ctx.SaveChanges();

            return Created(person);
        }

        #endregion

        #region UPDATE DATA FULLY (PUT)
        /// <summary>
        /// Http call (PUT): localhost/odata/People(3) 
        /// </summary>
        /// <param name="key">3</param>
        /// <param name="person">
        ///     {
        ///         "FirstName": "Nick",
        ///         "LastName": "Missorten",
        ///         "DateOfBirth": "1983-05-18T00:00:00+02:00",
        ///         "Gender": "Male",
        ///         "NumberOfRecordsOnWishList": 23,
        ///         "AmountOfCashToSpend": 2500.00
        ///     }
        ///</param>
        /// <returns></returns>
        public IHttpActionResult Put([FromODataUri]int key, Person person)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);

            if (currentPerson == null)
            {
                return NotFound();
            }

            //make sure that id in passed 'person' object matches existing one in db
            person.PersonId = currentPerson.PersonId;
            _ctx.Entry(currentPerson).CurrentValues.SetValues(person);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion

        #region UPDATE DATA PARTIALLY (PATCH)
        /// <summary>
        /// Http call (PATCH): localhost/odata/People(3) 
        /// </summary>
        /// <param name="key">3</param>
        /// <param name="patch">
        ///     {
        ///	        "FirstName": "Nickolson",
        ///	        "LastName": "Jack"
        ///     }
        ///</param>
        /// <returns></returns>
        public IHttpActionResult Patch([FromODataUri]int key, Delta<Person> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            patch.Patch(currentPerson);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion

        #region DELETE DATA (DELETE)
        /// <summary>
        /// Http call (DELETE): localhost/odata/People(3) 
        /// </summary>
        /// <param name="key">3</param>
        /// <returns></returns>
        public IHttpActionResult Delete([FromODataUri]int key)
        {
            var currentPerson = _ctx
                .People
                .Include("Friends")
                .FirstOrDefault(p => p.PersonId == key);

            if (currentPerson == null)
            {
                return NotFound();
            }

            var peopleWithCurrentPersonAsFriend = _ctx
                .People
                .Include("Friends")
                .Where(p => p.Friends
                               .Select(f => f.PersonId)
                               .AsQueryable()
                               .Contains(key)
                )
                .ToList();

            foreach (var person in peopleWithCurrentPersonAsFriend)
            {
                person.Friends.Remove(currentPerson);
            }

            _ctx.People.Remove(currentPerson);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion
        //CREATE RESOURCE END

        //CREATE COLLECTION ITEMS START
        #region GET DATA CONTAINED COLLECTION ALL ITEMS (NO-TOP LEVEL ACCESS)
        /// <summary>
        /// Http filter GET: "localhost/odata/People(1)/VinylRecords?$select=Title"
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult GetVinylRecordsForPerson([FromODataUri]int key)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
                return NotFound();

            var result = _ctx.VinylRecords
                .Include("DynamicVinylRecordProperties")
                .Where(v => v.Person.PersonId == key);

            return Ok(result);
        }
        #endregion

        #region GET DATA CONTAINED COLLECTION SINGLE ITEM (NO-TOP LEVEL ACCESS)
        /// <summary>
        /// Http filter GET: "localhost/odata/People(1)/VinylRecords(11)"
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery()]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult GetVinylRecordForPerson([FromODataUri]int key, [FromODataUri]int vinylRecordKey)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
                return NotFound();

            var result = _ctx.VinylRecords
                .Include("DynamicVinylRecordProperties")
                .Where(v => v.Person.PersonId == key && v.VinylRecordId == vinylRecordKey);

            if (!result.Any())
                return NotFound();

            return Ok(SingleResult.Create(result));
        }
        #endregion

        #region GET DATA COLLECTION PROPERTIES (GET)
        /// <summary>
        ///  <para>Http GET: "localhost/odata/People(7)/Friends" </para>
        ///  <para>Http GET: "localhost/odata/People(7)/VinylRecords" </para>
        /// </summary>
        /// <param name="key">7</param>
        /// <returns></returns>
        [HttpGet]
        [ODataRoute("People({key})/Friends")]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult GetPersonCollectionProperty([FromODataUri]int key)
        {
            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            var person = _ctx.People.Include(collectionPropertyToGet).FirstOrDefault(p => p.PersonId == key);
            if (person == null)
                return NotFound();
            //check if person obj property has value
            var collectionPropertyValue = person.GetValue(collectionPropertyToGet);
            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        /// <summary>
        /// Http filter GET: "localhost/odata/People(1)/Friends?$select=FirstName"
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [ODataRoute("People({key})/Friends")]
        public IHttpActionResult GetFriendsCollectionProperty([FromODataUri]int key)
        {
            var person = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            if (person == null)
                return NotFound();
            //check if person obj property has value
            var result = person.Friends;
            return Ok(result);
        }
        #endregion

        #region CREATE DATA COLLECTION ITEM
        [HttpPost]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult CreateVinylRecordForPerson([FromODataUri]int key, VinylRecord vinylRecord)
        {
            // CHECK IF MODEL IS VALID   
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // check if person object exists
            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
                return NotFound();

            // link person to VinylRecord (also avoids an invalid person
            // key ont the passed-in record - key fromthe URI wins)
            vinylRecord.Person = currentPerson;

            // add the vinylRecord
            _ctx.VinylRecords.Add(vinylRecord);
            _ctx.SaveChanges();

            return Created(vinylRecord);
        }
        #endregion

        #region PARTIAL UPDATE DATA COLLECTION ITEM
        [HttpPatch]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult PartialUpdateVinylRecordForPerson(
            [FromODataUri]int key,
            [FromODataUri]int vinylRecordKey,
            Delta<VinylRecord> patch)
        {
            // CHECK IF MODEL IS VALID   
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // check if person object exists
            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
                return NotFound();

            // check if vinyl record exists in current person collection
            var currentVinylRecord = _ctx.VinylRecords
                .Include("DynamicVinylRecordProperties")
                .FirstOrDefault(v => v.VinylRecordId == vinylRecordKey && v.Person.PersonId == key);

            if (currentVinylRecord == null)
                return NotFound();

            // update the vinylRecord
            patch.Patch(currentVinylRecord);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion

        #region DELETE DATA COLLECTION ITEM

        [HttpDelete]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult DeleteVinylRecordForPerson(
            [FromODataUri]int key,
            [FromODataUri]int vinylRecordKey)
        {
            // check if person object exists
            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
                return NotFound();

            // check if vinyl record exists in current person collection
            var currentVinylRecord = _ctx.VinylRecords.FirstOrDefault(v => v.VinylRecordId == vinylRecordKey && v.Person.PersonId == key);
            if (currentVinylRecord == null)
                return NotFound();

            // update the vinylRecord
            _ctx.VinylRecords.Remove(currentVinylRecord);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        #endregion
        //CREATE COLLECTION ITEMS END

        //CREATE ASSOCIATION LINKS START
        #region CREATE ASSOCIATION LINK (GET)
        /// <summary>
        /// <para>Http call:  localhost/odata/People(7)/Friends/$ref </para>
        /// <para>$ref = {"@odata.id": "localhost/odata/People(1)"}</para>
        /// <param name="key">7</param>
        /// <param name="link">localhost/odata/People(1)</param>
        /// <returns></returns>
        /// </summary>
        [HttpPost]
        [ODataRoute("People({key})/Friends/$ref")]
        public IHttpActionResult CreateLinkToFriend([FromODataUri]int key, [FromBody]Uri link)
        {
            //check if person object exists
            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            //check if people are linked already
            int keyOfFriendToAdd = Request.GetKeyValue<int>(link); //OData 5.9.


            if (currentPerson.Friends.Any(i => i.PersonId == keyOfFriendToAdd))
            {
                return BadRequest(string.Format("The person with {0} id is already linked to person with id {1}", key, keyOfFriendToAdd));
            }

            var friendToLink = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfFriendToAdd);

            if (friendToLink == null)
            {
                return NotFound();
            }

            currentPerson.Friends.Add(friendToLink);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);

        }
        #endregion

        #region UPDATE ASSOCIATION LINK
        /// <summary>
        /// Http call (PUT): localhost/odata/People(7)/Friends(1)/$ref
        /// $ref =  {"@odata.id": "localhost/odata/People(3)"}
        /// </summary>
        /// <param name="key">7</param>
        /// <param name="relatedKey">1</param>
        /// <param name="link">localhost/odata/People(3)</param>
        /// <returns></returns>
        [HttpPut]
        [ODataRoute("People({key})/Friends({relatedKey})/$ref")]
        public IHttpActionResult UpdateLinkToFriend([FromODataUri]int key, [FromODataUri]int relatedKey, [FromBody]Uri link)
        {
            //check if currentPerson exists
            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }
            //check if currentFriend exists
            var currentFriend = currentPerson.Friends.FirstOrDefault(f => f.PersonId == relatedKey);
            if (currentFriend == null)
            {
                return NotFound();
            }
            //get id[key] value from request
            int keyOfNewFriend = Request.GetKeyValue<int>(link);
            //check if currentPerson is already associated with new frined
            if (currentPerson.Friends.Any(f => f.PersonId == keyOfNewFriend))
            {
                return BadRequest(string.Format("The person with {0} id is already linked to person with id {1}", key, keyOfNewFriend));
            }

            //find friend we want to assiociate currentPerson with, instead of currentFriend
            var newFriend = _ctx.People.FirstOrDefault(f => f.PersonId == keyOfNewFriend);
            if (newFriend == null)
            {
                return NotFound();
            }

            currentPerson.Friends.Remove(currentFriend);
            currentPerson.Friends.Add(newFriend);

            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion

        #region DELETE ASSOCIATION LINK
        /// <summary>
        /// <para>Http call (DELETE): localhost/odata/People(7)/Friends/$ref$id=http://localhost:64951/odata/People(3) </para>
        /// <para>DELETE odata/People({key})/Friends({relatedKey})/$ref$id={'realtedUriWithRelatedKey'}</para>
        /// </summary>
        /// <param name="key">7</param>
        /// <param name="relatedKey">3</param>
        /// <returns></returns>
        [HttpPost]
        [ODataRoute("People({key})/Friends({relatedKey})/$ref")]
        public IHttpActionResult DeleteLinkToFriend([FromODataUri]int key, [FromODataUri]int relatedKey)
        {
            //check if currentPerson exists
            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }
            //check if currentFriend exists
            var friend = currentPerson.Friends.FirstOrDefault(f => f.PersonId == relatedKey);
            if (friend == null)
            {
                return NotFound();
            }

            currentPerson.Friends.Remove(friend);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion
        //CREATE ASSOCIATION LINKS END

        protected override void Dispose(bool disposing)
        {
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}