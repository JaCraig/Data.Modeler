using Data.Modeler.Providers;
using Data.Modeler.Tests.BaseClasses;
using Xunit;

namespace Data.Modeler.Tests.Providers
{
    public class ViewTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempView = new View("Name", "Definition", null);
            Assert.Empty(TempView.Columns);
            Assert.Empty(TempView.Constraints);
            Assert.Equal("Definition", TempView.Definition);
            Assert.Equal("Name", TempView.Name);
            Assert.Null(TempView.Source);
            Assert.Empty(TempView.Triggers);
        }
    }
}