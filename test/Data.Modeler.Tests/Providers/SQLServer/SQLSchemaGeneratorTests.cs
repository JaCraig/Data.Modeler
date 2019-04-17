using BigBook;
using Data.Modeler.Providers.Interfaces;
using Data.Modeler.Providers.SQLServer;
using Data.Modeler.Tests.BaseClasses;
using SQLHelperDB.ExtensionMethods;
using SQLHelperDB.HelperClasses;
using System;
using System.Data;
using System.Data.SqlClient;
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
            Desired.Tables[0].AddColumn<int>("Column B", System.Data.DbType.Int32);
            var Result = TestObject.GenerateSchema(Desired, Source);
            Assert.NotNull(Result);
            Assert.Single(Result);
            Assert.Equal("ALTER TABLE [dbo].[Attachment] ADD [Column B] Int", Result[0]);
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
            Assert.Empty(Source.Functions);
            Assert.Equal("TestDatabase", Source.Name);
            Assert.Empty(Source.StoredProcedures);
            Assert.Empty(Source.Views);
        }

        [Fact]
        public void SchemaGenerationForeignKeysAlreadyExist()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default2"));
            var Destination = new Modeler.Providers.Source("Default2");
            var Table = Destination.AddTable("ConcreteClass3_", "dbo");
            var Column1 = Table.AddColumn<int>("IInterface1_ID_", typeof(int).To<Type, DbType>());
            var Table2 = Destination.AddTable("IInterface1_", "dbo");
            Table2.AddColumn<int>("ID_", typeof(int).To<Type, DbType>());
            Column1.AddForeignKey("IInterface1_", "ID_");
            Table.SetupForeignKeys();
            var Results = TestObject.GenerateSchema(Destination, Source);
            Assert.Empty(Results);
        }

        [Fact]
        public void SchemaGenerationTimeSpan()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default2"));
            var Destination = new Modeler.Providers.Source("Default2");
            var Table = Destination.AddTable("AllReferencesAndID_", "dbo");
            Table.AddColumn<TimeSpan>("TimeSpanValue_", typeof(TimeSpan).To<Type, DbType>());
            var Results = TestObject.GenerateSchema(Destination, Source);
            Assert.Empty(Results);
        }

        [Fact]
        public void SetupAlreadyExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default"));
            Source.Tables[0].AddColumn<int>("Column B", System.Data.DbType.Int32);
            TestObject.Setup(Source, new Connection(Configuration, SqlClientFactory.Instance, "", "Default"));
        }

        [Fact]
        public void SetupDoesntExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>());
            var Source = TestObject.GetSourceStructure(new Connection(Configuration, SqlClientFactory.Instance, "", "Default"));
            Source.Tables[0].AddColumn<int>("Column B", System.Data.DbType.Int32);
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