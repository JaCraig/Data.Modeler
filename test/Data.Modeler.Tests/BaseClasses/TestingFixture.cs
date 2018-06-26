using Data.Modeler.Registration;
using FileCurator;
using FileCurator.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SQLHelperDB;
using SQLHelperDB.ExtensionMethods;
using SQLHelperDB.Registration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
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
            SetupDatabases();
        }

        public IConfigurationRoot Configuration { get; set; }

        protected string ConnectionString => "Data Source=localhost;Initial Catalog=TestDatabase;Integrated Security=SSPI;Pooling=false";

        protected string ConnectionString2 => "Data Source=localhost;Initial Catalog=TestDatabaseForeignKeys;Integrated Security=SSPI;Pooling=false";

        protected string ConnectionStringNew => "Data Source=localhost;Initial Catalog=TestDatabase2;Integrated Security=SSPI;Pooling=false";

        protected string DatabaseName => "TestDatabase";
        protected string MasterString => "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false";

        public void Dispose()
        {
            using (var TempConnection = SqlClientFactory.Instance.CreateConnection())
            {
                TempConnection.ConnectionString = MasterString;
                using (var TempCommand = TempConnection.CreateCommand())
                {
                    try
                    {
                        TempCommand.CommandText = "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase\r\nALTER DATABASE TestDatabaseForeignKeys SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabaseForeignKeys SET ONLINE\r\nDROP DATABASE TestDatabaseForeignKeys";
                        TempCommand.Open();
                        TempCommand.ExecuteNonQuery();
                    }
                    catch { }
                    finally { TempCommand.Close(); }
                }
            }
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

        private void SetupDatabases()
        {
            try
            {
                using (var TempConnection = SqlClientFactory.Instance.CreateConnection())
                {
                    TempConnection.ConnectionString = MasterString;
                    using (var TempCommand = TempConnection.CreateCommand())
                    {
                        try
                        {
                            TempCommand.CommandText = "Create Database TestDatabase";
                            TempCommand.Open();
                            TempCommand.ExecuteNonQuery();
                        }
                        finally { TempCommand.Close(); }
                    }
                }
                using (var TempConnection = SqlClientFactory.Instance.CreateConnection())
                {
                    TempConnection.ConnectionString = MasterString;
                    using (var TempCommand = TempConnection.CreateCommand())
                    {
                        try
                        {
                            TempCommand.CommandText = "Create Database TestDatabaseForeignKeys";
                            TempCommand.Open();
                            TempCommand.ExecuteNonQuery();
                        }
                        finally { TempCommand.Close(); }
                    }
                }
                var Queries = new FileInfo("./Scripts/script.sql").Read().Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string Query in Queries)
                {
                    new SQLHelper(Configuration, SqlClientFactory.Instance)
                        .CreateBatch()
                        .AddQuery(CommandType.Text, Query)
                        .ExecuteScalar<int>();
                }
                Queries = new FileInfo("./Scripts/testdatabase.sql").Read().Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string Query in Queries)
                {
                    new SQLHelper(Configuration, SqlClientFactory.Instance, "Default2")
                        .CreateBatch()
                        .AddQuery(CommandType.Text, Query)
                        .ExecuteScalar<int>();
                }
            }
            catch { }
        }

        private void SetupIoC()
        {
            if (Canister.Builder.Bootstrapper == null)
            {
                var Container = Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                                                .AddAssembly(typeof(TestingFixture).GetTypeInfo().Assembly)
                                                .RegisterDataModeler()
                                                .RegisterSQLHelper()
                                                .RegisterFileCurator()
                                                .Build();
                Container.Register(Configuration, ServiceLifetime.Singleton);
            }
        }
    }
}