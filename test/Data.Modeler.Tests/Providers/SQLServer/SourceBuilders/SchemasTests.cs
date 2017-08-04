using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.SourceBuilders
{
    public class SchemasTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempSchemas = new Schemas();
            Assert.NotNull(TempSchemas);
            Assert.Equal(5, TempSchemas.Order);
        }

        [Fact]
        public void FillSource()
        {
            var TempSchemas = new Schemas();
            var TempSource = new Source("My Source");
            var ConstraintsToAdd = new[]
            {
                new Dynamo(new
                {
                    Name="SchemaA"
                })
            };
            TempSchemas.FillSource(ConstraintsToAdd, TempSource);
            var Constraint = TempSource.Schemas.First();
            Assert.Equal("SchemaA", Constraint);
        }

        [Fact]
        public void GetCommand()
        {
            var TempSchemas = new Schemas();
            var SQLCommand = TempSchemas.GetCommand();
            Assert.Equal(@"SELECT name as [Name] FROM sys.schemas WHERE schema_id < 16384 AND schema_id > 4", SQLCommand);
        }
    }
}