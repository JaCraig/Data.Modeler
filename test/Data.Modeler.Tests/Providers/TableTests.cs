using Data.Modeler.Providers;
using Data.Modeler.Providers.Enums;
using Data.Modeler.Tests.BaseClasses;
using Xunit;

namespace Data.Modeler.Tests.Providers
{
    public class TableTests : TestingFixture
    {
        [Fact]
        public void AddCheckConstraint()
        {
            var TempTable = new Table("Name", "dbo", null);
            var CheckConstraint = TempTable.AddCheckConstraint("Name", "Definition");
            Assert.Empty(TempTable.Columns);
            Assert.NotEmpty(TempTable.Constraints);
            Assert.Contains(CheckConstraint, TempTable.Constraints);
            Assert.Equal("Name", TempTable.Name);
            Assert.Null(TempTable.Source);
            Assert.Empty(TempTable.Triggers);
            Assert.Equal("Name", CheckConstraint.Name);
            Assert.Equal("Definition", CheckConstraint.Definition);
            Assert.Equal(TempTable, CheckConstraint.ParentTable);
        }

        [Fact]
        public void AddColumn()
        {
            var TempTable = new Table("Name", "dbo", null);
            var TempColumn = TempTable.AddColumn<int>("A", System.Data.DbType.Int32, 0, true, true, true, true, true, null, null, 1, null, true, true, true);
            Assert.NotEmpty(TempTable.Columns);
            Assert.Contains(TempColumn, TempTable.Columns);
            Assert.Empty(TempTable.Constraints);
            Assert.Equal("Name", TempTable.Name);
            Assert.Null(TempTable.Source);
            Assert.Empty(TempTable.Triggers);

            Assert.True(TempColumn.AutoIncrement);
            Assert.Equal(System.Data.DbType.Int32, TempColumn.DataType);
            Assert.Equal("1", TempColumn.Default);
            Assert.Empty(TempColumn.ForeignKey);
            Assert.True(TempColumn.Index);
            Assert.Equal(0, TempColumn.Length);
            Assert.Equal("A", TempColumn.Name);
            Assert.True(TempColumn.Nullable);
            Assert.True(TempColumn.OnDeleteCascade);
            Assert.True(TempColumn.OnDeleteSetNull);
            Assert.True(TempColumn.OnUpdateCascade);
            Assert.Equal(TempTable, TempColumn.ParentTable);
            Assert.True(TempColumn.PrimaryKey);
            Assert.True(TempColumn.Unique);
            Assert.Null(TempColumn.ComputedColumnSpecification);
        }

        [Fact]
        public void AddForeignKey()
        {
            var TempTable = new Table("Name", "dbo", null);
            TempTable.AddForeignKey("ColumnName", "ForeignKeyTable", "ForeignKeyColumn");
            Assert.Empty(TempTable.Columns);
            Assert.Empty(TempTable.Constraints);
            Assert.Equal("Name", TempTable.Name);
            Assert.Null(TempTable.Source);
            Assert.Empty(TempTable.Triggers);
        }

        [Fact]
        public void AddTrigger()
        {
            var TempTable = new Table("Name", "dbo", null);
            var Trigger = TempTable.AddTrigger("Name", "Definition", TriggerType.Update);
            Assert.Empty(TempTable.Columns);
            Assert.Empty(TempTable.Constraints);
            Assert.Equal("Name", TempTable.Name);
            Assert.Null(TempTable.Source);
            Assert.NotEmpty(TempTable.Triggers);
            Assert.Contains(Trigger, TempTable.Triggers);
            Assert.Equal("Definition", Trigger.Definition);
            Assert.Equal("Name", Trigger.Name);
            Assert.Equal(TempTable, Trigger.ParentTable);
            Assert.Equal(TriggerType.Update, Trigger.Type);
        }

        [Fact]
        public void ContainsColumn()
        {
            var TempTable = new Table("Name", "dbo", null);
            var TempColumn = TempTable.AddColumn<int>("A", System.Data.DbType.Int32, 0, true, true, true, true, true, null, null, 1, null, true, true, true);
            Assert.True(TempTable.ContainsColumn("A"));
            Assert.False(TempTable.ContainsColumn("B"));
        }

        [Fact]
        public void Creation()
        {
            var TempTable = new Table("Name", "dbo", null);
            Assert.Empty(TempTable.Columns);
            Assert.Empty(TempTable.Constraints);
            Assert.Equal("Name", TempTable.Name);
            Assert.Null(TempTable.Source);
            Assert.Empty(TempTable.Triggers);
        }
    }
}