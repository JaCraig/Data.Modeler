using Data.Modeler.Providers;
using Data.Modeler.Tests.BaseClasses;
using Xunit;

namespace Data.Modeler.Tests.Providers
{
    public class FunctionTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempFunction = new Function("Function_Name", "SOMEDEFINITION", null);
            Assert.Equal("Function_Name", TempFunction.Name);
            Assert.Equal("SOMEDEFINITION", TempFunction.Definition);
            Assert.Null(TempFunction.Source);
        }
    }
}