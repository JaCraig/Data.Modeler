using Data.Modeler.Providers;
using Data.Modeler.Tests.BaseClasses;
using System;
using Xunit;

namespace Data.Modeler.Tests.Providers
{
    public class ColumnTests : TestingFixture
    {
        [Fact]
        public void Bool()
        {
            var TempColumn = new Column<bool>("A", System.Data.DbType.Int32, 0, true, true, true, true, true, null, null, default, null, true, true, true, null);
            Assert.True(TempColumn.AutoIncrement);
            Assert.Equal(System.Data.DbType.Int32, TempColumn.DataType);
            Assert.Equal("", TempColumn.Default);
            Assert.Empty(TempColumn.ForeignKey);
            Assert.True(TempColumn.Index);
            Assert.Equal(0, TempColumn.Length);
            Assert.Equal("A", TempColumn.Name);
            Assert.True(TempColumn.Nullable);
            Assert.True(TempColumn.OnDeleteCascade);
            Assert.True(TempColumn.OnDeleteSetNull);
            Assert.True(TempColumn.OnUpdateCascade);
            Assert.Null(TempColumn.ParentTable);
            Assert.True(TempColumn.PrimaryKey);
            Assert.True(TempColumn.Unique);
            Assert.Null(TempColumn.ComputedColumnSpecification);
        }

        [Fact]
        public void Creation()
        {
            var TempColumn = new Column<int>("A", System.Data.DbType.Int32, 0, true, true, true, true, true, null, null, 1, null, true, true, true, null);
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
            Assert.Null(TempColumn.ParentTable);
            Assert.True(TempColumn.PrimaryKey);
            Assert.True(TempColumn.Unique);
            Assert.Null(TempColumn.ComputedColumnSpecification);
        }

        [Fact]
        public void DateTime()
        {
            var TempColumn = new Column<DateTime>("A", System.Data.DbType.Int32, 0, true, true, true, true, true, null, null, default, null, true, true, true, null);
            Assert.True(TempColumn.AutoIncrement);
            Assert.Equal(System.Data.DbType.Int32, TempColumn.DataType);
            Assert.Equal("", TempColumn.Default);
            Assert.Empty(TempColumn.ForeignKey);
            Assert.True(TempColumn.Index);
            Assert.Equal(0, TempColumn.Length);
            Assert.Equal("A", TempColumn.Name);
            Assert.True(TempColumn.Nullable);
            Assert.True(TempColumn.OnDeleteCascade);
            Assert.True(TempColumn.OnDeleteSetNull);
            Assert.True(TempColumn.OnUpdateCascade);
            Assert.Null(TempColumn.ParentTable);
            Assert.True(TempColumn.PrimaryKey);
            Assert.True(TempColumn.Unique);
            Assert.Null(TempColumn.ComputedColumnSpecification);
        }

        [Fact]
        public void TimeSpan()
        {
            var TempColumn = new Column<TimeSpan>("A", System.Data.DbType.Int32, 0, true, true, true, true, true, null, null, default, null, true, true, true, null);
            Assert.True(TempColumn.AutoIncrement);
            Assert.Equal(System.Data.DbType.Int32, TempColumn.DataType);
            Assert.Equal("", TempColumn.Default);
            Assert.Empty(TempColumn.ForeignKey);
            Assert.True(TempColumn.Index);
            Assert.Equal(0, TempColumn.Length);
            Assert.Equal("A", TempColumn.Name);
            Assert.True(TempColumn.Nullable);
            Assert.True(TempColumn.OnDeleteCascade);
            Assert.True(TempColumn.OnDeleteSetNull);
            Assert.True(TempColumn.OnUpdateCascade);
            Assert.Null(TempColumn.ParentTable);
            Assert.True(TempColumn.PrimaryKey);
            Assert.True(TempColumn.Unique);
            Assert.Null(TempColumn.ComputedColumnSpecification);
        }
    }
}