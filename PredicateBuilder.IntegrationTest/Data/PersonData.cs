using Dapper;
using PredicateBuilder.Test.Data;
using PredicateBuilder.Test.Model;
using System.Data;
using System.Data.SqlClient;

namespace PredicateBuilder.IntegrationTest.Data
{
    public class PersonData
    {
        private static readonly string CreateIfNotExistsSql = @"
if not exists (SELECT 1 FROM sysobjects WHERE name='Person' and xtype='U') begin
CREATE TABLE [dbo].[Person](
	[PersonId] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](100) NULL,
	[LastName] [nvarchar](100) NOT NULL
CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC
)
)
end";

        private static readonly string InsertSql = @"
INSERT INTO [dbo].[Person]
           ([FirstName]
           ,[LastName])
     OUTPUT INSERTED.PersonId
     VALUES
           (@FirstName
           ,@LastName)";

        private string ConnectionString { get; set; }

        private AddressData _addressData;
        private PhoneNumberData _phoneNumberData;

        public PersonData(string connStr)
        {
            ConnectionString = connStr;

            _addressData = new AddressData(connStr);
            _phoneNumberData = new PhoneNumberData(connStr);
        }

        public void Initialize()
        {
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                db.Execute(CreateIfNotExistsSql);
            }
        }

        public int Add(PersonDto entity)
        {
            int id = 0;
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                id = db.QuerySingle<int>(InsertSql, entity);
            }

            if (entity.MailingAddress != null)
            {
                entity.MailingAddress.PersonId = id;
                int addressId = _addressData.Add(entity.MailingAddress);
            }
            else
            {
                AddressDto address = AddressGenerator.GetAddress(id);
                int addressId = _addressData.Add(address);
            }

            if (entity.AdditionalAddresses != null)
            {
                foreach (AddressDto address in entity.AdditionalAddresses)
                {
                    address.PersonId = id;
                    int addressId = _addressData.Add(address);
                }
            }

            if (entity.PhoneNumbers != null)
            {
                foreach (PhoneNumberDto phoneNumber in entity.PhoneNumbers)
                {
                    phoneNumber.PersonId = id;
                    int phoneNumberId = _phoneNumberData.Add(phoneNumber);
                }
            }
            else
            {
                PhoneNumberDto phoneNumber = PhoneNumberGenerator.GetPhoneNumber(id);
                int phoneNumberId = _phoneNumberData.Add(phoneNumber);
            }

            return id;
        }
    }
}
