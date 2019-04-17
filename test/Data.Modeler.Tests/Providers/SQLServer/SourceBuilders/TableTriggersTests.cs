using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.Enums;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.SourceBuilders
{
    public class TableTriggersTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempTableTriggers = new TableTriggers();
            Assert.NotNull(TempTableTriggers);
            Assert.Equal(30, TempTableTriggers.Order);
        }

        [Fact]
        public void FillSource()
        {
            var TempTableTriggers = new TableTriggers();
            var TempSource = new Source("My Source");
            var TempTable = TempSource.AddTable("Table A", "dbo");
            var ConstraintsToAdd = new[]
            {
                new Dynamo(new
                {
                    Table="Table A",
                    Name="Trigger A",
                    Type=1,
                    Definition="Definition A"
                })
            };
            TempTableTriggers.FillSource(ConstraintsToAdd, TempSource);
            var TempTable2 = TempSource.Tables[0];
            var Trigger = TempTable2.Triggers[0];
            Assert.Equal("Trigger A", Trigger.Name);
            Assert.Equal("Definition A", Trigger.Definition);
            Assert.Equal(TriggerType.Insert, Trigger.Type);
        }

        [Fact]
        public void GetCommand()
        {
            var TempTableTriggers = new TableTriggers();
            var SQLCommand = TempTableTriggers.GetCommand();
            Assert.Equal(@"SELECT sys.tables.name as [Table],sys.triggers.name as Name,sys.trigger_events.type as Type,
                                                                OBJECT_DEFINITION(sys.triggers.object_id) as Definition
                                                                FROM sys.triggers
                                                                INNER JOIN sys.trigger_events ON sys.triggers.object_id=sys.trigger_events.object_id
                                                                INNER JOIN sys.tables on sys.triggers.parent_id=sys.tables.object_id", SQLCommand);
        }
    }
}