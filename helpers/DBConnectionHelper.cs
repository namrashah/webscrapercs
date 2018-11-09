/*
This class will be used as a helper for DB connections.
*/

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace WebScraperModularized.helpers
{
    public class DBConnectionHelper
    {
        public static IDbConnection getConnection(String location)
        {
            if (location.SequenceEqual("aws"))
            {
                String secretString = AwsSecretManager.Get("PostgresSecret");
                JObject secret = JObject.Parse(secretString);
                String host = secret.GetValue("host").ToObject(typeof(String)).ToString();
                String username = secret.GetValue("username").ToObject(typeof(String)).ToString();
                String password = secret.GetValue("password").ToObject(typeof(String)).ToString();
                String dbName = secret.GetValue("dbname").ToObject(typeof(String)).ToString();
                String connectionString = new StringBuilder()
                    .Append("Host=").Append(host)
                    .Append(";Username=").Append(username)
                    .Append(";Password=").Append(password)
                    .Append(";Database=").Append(dbName).ToString();
                return new NpgsqlConnection(connectionString);
            }
            else
            {
                MyConfigurationHelper myConfigurationManager = new MyConfigurationHelper();
                return new NpgsqlConnection(myConfigurationManager.getDBConnectionString());
            }
        }
    }
}