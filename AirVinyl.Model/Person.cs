using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.OData.Builder;

namespace AirVinyl.Model
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }
        
        [StringLength(100)]    
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTimeOffset DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public int NumberOfRecordsOnWishList { get; set; }

        public decimal AmountOfCashToSpend { get; set; }

        public ICollection<Person> Friends { get; set; }

        /// <summary>
        /// accesible thru VinylRecordsController from top level:
        /// http://localhost:64951/odata/VinylRecords
        /// also needs controller
        /// check for changes in WebApiConfig
        /// </summary>
        //public ICollection<VinylRecord> VinylRecords { get; set; }

        /// <summary>
        /// accesible only thru PeopleController, cannot be accessed from top level:
        /// http://localhost:64951/odata/People(1)/VinylRecords
        /// </summary>
        [Contained]
        public ICollection<VinylRecord> VinylRecords { get; set; }

    }
}
