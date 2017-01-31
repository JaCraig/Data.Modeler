using Data.Modeler.Providers;
using Data.Modeler.Tests.BaseClasses;
using Xunit;

namespace Data.Modeler.Tests.Providers
{
    public class SourceTests : TestingFixture
    {
        [Fact]
        public void AddFunction()
        {
            var TempSource = new Source("MySource");
            var Function = TempSource.AddFunction("FunctionName", "FunctionDefinition");
            Assert.Equal("FunctionName", Function.Name);
            Assert.Equal("FunctionDefinition", Function.Definition);
            Assert.Equal(TempSource, Function.Source);
        }

        [Fact]
        public void AddStoredProcedure()
        {
            var TempSource = new Source("MySource");
            var TempStoredProcedure = TempSource.AddStoredProcedure("ProcedureName", "ProcedureDefinition");
            Assert.Equal("ProcedureName", TempStoredProcedure.Name);
            Assert.Equal("ProcedureDefinition", TempStoredProcedure.Definition);
            Assert.Equal(TempSource, TempStoredProcedure.Source);
        }

        [Fact]
        public void AddTable()
        {
            var TempSource = new Source("MySource");
            var TempTable = TempSource.AddTable("TableName");
            Assert.Equal("TableName", TempTable.Name);
            Assert.Empty(TempTable.Columns);
            Assert.Empty(TempTable.Constraints);
            Assert.Equal(TempSource, TempTable.Source);
            Assert.Empty(TempTable.Triggers);
        }

        [Fact]
        public void AddView()
        {
            var TempSource = new Source("MySource");
            var TempView = TempSource.AddView("ViewName", "ViewDefinition");
            Assert.Equal("ViewName", TempView.Name);
            Assert.Equal(TempSource, TempView.Source);
            Assert.Equal("ViewDefinition", TempView.Definition);
        }

        [Fact]
        public void Creation()
        {
            var TempSource = new Source("MySource");
            Assert.Empty(TempSource.Functions);
            Assert.Equal("MySource", TempSource.Name);
            Assert.Empty(TempSource.StoredProcedures);
            Assert.Empty(TempSource.Tables);
            Assert.Empty(TempSource.Views);
        }
    }
}