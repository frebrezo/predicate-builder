using Dapper;
using PredicateBuilder.Test.Model;
using System.Data;
using System.Data.SqlClient;

namespace PredicateBuilder.IntegrationTest.Data
{
    public class AddressData
    {
        public static readonly string CreateIfNotExistsSql = @"
if not exists (SELECT 1 FROM sysobjects WHERE name='Address' and xtype='U') begin
CREATE TABLE [dbo].[Address](
	[AddressId] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[StreetAddress1] [nvarchar](200) NULL,
	[StreetAddress2] [nvarchar](200) NULL,
	[City] [nvarchar](100) NULL,
	[ISOCountrySubdivisionLevel2Code] [nvarchar](10) NULL,
	[PostalCode] [nvarchar](20) NULL,
	[ISOCountryCode] [nvarchar](5) NULL,
	[AddressType] [int] NULL,
 CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED 
(
	[AddressId] ASC
)
)

ALTER TABLE [dbo].[Address]  WITH CHECK ADD  CONSTRAINT [FK_Address_Person_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([PersonId])
end";

        public static readonly string InsertSql = @"
INSERT INTO [dbo].[Address]
           ([PersonId]
           ,[StreetAddress1]
           ,[StreetAddress2]
           ,[City]
           ,[ISOCountrySubdivisionLevel2Code]
           ,[PostalCode]
           ,[ISOCountryCode]
           ,[AddressType])
     OUTPUT INSERTED.AddressId
     VALUES
           (@PersonId
           ,@StreetAddress1
           ,@StreetAddress2
           ,@City
           ,@ISOCountrySubdivisionLevel2Code
           ,@PostalCode
           ,@ISOCountryCode
           ,@AddressType)";

        private string ConnectionString { get; set; }

        public AddressData(string connStr)
        {
            ConnectionString = connStr;
        }

        public void Initialize()
        {
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                db.Execute(CreateIfNotExistsSql);
            }
        }

        public int Add(AddressDto entity)
        {
            int id = 0;
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                id = db.QuerySingle<int>(InsertSql, entity);
            }
            return id;
        }
    }
}
