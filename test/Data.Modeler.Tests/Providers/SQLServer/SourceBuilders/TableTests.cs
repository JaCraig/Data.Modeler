using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.SourceBuilders
{
    public class TablesTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempTables = new Tables();
            Assert.NotNull(TempTables);
            Assert.Equal(10, TempTables.Order);
        }

        [Fact]
        public void FillSource()
        {
            var TempTables = new Tables();
            var TempSource = new Source("My Source");
            var ConstraintsToAdd = new[]
            {
                new Dynamo(new
                {
                    TABLE_NAME="Table A",
                    TABLE_TYPE="BASE TABLE"
                }),
                new Dynamo(new
                {
                    TABLE_NAME="View A",
                    TABLE_TYPE="VIEW"
                })
            };
            TempTables.FillSource(ConstraintsToAdd, TempSource);
            var Constraint = TempSource.Tables.First();
            Assert.Equal("Table A", Constraint.Name);
            var View = TempSource.Views.First();
            Assert.Equal("View A", View.Name);
        }

        [Fact]
        public void GetCommand()
        {
            var TempTables = new Tables();
            var SQLCommand = TempTables.GetCommand();
            Assert.Equal("SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES", SQLCommand);
        }
    }
}