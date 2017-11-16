using Data.Modeler.Providers.SQLServer.CommandBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.CommandBuilders
{
    public class CreateSchemaCommandBuilderTests : CommandBuilderTestBase
    {
        [Fact]
        public void Creation()
        {
            var TempCheckConstraint = new CreateSchemaCommandBuilder();
            Assert.NotNull(TempCheckConstraint);
            Assert.Equal(5, TempCheckConstraint.Order);
        }

        [Fact]
        public void GetCommandsNoCurrentSource()
        {
            var TempCheckConstraint = new CreateSchemaCommandBuilder();
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, null).ToList();
            Assert.Single(Commands);
            Assert.Equal("CREATE SCHEMA SchemaA", Commands[0]);
        }

        [Fact]
        public void GetCommandsWithCurrentSource()
        {
            DesiredSource.Schemas.Add("SchemaB");
            var TempCheckConstraint = new CreateSchemaCommandBuilder();
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, CurrentSource).ToList();
            Assert.Single(Commands);
            Assert.Equal("CREATE SCHEMA SchemaB", Commands[0]);
        }
    }
}