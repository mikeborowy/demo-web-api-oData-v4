using AirVinyl.Model;
using AirVinyl.WebClient.Models;
using Microsoft.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirVinyl.WebClient.Controllers
{
    public class AirVinylController : Controller
    {
        #region GET DATA
        // GET: AirVinyl
        /// <summary>
        /// Get People Response
        /// </summary>
        /// <returns></returns>
        public ActionResult Example1()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:64951/odata"));
            var description = "Get People Response";

            //using AirVinyl.Model START
            var peopleResponse = context.People.Execute();
            var personResponse = context.People.ByKey(1).GetValue();
            //using AirVinyl.Model END

            AirVinylViewModel avvm = new AirVinylViewModel()
            {
                People = peopleResponse,
                Person = personResponse,
                Description = description
            };

            return View("Index", avvm);
        }

        /// <summary>
        /// Get People Response including total count
        /// </summary>
        /// <returns></returns>
        public ActionResult Example2()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:64951/odata"));
            var description = "Get People Response including total count";

            //using AirVinyl.Model START
            var peopleResponse = context.People
                .IncludeTotalCount()
                .Execute() as QueryOperationResponse<Person>;

            string additionalData = "Total Count: " + peopleResponse.TotalCount.ToString();

            var personResponse = context.People.ByKey(1).GetValue();
            //using AirVinyl.Model END

            AirVinylViewModel avvm = new AirVinylViewModel()
            {
                People = peopleResponse,
                Person = personResponse,
                AdditionalData = additionalData,
                Description = description
            };

            return View("Index", avvm);
        }

        /// <summary>
        /// Get People Response with expanded Vinyl Records Collection
        /// </summary>
        /// <returns></returns>
        public ActionResult Example3a()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:64951/odata"));
            var description = "Get People Response with expanded Vinyl Records Collection";

            //using AirVinyl.Model START
            var peopleResponse = context.People
                .IncludeTotalCount()
                .Expand(v => v.VinylRecords)
                .Execute() as QueryOperationResponse<Person>;

            //must be put to list otherwise personResponse will overwrite first record
            //and will show it without VinylRecords list
            var peopleAsList = peopleResponse.ToList();

            string additionalData = "Total Count: " + peopleResponse.TotalCount.ToString();

            var personResponse = context.People.ByKey(1).GetValue();
            //using AirVinyl.Model END

            AirVinylViewModel avvm = new AirVinylViewModel()
            {
                People = peopleAsList,
                Person = personResponse,
                AdditionalData = additionalData,
                Description = description
            };

            return View("Index", avvm);
        }

        /// <summary>
        /// Get People Response with paging 
        /// </summary>
        /// <returns></returns>
        public ActionResult Example3b()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:64951/odata"));
            var description = "Get People Response with expanded Vinyl Records Collection with Paging";

            //using AirVinyl.Model START
            //var peopleResponse = context.People.Execute();
            var peopleResponse = context.People
                .IncludeTotalCount()
                .Expand(v => v.VinylRecords)
                .Execute() as QueryOperationResponse<Person>;

            //must be put to list otherwise personResponse will overwrite first record
            //and will show it without VinylRecords list
            var peopleAsList = peopleResponse.ToList();

            //Paging
            DataServiceQueryContinuation<Person> token = peopleResponse.GetContinuation();
            peopleResponse = context.Execute(token);
            //fill list people with latest data
            peopleAsList = peopleResponse.ToList();

            string additionalData = "Total Count: " + peopleResponse.TotalCount.ToString();

            var personResponse = context.People.ByKey(1).GetValue();
            //using AirVinyl.Model END

            AirVinylViewModel avvm = new AirVinylViewModel()
            {
                People = peopleAsList,
                Person = personResponse,
                AdditionalData = additionalData,
                Description = description
            };

            return View("Index", avvm);
        }

        /// <summary>
        /// Get People Response with First Name that ends with "n" and order them descending by fist name
        /// </summary>
        /// <returns></returns>
        public ActionResult Example4()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:64951/odata"));
            var description = "Get People Response with First Name that ends with 'n' \n and order them descending by fist name";

            var peopleResponse = context.People
                .Expand(v => v.VinylRecords)
                .Where(p => p.FirstName.EndsWith("n"))
                .OrderByDescending(p => p.FirstName);

            var peopleAsList = peopleResponse.ToList();
            var personResponse = context.People.ByKey(1).GetValue();

            AirVinylViewModel avvm = new AirVinylViewModel()
            {
                People = peopleAsList,
                Person = personResponse,
                Description = description
            };

            return View("Index", avvm);
        }

        /// <summary>
        /// Skip first record and grab just next one
        /// </summary>
        /// <returns></returns>
        public ActionResult Example5()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:64951/odata"));
            var description = "Skip first record and grab just next one";

            var peopleResponse = context.People
                .Expand(v => v.VinylRecords)
                .Skip(1)
                .Take(1);

            var peopleAsList = peopleResponse.ToList();
            var personResponse = context.People.ByKey(1).GetValue();

            AirVinylViewModel avvm = new AirVinylViewModel()
            {
                People = peopleAsList,
                Person = personResponse,
                Description = description
            };

            return View("Index", avvm);
        }

        /// <summary>
        /// View just desired properties of People as Additional Data
        /// </summary>
        /// <returns></returns>
        public ActionResult Example6()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:64951/odata"));
            var description = "View just desired properties of People as Additional Data";

            var selectFromPeople = context.People.Select(p => new { p.FirstName, p.LastName });

            string additionalData = "";

            foreach (var partialPerson in selectFromPeople)
            {
                additionalData += partialPerson.FirstName + " " + partialPerson.LastName + "\n";
            }

            var personResponse = context.People.ByKey(1).GetValue();

            AirVinylViewModel avvm = new AirVinylViewModel()
            {
                Person = personResponse,
                AdditionalData = additionalData,
                Description = description
            };

            return View("Index", avvm);
        }
        #endregion

        #region CREATE DATA
        /// <summary>
        /// Create new Peson: Mike Borowy
        /// </summary>
        /// <returns></returns>
        public ActionResult Example7()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:64951/odata"));
            var description = "Create new Peson: Mike Borowy";

            var peopleResponse = context.People.OrderByDescending(p => p.PersonId);
            var peopleList = peopleResponse
                             .OrderByDescending(p => p.PersonId)
                             .ToList();

            var newPerson = new Person
            {
                FirstName = "Mike",
                LastName = "Borowy"
            };

            context.AddToPeople(newPerson);
            context.SaveChanges();

            AirVinylViewModel avvm = new AirVinylViewModel()
            {
                People = peopleList,
                Description = description
            };

            return View("Index", avvm);
        }
        #endregion

        #region UPDATE DATA
        /// <summary>
        /// Update Mike Borowy to Michal Borowy
        /// </summary>
        /// <returns></returns>
        public ActionResult Example8()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:64951/odata"));
            var description = "Update Mike Borowy to Michal Borowy";

            var peopleResponse = context.People.OrderByDescending(p => p.PersonId);
            var personToUpdate = peopleResponse.Where(p => p.LastName == "Borowy").FirstOrDefault();

            if (personToUpdate != null)
            {
                personToUpdate.FirstName = "Michael";

                context.UpdateObject(personToUpdate);
                context.SaveChanges();
            }

            var peopleList = peopleResponse.OrderByDescending(p => p.PersonId).ToList();

            AirVinylViewModel avvm = new AirVinylViewModel()
            {
                People = peopleList,
                Description = description
            };

            return View("Index", avvm);
        }
        #endregion

        #region DELETE DATA
        /// <summary>
        /// Delete Mike Borowy
        /// <returns></returns>
        public ActionResult Example9()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:64951/odata"));
            var description = " Delete Mike Borowy";

            var peopleResponse = context.People.OrderByDescending(p => p.PersonId);
            var personToDelete = peopleResponse.Where(p => p.LastName == "Borowy").FirstOrDefault();

            if (personToDelete != null)
            {
                context.DeleteObject(personToDelete);
                context.SaveChanges();
            }

            var peopleList = peopleResponse.OrderByDescending(p => p.PersonId).ToList();

            AirVinylViewModel avvm = new AirVinylViewModel()
            {
                People = peopleList,
                Description = description
            };

            return View("Index", avvm);
        }
        #endregion

    }
}