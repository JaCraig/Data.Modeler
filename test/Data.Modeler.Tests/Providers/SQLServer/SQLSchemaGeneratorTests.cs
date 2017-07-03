using BigBook;
using Data.Modeler.Providers.Interfaces;
using Data.Modeler.Providers.SQLServer;
using Data.Modeler.Tests.BaseClasses;
using SQLHelper.ExtensionMethods;
using SQLHelper.HelperClasses;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer
{
    public class SQLServerSchemaGeneratorTests : TestingFixture
    {
        [Fact]
        public void ConstraintExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            Assert.False(TestObject.ConstraintExists("Test_Constraint", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")));
        }

        [Fact]
        public void Creation()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            Assert.Equal(SqlClientFactory.Instance, TestObject.Provider);
        }

        [Fact]
        public void GenerateSchemaChanges()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default"));
            var Desired = Source.Copy();
            Desired.Tables.First().AddColumn<int>("Column B", System.Data.DbType.Int32);
            var Result = TestObject.GenerateSchema(Desired, Source);
            Assert.NotNull(Result);
            Assert.Equal(1, Result.Count());
            Assert.Equal("ALTER TABLE [Attachment] ADD [Column B] Int", Result.First());
        }

        [Fact]
        public void GenerateSchemaNoChanges()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default"));
            var Result = TestObject.GenerateSchema(Source, Source);
            Assert.NotNull(Result);
            Assert.Empty(Result);
        }

        [Fact]
        public void GetSourceStructure()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default"));
            Assert.Equal(6, Source.Tables.Count);
            Assert.Equal(0, Source.Functions.Count);
            Assert.Equal("TestDatabase", Source.Name);
            Assert.Equal(0, Source.StoredProcedures.Count);
            Assert.Equal(0, Source.Views.Count);
        }

        [Fact]
        public void SchemaGenerationForeignKeysAlreadyExist()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default2"));
            var Destination = new Modeler.Providers.Source("Default2");
            var Table = Destination.AddTable("ConcreteClass3_");
            var Column1 = Table.AddColumn<int>("IInterface1_ID_", typeof(int).To<Type, DbType>());
            var Table2 = Destination.AddTable("IInterface1_");
            Table2.AddColumn<int>("ID_", typeof(int).To<Type, DbType>());
            Column1.AddForeignKey("IInterface1_", "ID_");
            Table.SetupForeignKeys();
            var Results = TestObject.GenerateSchema(Destination, Source);
            Assert.Equal(0, Results.Count());
        }

        [Fact]
        public void SchemaGenerationTimeSpan()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default2"));
            var Destination = new Modeler.Providers.Source("Default2");
            var Table = Destination.AddTable("AllReferencesAndID_");
            Table.AddColumn<TimeSpan>("TimeSpanValue_", typeof(TimeSpan).To<Type, DbType>());
            var Results = TestObject.GenerateSchema(Destination, Source);
            Assert.Equal(0, Results.Count());
        }

        [Fact]
        public void SetupAlreadyExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default"));
            Source.Tables.First().AddColumn<int>("Column B", System.Data.DbType.Int32);
            TestObject.Setup(Source, new Connection(Configuration, SqlClientFactory.Instance, "", "Default"));
        }

        [Fact]
        public void SetupDoesntExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default"));
            Source.Tables.First().AddColumn<int>("Column B", System.Data.DbType.Int32);
            Source.Name = "TestDatabase2";
            TestObject.Setup(Source, new Connection(Configuration, SqlClientFactory.Instance, "", "DefaultNew"));
            TestObject.SourceExists("TestDatabase2", new Connection(Configuration, SqlClientFactory.Instance, "", "DefaultNew"));

            using (var TempConnection = SqlClientFactory.Instance.CreateConnection())
            {
                TempConnection.ConnectionString = MasterString;
                using (var TempCommand = TempConnection.CreateCommand())
                {
                    try
                    {
                        TempCommand.CommandText = "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2";
                        TempCommand.Open();
                        TempCommand.ExecuteNonQuery();
                    }
                    finally { TempCommand.Close(); }
                }
            }
        }

        [Fact]
        public void SourceExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            Assert.True(TestObject.SourceExists("TestDatabase", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")));
        }

        [Fact]
        public void StoredProcedureExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            Assert.False(TestObject.StoredProcedureExists("TestSP", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")));
        }

        [Fact]
        public void TableExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            Assert.True(TestObject.TableExists("Attachment", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")));
        }

        [Fact]
        public void TriggerExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            Assert.False(TestObject.TriggerExists("TestTrigger", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")));
        }

        [Fact]
        public void ViewExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            Assert.False(TestObject.ViewExists("TestView", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")));
        }
    }
}