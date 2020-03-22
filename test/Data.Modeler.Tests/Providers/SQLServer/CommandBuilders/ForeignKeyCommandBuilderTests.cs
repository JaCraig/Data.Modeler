using Data.Modeler.Providers.SQLServer.CommandBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.CommandBuilders
{
    public class ForeignKeyCommandBuilderTests : CommandBuilderTestBase
    {
        [Fact]
        public void Creation()
        {
            var TempCheckConstraint = new ForeignKeyCommandBuilder(ObjectPool);
            Assert.NotNull(TempCheckConstraint);
            Assert.Equal(20, TempCheckConstraint.Order);
        }

        [Fact]
        public void GetCommandsNoCurrentSource()
        {
            var TempCheckConstraint = new ForeignKeyCommandBuilder(ObjectPool);
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, null).ToList();
            Assert.Single(Commands);
            Assert.Equal("ALTER TABLE [dbo].[Table A] ADD FOREIGN KEY ([Column A]) REFERENCES [dbo].[Foreign Table]([Foreign Column])", Commands[0]);
        }

        [Fact]
        public void GetCommandsWithCurrentSource()
        {
            var TempCheckConstraint = new ForeignKeyCommandBuilder(ObjectPool);
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, CurrentSource).ToList();
            Assert.Single(Commands);
            Assert.Equal("ALTER TABLE [dbo].[Table A] ADD FOREIGN KEY ([Column A]) REFERENCES [dbo].[Foreign Table]([Foreign Column])", Commands[0]);
        }
    }
}