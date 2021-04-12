namespace PredicateBuilder.Test.Model
{
    public class AddressDto
    {
        public int AddressId { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        /// <summary>
        /// ISO state/province/region code. See ISO-3166-2 subdivision level 2.
        /// </summary>
        public string ISOCountrySubdivisionLevel2Code { get; set; }
        public string PostalCode { get; set; }
        public string ISOCountryCode { get; set; }
        public AddressType? AddressType { get; set; }
        public int PersonId { get; set; }
    }
}
