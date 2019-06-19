using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Collections.Generic;
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
            var ConstraintsToAdd = new List<dynamic>
            {
                new Dynamo(new
                {
                    TABLE_NAME="Table A",
                    TABLE_TYPE="BASE TABLE",
                    TABLE_SCHEMA=""
                }),
                new Dynamo(new
                {
                    TABLE_NAME="View A",
                    TABLE_TYPE="VIEW",
                    TABLE_SCHEMA=""
                })
            };
            TempTables.FillSource(ConstraintsToAdd, TempSource);
            var Constraint = TempSource.Tables[0];
            Assert.Equal("Table A", Constraint.Name);
            var View = TempSource.Views[0];
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