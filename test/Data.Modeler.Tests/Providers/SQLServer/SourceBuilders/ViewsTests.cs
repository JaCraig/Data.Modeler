using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Data;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.SourceBuilders
{
    public class ViewsTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempViews = new Views();
            Assert.NotNull(TempViews);
            Assert.Equal(50, TempViews.Order);
        }

        [Fact]
        public void FillSource()
        {
            var TempViews = new Views();
            var TempSource = new Source("My Source");
            var TempTable = TempSource.AddView("View A", "dbo", "");
            var ConstraintsToAdd = new[]
            {
                new Dynamo(new
                {
                    View="View A",
                    Definition="My Definition",
                    Column="Column A",
                    COLUMN_TYPE="Int",
                    MAX_LENGTH=4,
                    IS_NULLABLE=false
                })
            };
            TempViews.FillSource(ConstraintsToAdd, TempSource);
            var TempTable2 = (View)TempSource.Views.First();
            var Column = TempTable2.Columns[0];
            Assert.Equal("Column A", Column.Name);
            Assert.Equal("My Definition", TempTable.Definition);
            Assert.Equal(DbType.Int32, Column.DataType);
        }

        [Fact]
        public void GetCommand()
        {
            var TempViews = new Views();
            var SQLCommand = TempViews.GetCommand();
            Assert.Equal(@"SELECT sys.views.name as [View],OBJECT_DEFINITION(sys.views.object_id) as Definition,
                                                        sys.columns.name AS [Column], sys.systypes.name AS [COLUMN_TYPE],
                                                        sys.columns.max_length as [MAX_LENGTH], sys.columns.is_nullable as [IS_NULLABLE]
                                                        FROM sys.views
                                                        INNER JOIN sys.columns on sys.columns.object_id=sys.views.object_id
                                                        INNER JOIN sys.systypes ON sys.systypes.xtype = sys.columns.system_type_id
                                                        WHERE sys.systypes.xusertype <> 256", SQLCommand);
        }
    }
}