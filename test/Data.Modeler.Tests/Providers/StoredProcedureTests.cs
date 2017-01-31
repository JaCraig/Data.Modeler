using Data.Modeler.Providers;
using Data.Modeler.Tests.BaseClasses;
using Xunit;

namespace Data.Modeler.Tests.Providers
{
    public class StoredProcedureTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempStoredProcedure = new StoredProcedure("Name", "Definition", null);
            Assert.Empty(TempStoredProcedure.Columns);
            Assert.Empty(TempStoredProcedure.Constraints);
            Assert.Equal("Definition", TempStoredProcedure.Definition);
            Assert.Equal("Name", TempStoredProcedure.Name);
            Assert.Null(TempStoredProcedure.Source);
            Assert.Empty(TempStoredProcedure.Triggers);
        }
    }
}