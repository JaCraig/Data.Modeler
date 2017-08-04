using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.SourceBuilders
{
    public class StoredProceduresTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempStoredProcedures = new StoredProcedures();
            Assert.NotNull(TempStoredProcedures);
            Assert.Equal(60, TempStoredProcedures.Order);
        }

        [Fact]
        public void FillSource()
        {
            var TempStoredProcedures = new StoredProcedures();
            var TempSource = new Source("My Source");
            var ConstraintsToAdd = new[]
            {
                new Dynamo(new
                {
                    NAME="Constraint A",
                    DEFINITION="Definition A"
                })
            };
            TempStoredProcedures.FillSource(ConstraintsToAdd, TempSource);
            var Constraint = TempSource.StoredProcedures.First();
            Assert.Equal("Definition A", Constraint.Definition);
            Assert.Equal("Constraint A", Constraint.Name);
        }

        [Fact]
        public void GetCommand()
        {
            var TempStoredProcedures = new StoredProcedures();
            var SQLCommand = TempStoredProcedures.GetCommand();
            Assert.Equal(@"SELECT sys.schemas.name as [SCHEMA],
sys.procedures.name as NAME,
OBJECT_DEFINITION(sys.procedures.object_id) as DEFINITION
FROM sys.procedures
INNER JOIN sys.schemas ON sys.schemas.schema_id=sys.procedures.schema_id", SQLCommand);
        }
    }
}