using Data.Modeler.Tests.BaseClasses;
using Microsoft.Extensions.DependencyInjection;
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
            var Generator = GetServiceProvider().GetService<DataModeler>().GetSchemaGenerator(SqlClientFactory.Instance);
            Assert.Contains(SqlClientFactory.Instance, Generator.Providers);
        }
    }
}