/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using BigBook;
using Data.Modeler.Providers.Interfaces;
using SQLHelperDB;
using SQLHelperDB.HelperClasses;
using SQLHelperDB.HelperClasses.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Data.Modeler.Providers.SQLServer
{
    /// <summary>
    /// SQL Server schema generator
    /// </summary>
    /// <seealso cref="ISchemaGenerator"/>
    public class SQLServerSchemaGenerator : ISchemaGenerator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryBuilders">The query builders.</param>
        /// <param name="commandBuilders">The command builders.</param>
        public SQLServerSchemaGenerator(IEnumerable<ISourceBuilder> queryBuilders, IEnumerable<ICommandBuilder> commandBuilders)
        {
            CommandBuilders = commandBuilders.Where(x => x.Provider == Provider).OrderBy(x => x.Order).ToArray();
            QueryBuilders = queryBuilders.Where(x => x.Provider == Provider).OrderBy(x => x.Order).ToArray();
        }

        /// <summary>
        /// Provider name associated with the schema generator
        /// </summary>
        public DbProviderFactory Provider { get; } = SqlClientFactory.Instance;

        /// <summary>
        /// Gets the command builders.
        /// </summary>
        /// <value>The command builders.</value>
        private ICommandBuilder[] CommandBuilders { get; }

        /// <summary>
        /// Gets or sets the query builders.
        /// </summary>
        /// <value>The query builders.</value>
        private ISourceBuilder[] QueryBuilders { get; }

        /// <summary>
        /// Checks if a constraint exists
        /// </summary>
        /// <param name="constraint">The constraint to check.</param>
        /// <param name="source">Source to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool ConstraintExists(string constraint, IConnection source) => Exists("SELECT * FROM sys.check_constraints WHERE name=@0", constraint, source);

        /// <summary>
        /// Generates a list of commands used to modify the source. If it does not exist prior, the
        /// commands will create the source from scratch. Otherwise the commands will only add new
        /// fields, tables, etc. It does not delete old fields.
        /// </summary>
        /// <param name="desiredStructure">Desired source structure</param>
        /// <param name="source">Source to use</param>
        /// <returns>List of commands generated</returns>
        public string[] GenerateSchema(ISource desiredStructure, ISource? source)
        {
            var Commands = new List<string>();
            desiredStructure ??= new Source("");
            for (int i = 0, CommandBuildersLength = CommandBuilders.Length; i < CommandBuildersLength; i++)
            {
                var CommandBuilder = CommandBuilders[i];
                Commands.Add(CommandBuilder.GetCommands(desiredStructure, source));
            }

            return Commands.ToArray();
        }

        /// <summary>
        /// Gets the structure of a source
        /// </summary>
        /// <param name="connectionInfo">Source to use</param>
        /// <returns>The source structure</returns>
        public ISource? GetSourceStructure(IConnection connectionInfo)
        {
            var DatabaseName = connectionInfo.DatabaseName ?? "";
            var DatabaseSource = new Connection(connectionInfo.Configuration, connectionInfo.Factory, connectionInfo.ConnectionString.RemoveInitialCatalog(), "Name");
            if (!SourceExists(DatabaseName, DatabaseSource))
                return null;
            var Temp = new Source(DatabaseName);
            var Batch = new SQLHelper(connectionInfo.Configuration, connectionInfo.Factory, connectionInfo.ConnectionString)
                                     .CreateBatch();
            for (int i = 0, QueryBuildersLength = QueryBuilders.Length; i < QueryBuildersLength; i++)
            {
                var Builder = QueryBuilders[i];
                Batch.AddQuery(CommandType.Text, Builder.GetCommand());
            }

            var Results = Batch.Execute();
            for (int x = 0, QueryBuildersLength = QueryBuilders.Length; x < QueryBuildersLength; ++x)
            {
                var Builder = QueryBuilders[x];
                Builder.FillSource(Results[x], Temp);
            }
            return Temp;
        }

        /// <summary>
        /// Sets up the specified database schema
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="connection">The connection.</param>
        public void Setup(ISource source, IConnection connection)
        {
            var CurrentSource = GetSourceStructure(connection);
            var Commands = GenerateSchema(source, CurrentSource).ToArray();

            var DatabaseSource = new Connection(connection.Configuration, connection.Factory, connection.ConnectionString.RemoveInitialCatalog(), "Name");
            var Batch = new SQLHelper(connection.Configuration, connection.Factory, connection.ConnectionString);
            for (var x = 0; x < Commands.Length; ++x)
            {
                if (Commands[x].IndexOf("CREATE DATABASE", System.StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    new SQLHelper(connection.Configuration, connection.Factory, DatabaseSource.ConnectionString)
                                 .CreateBatch()
                                 .AddQuery(CommandType.Text, Commands[x])
                                 .Execute();
                }
                else if (Commands[x].IndexOf("CREATE TRIGGER", System.StringComparison.InvariantCultureIgnoreCase) >= 0 || Commands[x].IndexOf("CREATE FUNCTION", System.StringComparison.InvariantCultureIgnoreCase) >= 0 || Commands[x].IndexOf("CREATE SCHEMA", System.StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    if (Batch.Count > 0)
                    {
                        Batch.Execute();
                        Batch.CreateBatch();
                    }
                    Batch.AddQuery(CommandType.Text, Commands[x]);
                    if (x < Commands.Length - 1)
                    {
                        Batch.Execute();
                        Batch.CreateBatch();
                    }
                }
                else
                {
                    Batch.AddQuery(CommandType.Text, Commands[x]);
                }
            }
            Batch.Execute();
        }

        /// <summary>
        /// Checks if a source exists
        /// </summary>
        /// <param name="source">Source to check</param>
        /// <param name="connectionInfo">Source info to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool SourceExists(string source, IConnection connectionInfo) => Exists("SELECT * FROM Master.sys.Databases WHERE name=@0", source, connectionInfo);

        /// <summary>
        /// Checks if a stored procedure exists
        /// </summary>
        /// <param name="storedProcedure">Stored procedure to check</param>
        /// <param name="connectionInfo">Source to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool StoredProcedureExists(string storedProcedure, IConnection connectionInfo) => Exists("SELECT * FROM sys.Procedures WHERE name=@0", storedProcedure, connectionInfo);

        /// <summary>
        /// Checks if a table exists
        /// </summary>
        /// <param name="table">Table to check</param>
        /// <param name="connectionInfo">Source to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool TableExists(string table, IConnection connectionInfo) => Exists("SELECT * FROM sys.Tables WHERE name=@0", table, connectionInfo);

        /// <summary>
        /// Checks if a trigger exists
        /// </summary>
        /// <param name="trigger">Trigger to check</param>
        /// <param name="connectionInfo">Source to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool TriggerExists(string trigger, IConnection connectionInfo) => Exists("SELECT * FROM sys.triggers WHERE name=@0", trigger, connectionInfo);

        /// <summary>
        /// Checks if a view exists
        /// </summary>
        /// <param name="view">View to check</param>
        /// <param name="connectionInfo">Source to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool ViewExists(string view, IConnection connectionInfo) => Exists("SELECT * FROM sys.views WHERE name=@0", view, connectionInfo);

        private bool Exists(string command, string value, IConnection source)
        {
            if (source == null || value == null || command == null)
                return false;
            return new SQLHelper(source.Configuration, source.Factory, source.ConnectionString)
                           .AddQuery(CommandType.Text, command, value)
                           .Execute()[0]
                           .Count > 0;
        }
    }
}