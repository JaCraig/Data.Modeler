using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.SourceBuilders
{
    public class TableForeignKeysTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempTableForeignKeys = new TableForeignKeys();
            Assert.NotNull(TempTableForeignKeys);
            Assert.Equal(40, TempTableForeignKeys.Order);
        }

        [Fact]
        public void FillSource()
        {
            var TempTableForeignKeys = new TableForeignKeys();
            var TempSource = new Source("My Source");
            var TempTable = TempSource.AddTable("Table A", "dbo");
            TempTable.AddColumn<int>("Column A", DbType.Int32, foreignKeyTable: "Table B", foreignKeyColumn: "Column B");
            var TempForeignTable = TempSource.AddTable("Table B", "dbo");
            TempForeignTable.AddColumn<int>("Column B", DbType.Int32);
            var ConstraintsToAdd = new List<dynamic>
            {
                new Dynamo(new                {                })
            };
            TempTableForeignKeys.FillSource(ConstraintsToAdd, TempSource);
            var TempTable2 = (Modeler.Providers.Table)TempSource.Tables.First(x => x.Name == "Table A");
            Assert.Equal(TempForeignTable.Columns[0], TempTable2.Columns[0].ForeignKey[0]);
        }

        [Fact]
        public void GetCommand()
        {
            var TempTableForeignKeys = new TableForeignKeys();
            var SQLCommand = TempTableForeignKeys.GetCommand();
            Assert.Equal("SELECT sys.tables.name as [Table] FROM sys.tables", SQLCommand);
        }
    }
}