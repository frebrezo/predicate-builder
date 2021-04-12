using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NUnit.Framework;
using PredicateBuilder.IntegrationTest.Data;
using PredicateBuilder.IntegrationTest.Model;
using PredicateBuilder.Test.Data;
using PredicateBuilder.Test.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PredicateBuilder.IntegrationTest
{
    public class PredicateHelperIntegrationTest
    {
        private PersonData _personData;
        private AddressData _addressData;
        private PhoneNumberData _phoneNumberData;
        private PredicateBuilderTestContext _context;

        [SetUp]
        public void Setup()
        {
            string configDir = Assembly.GetExecutingAssembly().Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(configDir);
            string connStr = config.ConnectionStrings.ConnectionStrings["PredicateBuilderTest"].ConnectionString;
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new NLogLoggerProvider());

            _personData = new PersonData(connStr);
            _addressData = new AddressData(connStr);
            _phoneNumberData = new PhoneNumberData(connStr);

            _personData.Initialize();
            _addressData.Initialize();
            _phoneNumberData.Initialize();

            // Scaffold-DbContext -Provider Microsoft.EntityFrameworkCore.SqlServer -Connection "Server=.;Database=PredicateBuilderTest;Trusted_Connection=True;" -OutputDir Model
            DbContextOptionsBuilder<PredicateBuilderTestContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<PredicateBuilderTestContext>();
            dbContextOptionsBuilder.UseSqlServer(connStr);
            dbContextOptionsBuilder.UseLoggerFactory(loggerFactory);
            dbContextOptionsBuilder.EnableSensitiveDataLogging();
            _context = new PredicateBuilderTestContext(dbContextOptionsBuilder.Options);
        }

        private Person GetPerson(PersonDto personDto)
        {
            int personId = _personData.Add(personDto);

            Expression<Func<Person, bool>> personPredicate = LinqKit.PredicateBuilder.New<Person>(true);
            personPredicate = personPredicate.And(PredicateHelper<Person>.AddCondition(x => x.PersonId, personId, ConditionalOperator.Equal));

            Person person = _context.People.Where(personPredicate).FirstOrDefault();
            _context.Entry(person).Collection(x => x.Addresses).Load();
            _context.Entry(person).Collection(x => x.PhoneNumbers).Load();

            return person;
        }

        [Test]
        public void AddCondition_EqualTest()
        {
            Guid personGuid = Guid.NewGuid();
            PersonDto personDto = PersonGenerator.GetPerson(personGuid);
            Person person = GetPerson(personDto);

            Assert.AreEqual(personDto.FirstName, person.FirstName);
            Assert.AreEqual(personDto.LastName, person.LastName);
        }
    }
}
