using System;
using System.Collections.Generic;

#nullable disable

namespace PredicateBuilder.IntegrationTest.Model
{
    public partial class Person
    {
        public Person()
        {
            Addresses = new HashSet<Address>();
            PhoneNumbers = new HashSet<PhoneNumber>();
        }

        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
    }
}
