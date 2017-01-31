using Data.Modeler.Providers;
using Data.Modeler.Providers.Enums;
using Data.Modeler.Tests.BaseClasses;
using Xunit;

namespace Data.Modeler.Tests.Providers
{
    public class TriggerTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempTrigger = new Trigger("Name", "Definition", TriggerType.Insert, null);
            Assert.Equal("Definition", TempTrigger.Definition);
            Assert.Equal("Name", TempTrigger.Name);
            Assert.Null(TempTrigger.ParentTable);
            Assert.Equal(TriggerType.Insert, TempTrigger.Type);
        }
    }
}