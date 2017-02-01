using BigBook;
using Data.Modeler.Providers;
using Data.Modeler.Providers.SQLServer.SourceBuilders;
using Data.Modeler.Tests.BaseClasses;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.SourceBuilders
{
    public class FunctionsTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempFunctions = new Functions();
            Assert.NotNull(TempFunctions);
            Assert.Equal(80, TempFunctions.Order);
        }

        [Fact]
        public void FillSource()
        {
            var TempFunctions = new Functions();
            var TempSource = new Source("My Source");
            var ConstraintsToAdd = new[]
            {
                new Dynamo(new
                {
                    NAME="Constraint A",
                    DEFINITION="Definition A"
                })
            };
            TempFunctions.FillSource(ConstraintsToAdd, TempSource);
            var Constraint = TempSource.Functions.First();
            Assert.Equal("Definition A", Constraint.Definition);
            Assert.Equal("Constraint A", Constraint.Name);
        }

        [Fact]
        public void GetCommand()
        {
            var TempFunctions = new Functions();
            var SQLCommand = TempFunctions.GetCommand();
            Assert.Equal("SELECT SPECIFIC_NAME as NAME,ROUTINE_DEFINITION as DEFINITION FROM INFORMATION_SCHEMA.ROUTINES WHERE INFORMATION_SCHEMA.ROUTINES.ROUTINE_TYPE='FUNCTION'", SQLCommand);
        }
    }
}