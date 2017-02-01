using Data.Modeler.Providers.SQLServer.CommandBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.CommandBuilders
{
    public class TriggerCommandBuilderTests : CommandBuilderTestBase
    {
        [Fact]
        public void Creation()
        {
            var TempCheckConstraint = new TriggerCommandBuilder();
            Assert.NotNull(TempCheckConstraint);
            Assert.Equal(30, TempCheckConstraint.Order);
        }

        [Fact]
        public void GetCommandsNoCurrentSource()
        {
            var TempCheckConstraint = new TriggerCommandBuilder();
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, null).ToList();
            Assert.Equal(1, Commands.Count());
            Assert.Equal("My Definition 2", Commands[0]);
        }

        [Fact]
        public void GetCommandsWithCurrentSource()
        {
            var TempCheckConstraint = new TriggerCommandBuilder();
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, CurrentSource).ToList();
            Assert.Equal(2, Commands.Count());
            Assert.Equal("DROP TRIGGER [Trigger A]", Commands[0]);
            Assert.Equal("My Definition 2", Commands[1]);
        }
    }
}