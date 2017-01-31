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
            Assert.Equal(0, Source.Functions.Count);
            Assert.Equal("MyTestDatabase", Source.Name);
            Assert.Equal(0, Source.StoredProcedures.Count);
            Assert.Equal(0, Source.Tables.Count);
            Assert.Equal(0, Source.Views.Count);
        }

        [Fact]
        public void GetSchemaGenerator()
        {
            var Generator = DataModeler.GetSchemaGenerator(SqlClientFactory.Instance);
            Assert.Equal(SqlClientFactory.Instance, Generator.Provider);
        }
    }
}