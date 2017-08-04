using Data.Modeler.Providers.SQLServer.CommandBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.CommandBuilders
{
    public class CheckConstraintCommandBuilderTests : CommandBuilderTestBase
    {
        [Fact]
        public void Creation()
        {
            var TempCheckConstraint = new CheckConstraintCommandBuilder();
            Assert.NotNull(TempCheckConstraint);
            Assert.Equal(31, TempCheckConstraint.Order);
        }

        [Fact]
        public void GetCommandsNoCurrentSource()
        {
            var TempCheckConstraint = new CheckConstraintCommandBuilder();
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, null).ToList();
            Assert.Equal(1, Commands.Count());
            Assert.Equal("ALTER TABLE [dbo].[Table A] ADD CONSTRAINT [Constraint A] CHECK (My Definition2)", Commands[0]);
        }

        [Fact]
        public void GetCommandsWithCurrentSource()
        {
            var TempCheckConstraint = new CheckConstraintCommandBuilder();
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, CurrentSource).ToList();
            Assert.Equal(2, Commands.Count());
            Assert.Equal("ALTER TABLE [dbo].[Table A] DROP CONSTRAINT [Constraint A]", Commands[0]);
            Assert.Equal("ALTER TABLE [dbo].[Table A] ADD CONSTRAINT [Constraint A] CHECK (My Definition2)", Commands[1]);
        }
    }
}