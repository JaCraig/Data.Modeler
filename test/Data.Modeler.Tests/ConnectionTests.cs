using Data.Modeler.Tests.BaseClasses;
using System.Data.SqlClient;
using Xunit;

namespace Data.Modeler.Tests
{
    public class ConnectionTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TempConnection = new Connection(Configuration, SqlClientFactory.Instance, ConnectionString);
            Assert.Equal(Configuration, TempConnection.Configuration);
            Assert.Equal(ConnectionString, TempConnection.ConnectionString);
            Assert.Equal(DatabaseName, TempConnection.DatabaseName);
            Assert.Equal(SqlClientFactory.Instance, TempConnection.Factory);
        }

        [Fact]
        public void CreationWithConnectionStringName()
        {
            var TempConnection = new Connection(Configuration, SqlClientFactory.Instance, "Default");
            Assert.Equal(Configuration, TempConnection.Configuration);
            Assert.Equal(ConnectionString, TempConnection.ConnectionString);
            Assert.Equal(DatabaseName, TempConnection.DatabaseName);
            Assert.Equal(SqlClientFactory.Instance, TempConnection.Factory);
        }
    }
}