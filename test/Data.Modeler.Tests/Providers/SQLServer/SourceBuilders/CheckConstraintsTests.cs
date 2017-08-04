using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.SourceBuilders
{
    public class CheckConstraintsTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempCheckConstraints = new CheckConstraints();
            Assert.NotNull(TempCheckConstraints);
            Assert.Equal(31, TempCheckConstraints.Order);
        }

        [Fact]
        public void FillSource()
        {
            var TempCheckConstraints = new CheckConstraints();
            var TempSource = new Source("My Source");
            var TableA = TempSource.AddTable("Table A", "dbo");
            var ConstraintsToAdd = new[]
            {
                new Dynamo(new
                {
                    Table="Table A",
                    Name="Constraint A",
                    Definition="(Definition A)"
                })
            };
            TempCheckConstraints.FillSource(ConstraintsToAdd, TempSource);
            var Constraint = TempSource.Tables.First().Constraints.First();
            Assert.Equal("Definition A", Constraint.Definition);
            Assert.Equal("Constraint A", Constraint.Name);
            Assert.Equal(TableA, Constraint.ParentTable);
        }

        [Fact]
        public void GetCommand()
        {
            var TempCheckConstraints = new CheckConstraints();
            var SQLCommand = TempCheckConstraints.GetCommand();
            Assert.Equal(@"SELECT sys.tables.name as [Table],sys.check_constraints.name as [Name],OBJECT_DEFINITION(sys.check_constraints.object_id) as [Definition]
FROM sys.check_constraints
INNER JOIN sys.tables ON sys.tables.object_id=sys.check_constraints.parent_object_id", SQLCommand);
        }
    }
}