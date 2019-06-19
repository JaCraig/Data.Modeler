using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.SourceBuilders
{
    public class TableColumnsTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempTableColumns = new TableColumns();
            Assert.NotNull(TempTableColumns);
            Assert.Equal(20, TempTableColumns.Order);
        }

        [Fact]
        public void FillSource()
        {
            var TempTableColumns = new TableColumns();
            var TempSource = new Source("My Source");
            var TempTable = TempSource.AddTable("Table A", "dbo");
            var ConstraintsToAdd = new List<dynamic>
            {
                new Dynamo(new
                {
                    Table="Table A",
                    Definition="My Definition",
                    Column="Column A",
                    COLUMN_TYPE="Int",
                    MAX_LENGTH=4,
                    IS_NULLABLE=false,
                    IS_IDENTITY=false,
                    IS_INDEX=1,
                    PRIMARY_KEY="PK_Table_A",
                    UNIQUE="IX_Table_A",
                    FOREIGN_KEY_TABLE="",
                    FOREIGN_KEY_COLUMN="",
                    DEFAULT_VALUE="",
                    ComputedColumnSpecification=""
                })
            };
            TempTableColumns.FillSource(ConstraintsToAdd, TempSource);
            var TempTable2 = (Modeler.Providers.Table)TempSource.Tables[0];
            var Column = TempTable2.Columns[0];
            Assert.Equal("Column A", Column.Name);
            Assert.Equal(DbType.Int32, Column.DataType);
            Assert.False(Column.AutoIncrement);
            Assert.Equal("", Column.ComputedColumnSpecification);
            Assert.Equal("", Column.Default);
            Assert.Empty(Column.ForeignKey);
            Assert.True(Column.Index);
            Assert.Equal(4, Column.Length);
            Assert.False(Column.Nullable);
            Assert.False(Column.OnDeleteCascade);
            Assert.False(Column.OnDeleteSetNull);
            Assert.False(Column.OnUpdateCascade);
            Assert.Equal(TempTable, Column.ParentTable);
            Assert.True(Column.PrimaryKey);
            Assert.True(Column.Unique);
        }

        [Fact]
        public void GetCommand()
        {
            var TempTableColumns = new TableColumns();
            var SQLCommand = TempTableColumns.GetCommand();
            Assert.Equal(@"SELECT sys.tables.name as [Table],sys.columns.name AS [Column], sys.systypes.name AS [COLUMN_TYPE],
                                                        sys.columns.max_length as [MAX_LENGTH], sys.columns.is_nullable as [IS_NULLABLE],
                                                        sys.columns.is_identity as [IS_IDENTITY], sys.index_columns.index_id as [IS_INDEX],
                                                        key_constraints.name as [PRIMARY_KEY], key_constraints_1.name as [UNIQUE],
                                                        tables_1.name as [FOREIGN_KEY_TABLE], columns_1.name as [FOREIGN_KEY_COLUMN],
                                                        sys.default_constraints.definition as [DEFAULT_VALUE],sys.computed_columns.definition as [ComputedColumnSpecification]
                                                        FROM sys.tables
                                                        INNER JOIN sys.columns on sys.columns.object_id=sys.tables.object_id
                                                        INNER JOIN sys.systypes ON sys.systypes.xtype = sys.columns.system_type_id
                                                        LEFT OUTER JOIN sys.index_columns on sys.index_columns.object_id=sys.tables.object_id and sys.index_columns.column_id=sys.columns.column_id
                                                        LEFT OUTER JOIN sys.key_constraints on sys.key_constraints.parent_object_id=sys.tables.object_id and sys.key_constraints.parent_object_id=sys.index_columns.object_id and sys.index_columns.index_id=sys.key_constraints.unique_index_id and sys.key_constraints.type='PK'
                                                        LEFT OUTER JOIN sys.foreign_key_columns on sys.foreign_key_columns.parent_object_id=sys.tables.object_id and sys.foreign_key_columns.parent_column_id=sys.columns.column_id
                                                        LEFT OUTER JOIN sys.tables as tables_1 on tables_1.object_id=sys.foreign_key_columns.referenced_object_id
                                                        LEFT OUTER JOIN sys.columns as columns_1 on columns_1.column_id=sys.foreign_key_columns.referenced_column_id and columns_1.object_id=tables_1.object_id
                                                        LEFT OUTER JOIN sys.key_constraints as key_constraints_1 on key_constraints_1.parent_object_id=sys.tables.object_id and key_constraints_1.parent_object_id=sys.index_columns.object_id and sys.index_columns.index_id=key_constraints_1.unique_index_id and key_constraints_1.type='UQ'
                                                        LEFT OUTER JOIN sys.default_constraints on sys.default_constraints.object_id=sys.columns.default_object_id
                                                        LEFT OUTER JOIN sys.computed_columns ON sys.computed_columns.object_id=sys.columns.object_id and sys.computed_columns.column_id=sys.columns.column_id
                                                        WHERE sys.systypes.xusertype <> 256", SQLCommand);
        }
    }
}