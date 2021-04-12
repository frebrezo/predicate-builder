using System;
using System.Collections.Generic;

#nullable disable

namespace PredicateBuilder.IntegrationTest.Model
{
    public partial class PhoneNumber
    {
        public int PhoneNumberId { get; set; }
        public int PersonId { get; set; }
        public string CountryCallingCode { get; set; }
        public string Number { get; set; }
        public string Extension { get; set; }
        public int? PhoneNumberType { get; set; }
        public bool IsPrimary { get; set; }

        public virtual Person Person { get; set; }
    }
}
