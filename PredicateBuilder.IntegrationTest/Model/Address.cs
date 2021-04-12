using System;
using System.Collections.Generic;

#nullable disable

namespace PredicateBuilder.IntegrationTest.Model
{
    public partial class Address
    {
        public int AddressId { get; set; }
        public int PersonId { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string IsocountrySubdivisionLevel2Code { get; set; }
        public string PostalCode { get; set; }
        public string IsocountryCode { get; set; }
        public int? AddressType { get; set; }

        public virtual Person Person { get; set; }
    }
}
