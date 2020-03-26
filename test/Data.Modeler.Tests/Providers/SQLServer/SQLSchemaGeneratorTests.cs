using BigBook;
using Data.Modeler.Providers.Interfaces;
using Data.Modeler.Providers.SQLServer;
using Data.Modeler.Tests.BaseClasses;
using Microsoft.Extensions.Configuration;
using SQLHelperDB.ExtensionMethods;
using SQLHelperDB.HelperClasses;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer
{
    public class SQLServerSchemaGeneratorTests : TestingFixture
    {
        [Fact]
        public async Task ConstraintExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            Assert.False(await TestObject.ConstraintExistsAsync("Test_Constraint", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false));
        }

        [Fact]
        public void Creation()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            Assert.Equal(SqlClientFactory.Instance, TestObject.Provider);
        }

        [Fact]
        public async Task GenerateSchemaChanges()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            var Source = await TestObject.GetSourceStructureAsync(new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false);
            var Desired = Source.Copy();
            Desired.Tables[0].AddColumn<int>("Column B", System.Data.DbType.Int32);
            var Result = TestObject.GenerateSchema(Desired, Source);
            Assert.NotNull(Result);
            Assert.Single(Result);
            Assert.Equal("ALTER TABLE [dbo].[Attachment] ADD [Column B] Int", Result[0]);
        }

        [Fact]
        public async Task GenerateSchemaNoChanges()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            var Source = await TestObject.GetSourceStructureAsync(new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false);
            var Result = TestObject.GenerateSchema(Source, Source);
            Assert.NotNull(Result);
            Assert.Empty(Result);
        }

        [Fact]
        public async Task GetSourceStructure()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            var Source = await TestObject.GetSourceStructureAsync(new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false);
            Assert.Equal(6, Source.Tables.Count);
            Assert.Empty(Source.Functions);
            Assert.Equal("TestDatabase", Source.Name);
            Assert.Empty(Source.StoredProcedures);
            Assert.Empty(Source.Views);
        }

        [Fact]
        public async Task SchemaGenerationForeignKeysAlreadyExist()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            var Source = await TestObject.GetSourceStructureAsync(new Connection(Configuration, SqlClientFactory.Instance, "", "Default2")).ConfigureAwait(false);
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
        public async Task SchemaGenerationTimeSpan()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            var Source = await TestObject.GetSourceStructureAsync(new Connection(Configuration, SqlClientFactory.Instance, "", "Default2")).ConfigureAwait(false);
            var Destination = new Modeler.Providers.Source("Default2");
            var Table = Destination.AddTable("AllReferencesAndID_", "dbo");
            Table.AddColumn<TimeSpan>("TimeSpanValue_", typeof(TimeSpan).To<Type, DbType>());
            var Results = TestObject.GenerateSchema(Destination, Source);
            Assert.Empty(Results);
        }

        [Fact]
        public async Task SetupAlreadyExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            var Source = await TestObject.GetSourceStructureAsync(new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false);
            Source.Tables[0].AddColumn<int>("Column B", System.Data.DbType.Int32);
            await TestObject.SetupAsync(Source, new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false);
        }

        [Fact]
        public async Task SetupDoesntExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            var Source = await TestObject.GetSourceStructureAsync(new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false);
            Source.Tables[0].AddColumn<int>("Column B", System.Data.DbType.Int32);
            Source.Name = "TestDatabase2";
            await TestObject.SetupAsync(Source, new Connection(Configuration, SqlClientFactory.Instance, "", "DefaultNew")).ConfigureAwait(false);
            await TestObject.SourceExistsAsync("TestDatabase2", new Connection(Configuration, SqlClientFactory.Instance, "", "DefaultNew")).ConfigureAwait(false);

            using var TempConnection = SqlClientFactory.Instance.CreateConnection();
            TempConnection.ConnectionString = MasterString;
            using var TempCommand = TempConnection.CreateCommand();
            try
            {
                TempCommand.CommandText = "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2";
                TempCommand.Open();
                TempCommand.ExecuteNonQuery();
            }
            finally { TempCommand.Close(); }
        }

        [Fact]
        public async Task SourceExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            Assert.True(await TestObject.SourceExistsAsync("TestDatabase", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false));
        }

        [Fact]
        public async Task StoredProcedureExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            Assert.False(await TestObject.StoredProcedureExistsAsync("TestSP", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false));
        }

        [Fact]
        public async Task TableExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            Assert.True(await TestObject.TableExistsAsync("Attachment", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false));
        }

        [Fact]
        public async Task TriggerExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            Assert.False(await TestObject.TriggerExistsAsync("TestTrigger", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false));
        }

        [Fact]
        public async Task ViewExists()
        {
            var TestObject = new SQLServerSchemaGenerator(Canister.Builder.Bootstrapper.ResolveAll<ISourceBuilder>(), Canister.Builder.Bootstrapper.ResolveAll<ICommandBuilder>(), Canister.Builder.Bootstrapper.Resolve<IConfiguration>(), Helper, Helper);
            Assert.False(await TestObject.ViewExistsAsync("TestView", new Connection(Configuration, SqlClientFactory.Instance, "", "Default")).ConfigureAwait(false));
        }
    }
}