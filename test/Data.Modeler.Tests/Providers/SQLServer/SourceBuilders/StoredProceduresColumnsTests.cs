using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Data;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.SourceBuilders
{
    public class StoredProcedureColumnsTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempStoredProcedureColumns = new StoredProcedureColumns();
            Assert.NotNull(TempStoredProcedureColumns);
            Assert.Equal(70, TempStoredProcedureColumns.Order);
        }

        [Fact]
        public void FillSource()
        {
            var TempStoredProcedureColumns = new StoredProcedureColumns();
            var TempSource = new Source("My Source");
            TempSource.AddStoredProcedure("Procedure A", "");
            var ConstraintsToAdd = new[]
            {
                new Dynamo(new
                {
                    Procedure="Procedure A",
                    TYPE="Int",
                    NAME="Column A",
                    LENGTH=4
                })
            };
            TempStoredProcedureColumns.FillSource(ConstraintsToAdd, TempSource);
            var Constraint = (StoredProcedure)TempSource.StoredProcedures.First();
            var Column = Constraint.Columns.First();
            Assert.Equal(DbType.Int32, Column.DataType);
            Assert.Equal("Column A", Column.Name);
            Assert.Equal(4, Column.Length);
        }

        [Fact]
        public void GetCommand()
        {
            var TempStoredProcedureColumns = new StoredProcedureColumns();
            var SQLCommand = TempStoredProcedureColumns.GetCommand();
            Assert.Equal(@"SELECT sys.procedures.name as [Procedure],sys.systypes.name as TYPE,sys.parameters.name as NAME,
sys.parameters.max_length as LENGTH,sys.parameters.default_value as [DEFAULT VALUE]
FROM sys.procedures
INNER JOIN sys.parameters on sys.procedures.object_id=sys.parameters.object_id
INNER JOIN sys.systypes on sys.systypes.xusertype=sys.parameters.system_type_id
WHERE sys.systypes.xusertype <> 256", SQLCommand);
        }
    }
}