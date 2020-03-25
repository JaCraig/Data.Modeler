using Data.Modeler.Tests.BaseClasses;
using System.Data.SqlClient;
using Xunit;

namespace Data.Modeler.Tests
{
    public class DataModelerTests : TestingFixture
    {
        [Fact]
        public void CreateSource()
        {
            var Source = DataModeler.CreateSource("MyTestDatabase");
            Assert.Empty(Source.Functions);
            Assert.Equal("MyTestDatabase", Source.Name);
            Assert.Empty(Source.StoredProcedures);
            Assert.Empty(Source.Tables);
            Assert.Empty(Source.Views);
        }

        [Fact]
        public void GetSchemaGenerator()
        {
            var Generator = Canister.Builder.Bootstrapper.Resolve<DataModeler>().GetSchemaGenerator(SqlClientFactory.Instance);
            Assert.Equal(SqlClientFactory.Instance, Generator.Provider);
        }
    }
}