using Data.Modeler.Providers.SQLServer.CommandBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.CommandBuilders
{
    public class CreateFunctionCommandBuilderTests : CommandBuilderTestBase
    {
        [Fact]
        public void Creation()
        {
            var TempCheckConstraint = new CreateFunctionCommandBuilder();
            Assert.NotNull(TempCheckConstraint);
            Assert.Equal(40, TempCheckConstraint.Order);
        }

        [Fact]
        public void GetCommandsNoCurrentSource()
        {
            var TempCheckConstraint = new CreateFunctionCommandBuilder();
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, null).ToList();
            Assert.Single(Commands);
            Assert.Equal("My Definition 2", Commands[0]);
        }

        [Fact]
        public void GetCommandsWithCurrentSource()
        {
            var TempCheckConstraint = new CreateFunctionCommandBuilder();
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, CurrentSource).ToList();
            Assert.Equal(2, Commands.Count);
            Assert.Equal("DROP FUNCTION [dbo].[Function A]", Commands[0]);
            Assert.Equal("My Definition 2", Commands[1]);
        }
    }
}