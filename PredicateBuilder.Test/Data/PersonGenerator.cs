using NLipsum.Core;
using PredicateBuilder.Test.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PredicateBuilder.Test.Data
{
    public class PersonGenerator
    {
        private static readonly Random Random = new Random(Environment.TickCount);
        private static readonly LipsumGenerator IpsumGen = new LipsumGenerator();

        public static PersonDto GetPerson(Guid guid)
        {
            PersonDto entity = new PersonDto
            {
                FirstName = IpsumGen.GenerateCharacters(Random.Next() % 20).FirstOrDefault(),
                LastName = guid.ToString(),
                MailingAddress = AddressGenerator.GetAddress(0),
                AdditionalAddresses = new List<AddressDto> { AddressGenerator.GetAddress(0) },
                PhoneNumbers = new List<PhoneNumberDto>
                {
                    PhoneNumberGenerator.GetPhoneNumber(0, true),
                    PhoneNumberGenerator.GetPhoneNumber(0)
                }
            };
            return entity;
        }
    }
}
