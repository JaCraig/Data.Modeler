using Data.Modeler.Providers.SQLServer.CommandBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.CommandBuilders
{
    public class CreateDatabaseCommandTests : CommandBuilderTestBase
    {
        [Fact]
        public void Creation()
        {
            var TempCheckConstraint = new CreateDatabaseCommandBuilder(ObjectPool);
            Assert.NotNull(TempCheckConstraint);
            Assert.Equal(1, TempCheckConstraint.Order);
        }

        [Fact]
        public void GetCommandsNoCurrentSource()
        {
            var TempCheckConstraint = new CreateDatabaseCommandBuilder(ObjectPool);
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, null).ToList();
            Assert.Single(Commands);
            Assert.Equal("CREATE DATABASE [My Data]", Commands[0]);
        }

        [Fact]
        public void GetCommandsWithCurrentSource()
        {
            var TempCheckConstraint = new CreateDatabaseCommandBuilder(ObjectPool);
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, CurrentSource).ToList();
            Assert.Empty(Commands);
        }
    }
}