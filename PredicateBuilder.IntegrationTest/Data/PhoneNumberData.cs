using Dapper;
using PredicateBuilder.Test.Model;
using System.Data;
using System.Data.SqlClient;

namespace PredicateBuilder.IntegrationTest.Data
{
    public class PhoneNumberData
    {
        public static readonly string CreateIfNotExistsSql = @"
if not exists (SELECT 1 FROM sysobjects WHERE name='PhoneNumber' and xtype='U') begin
CREATE TABLE [dbo].[PhoneNumber](
	[PhoneNumberId] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[CountryCallingCode] [nvarchar](10) NULL,
	[Number] [nvarchar](20) NOT NULL,
	[Extension] [nvarchar](10) NULL,
	[PhoneNumberType] [int] NULL,
	[IsPrimary] [bit] NOT NULL,
 CONSTRAINT [PK_PhoneNumber] PRIMARY KEY CLUSTERED 
(
	[PhoneNumberId] ASC
)
)

ALTER TABLE [dbo].[PhoneNumber]  WITH CHECK ADD  CONSTRAINT [FK_PhoneNumber_Person_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([PersonId])
end";

        public static readonly string InsertSql = @"
INSERT INTO [dbo].[PhoneNumber]
           ([PersonId]
           ,[CountryCallingCode]
           ,[Number]
           ,[Extension]
           ,[PhoneNumberType]
           ,[IsPrimary])
     OUTPUT INSERTED.PhoneNumberId
     VALUES
           (@PersonId
           ,@CountryCallingCode
           ,@Number
           ,@Extension
           ,@PhoneNumberType
           ,@IsPrimary)";

        private string ConnectionString { get; set; }

        public PhoneNumberData(string connStr)
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

        public int Add(PhoneNumberDto entity)
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
