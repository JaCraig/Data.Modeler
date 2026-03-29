using Data.Modeler.Tests.Utils;
using FileCurator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using SQLHelperDB;
using SQLHelperDB.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Data.Modeler.Tests.BaseClasses
{
    [Collection("DirectoryCollection")]
    public class TestingFixture : IDisposable
    {
        public TestingFixture()
        {
            SetupConfiguration();
            SetupIoC();
            var TempTask = Task.Run(async () => await SetupDatabasesAsync().ConfigureAwait(false));
            TempTask.GetAwaiter().GetResult();
        }

        public static IConfiguration Configuration { get; set; }
        protected static Aspectus.Aspectus Aspectus => GetServiceProvider().GetService<Aspectus.Aspectus>();
        protected static string ConnectionString { get; } = TestConnectionStrings.Default;
        protected static string ConnectionString2 { get; } = TestConnectionStrings.Default2;
        protected static string ConnectionStringNew { get; } = TestConnectionStrings.Default2;
        protected static string DatabaseName { get; } = "TestDatabase";
        protected static SQLHelper Helper => GetServiceProvider().GetService<SQLHelper>();
        protected static string MasterString { get; } = TestConnectionStrings.Master;
        protected static ObjectPool<StringBuilder> ObjectPool => GetServiceProvider().GetService<ObjectPool<StringBuilder>>();

        /// <summary>
        /// The service provider lock
        /// </summary>
        private static readonly object ServiceProviderLock = new object();

        /// <summary>
        /// The service provider
        /// </summary>
        private static IServiceProvider ServiceProvider;

        public void Dispose()
        {
            // No-op: test databases are reset before each test execution.
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <returns></returns>
        protected static IServiceProvider GetServiceProvider()
        {
            if (ServiceProvider is not null)
                return ServiceProvider;
            lock (ServiceProviderLock)
            {
                if (ServiceProvider is not null)
                    return ServiceProvider;
                ServiceProvider = new ServiceCollection().AddLogging().AddSingleton(Configuration).AddCanisterModules()?.BuildServiceProvider();
            }
            return ServiceProvider;
        }

        private static void SetupConfiguration()
        {
            var dict = new Dictionary<string, string>
                {
                    { "ConnectionStrings:Default", ConnectionString },
                    { "ConnectionStrings:Default2", ConnectionString2 },
                    { "ConnectionStrings:DefaultNew", ConnectionStringNew },
                    { "ConnectionStrings:MasterString", MasterString }
                };
            Configuration = new ConfigurationBuilder()
                             .AddInMemoryCollection(dict)
                             .Build();
        }

        private async Task SetupDatabasesAsync()
        {
            await TestDatabaseManager.ResetKnownDatabasesAsync().ConfigureAwait(false);

            var scriptRoot = AppContext.BaseDirectory;
            var defaultScriptPath = Path.Combine(scriptRoot, "Scripts", "script.sql");
            var default2ScriptPath = Path.Combine(scriptRoot, "Scripts", "testdatabase.sql");

            await ExecuteScriptAsync(ConnectionString, defaultScriptPath).ConfigureAwait(false);
            await ExecuteScriptAsync(ConnectionString2, default2ScriptPath).ConfigureAwait(false);
        }

        private static async Task ExecuteScriptAsync(string connectionString, string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("SQL setup script was not found.", scriptPath);
            }

            var rawScript = File.ReadAllText(scriptPath);
            var commands = SplitSqlCommands(rawScript);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            foreach (var commandText in commands)
            {
                await using var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 120;
                command.CommandText = commandText;
                _ = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private static IEnumerable<string> SplitSqlCommands(string script)
        {
            var normalized = script
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("\r", "\n", StringComparison.Ordinal);

            return normalized
                .Split(["\n\n", "\nGO\n", "\ngo\n"], StringSplitOptions.RemoveEmptyEntries)
                .Select(static x => x.Trim())
                .Where(static x => x.Length > 0);
        }

        private void SetupIoC()
        {
        }
    }
}