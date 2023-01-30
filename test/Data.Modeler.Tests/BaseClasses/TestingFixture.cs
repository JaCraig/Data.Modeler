using FileCurator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using SQLHelperDB;
using SQLHelperDB.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Data.Modeler.Tests.BaseClasses
{
    [Collection("DirectoryCollection")]
    public class TestingFixture : IDisposable
    {
        public TestingFixture()
        {
            SetupConfiguration();
            SetupIoC();
            var TempTask = Task.Run(async () => await SetupDatabasesAsync().ConfigureAwait(false));
            TempTask.GetAwaiter().GetResult();
        }

        public static IConfiguration Configuration { get; set; }
        protected static Aspectus.Aspectus Aspectus => GetServiceProvider().GetService<Aspectus.Aspectus>();
        protected static string ConnectionString { get; } = "Data Source=localhost;Initial Catalog=TestDatabase;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True";
        protected static string ConnectionString2 { get; } = "Data Source=localhost;Initial Catalog=TestDatabaseForeignKeys;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True";
        protected static string ConnectionStringNew { get; } = "Data Source=localhost;Initial Catalog=TestDatabase2;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True";
        protected static string DatabaseName { get; } = "TestDatabase";
        protected static SQLHelper Helper => GetServiceProvider().GetService<SQLHelper>();
        protected static string MasterString { get; } = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True";
        protected static ObjectPool<StringBuilder> ObjectPool => GetServiceProvider().GetService<ObjectPool<StringBuilder>>();

        /// <summary>
        /// The service provider lock
        /// </summary>
        private static readonly object ServiceProviderLock = new object();

        /// <summary>
        /// The service provider
        /// </summary>
        private static IServiceProvider ServiceProvider;

        public void Dispose()
        {
            using var TempConnection = SqlClientFactory.Instance.CreateConnection();
            TempConnection.ConnectionString = MasterString;
            using var TempCommand = TempConnection.CreateCommand();
            try
            {
                TempCommand.CommandText = "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase\r\nALTER DATABASE TestDatabaseForeignKeys SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabaseForeignKeys SET ONLINE\r\nDROP DATABASE TestDatabaseForeignKeys";
                TempCommand.Open();
                TempCommand.ExecuteNonQuery();
            }
            catch { }
            finally { TempCommand.Close(); }
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <returns></returns>
        protected static IServiceProvider GetServiceProvider()
        {
            if (ServiceProvider is not null)
                return ServiceProvider;
            lock (ServiceProviderLock)
            {
                if (ServiceProvider is not null)
                    return ServiceProvider;
                ServiceProvider = new ServiceCollection().AddLogging().AddSingleton(Configuration).AddCanisterModules()?.BuildServiceProvider();
            }
            return ServiceProvider;
        }

        private static void SetupConfiguration()
        {
            var dict = new Dictionary<string, string>
                {
                    { "ConnectionStrings:Default", ConnectionString },
                    { "ConnectionStrings:Default2", ConnectionString2 },
                    { "ConnectionStrings:DefaultNew", ConnectionStringNew },
                    { "ConnectionStrings:MasterString", MasterString }
                };
            Configuration = new ConfigurationBuilder()
                             .AddInMemoryCollection(dict)
                             .Build();
        }

        private async Task SetupDatabasesAsync()
        {
            var TempHelper = Helper;
            try
            {
                using (var TempConnection = SqlClientFactory.Instance.CreateConnection())
                {
                    TempConnection.ConnectionString = MasterString;
                    using var TempCommand = TempConnection.CreateCommand();
                    try
                    {
                        TempCommand.CommandText = "Create Database TestDatabase";
                        TempCommand.Open();
                        TempCommand.ExecuteNonQuery();
                    }
                    finally { TempCommand.Close(); }
                }
                using (var TempConnection = SqlClientFactory.Instance.CreateConnection())
                {
                    TempConnection.ConnectionString = MasterString;
                    using var TempCommand = TempConnection.CreateCommand();
                    try
                    {
                        TempCommand.CommandText = "Create Database TestDatabaseForeignKeys";
                        TempCommand.Open();
                        TempCommand.ExecuteNonQuery();
                    }
                    finally { TempCommand.Close(); }
                }
                var Queries = new FileInfo("./Scripts/script.sql").Read().Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var Query in Queries)
                {
                    await TempHelper
                        .CreateBatch()
                        .AddQuery(CommandType.Text, Query)
                        .ExecuteScalarAsync<int>().ConfigureAwait(false);
                }
                Queries = new FileInfo("./Scripts/testdatabase.sql").Read().Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var Query in Queries)
                {
                    await TempHelper
                        .CreateBatch(database: "Default2")
                        .AddQuery(CommandType.Text, Query)
                        .ExecuteScalarAsync<int>().ConfigureAwait(false);
                }
            }
            catch { }
        }

        private void SetupIoC()
        {
        }
    }
}