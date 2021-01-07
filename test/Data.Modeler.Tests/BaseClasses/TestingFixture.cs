using BigBook.DataMapper;
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

        protected static Aspectus.Aspectus Aspectus => Canister.Builder.Bootstrapper.Resolve<Aspectus.Aspectus>();
        public IConfiguration Configuration { get; set; }
        protected static Manager DataMapper => Canister.Builder.Bootstrapper.Resolve<Manager>();
        protected static SQLHelper Helper => Canister.Builder.Bootstrapper.Resolve<SQLHelper>();
        protected static ObjectPool<StringBuilder> ObjectPool => Canister.Builder.Bootstrapper.Resolve<ObjectPool<StringBuilder>>();
        protected string ConnectionString { get; } = "Data Source=localhost;Initial Catalog=TestDatabase;Integrated Security=SSPI;Pooling=false";
        protected string ConnectionString2 { get; } = "Data Source=localhost;Initial Catalog=TestDatabaseForeignKeys;Integrated Security=SSPI;Pooling=false";
        protected string ConnectionStringNew { get; } = "Data Source=localhost;Initial Catalog=TestDatabase2;Integrated Security=SSPI;Pooling=false";
        protected string DatabaseName { get; } = "TestDatabase";
        protected string MasterString { get; } = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false";

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

        private void SetupConfiguration()
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
            if (Canister.Builder.Bootstrapper == null)
            {
                var Services = new ServiceCollection();
                Services.AddLogging()
                    .AddSingleton(Configuration)
                    .AddCanisterModules(x => x.AddAssembly(typeof(TestingFixture).Assembly)
                                                .RegisterDataModeler()
                                                .RegisterSQLHelper()
                                                .RegisterFileCurator()
                                                .RegisterBigBookOfDataTypes());
            }
        }
    }
}