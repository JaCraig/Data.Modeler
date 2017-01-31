using Data.Modeler.Providers;
using Data.Modeler.Tests.BaseClasses;
using Xunit;

namespace Data.Modeler.Tests.Providers
{
    public class CheckConstraintTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempConstraint = new CheckConstraint("Constraint_Name", "LEN(SomeColumn)>5", null);
            Assert.Equal("Constraint_Name", TempConstraint.Name);
            Assert.Equal("LEN(SomeColumn)>5", TempConstraint.Definition);
            Assert.Null(TempConstraint.ParentTable);
        }
    }
}