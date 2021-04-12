using System.Collections.Generic;

namespace PredicateBuilder.Test.Model
{
    public class PersonDto
    {
        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AddressDto MailingAddress { get; set; }
        public List<AddressDto> AdditionalAddresses { get; set; }
        public List<PhoneNumberDto> PhoneNumbers { get; set; }
    }
}
