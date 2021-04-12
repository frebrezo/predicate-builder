namespace PredicateBuilder.Test.Model
{
    public class PhoneNumberDto
    {
        public int PhoneNumberId { get; set; }
        public string CountryCallingCode { get; set; }
        public string Number { get; set; }
        public string Extension { get; set; }
        public PhoneNumberType? PhoneNumberType { get; set; }
        public bool IsPrimary { get; set; }
        public int PersonId { get; set; }
    }
}
