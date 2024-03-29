﻿/*
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

using SQLHelperDB.HelperClasses.Interfaces;
using System.Data.Common;
using System.Threading.Tasks;

namespace Data.Modeler.Providers.Interfaces
{
    /// <summary>
    /// Schema generator interface
    /// </summary>
    public interface ISchemaGenerator
    {
        /// <summary>
        /// Provider associated with the schema generator
        /// </summary>
        DbProviderFactory[] Providers { get; }

        /// <summary>
        /// Generates a list of commands used to modify the source. If it does not exist prior, the
        /// commands will create the source from scratch. Otherwise the commands will only add new
        /// fields, tables, etc. It does not delete old fields.
        /// </summary>
        /// <param name="desiredStructure">Desired source structure</param>
        /// <param name="source">Source to use</param>
        /// <returns>List of commands generated</returns>
        string[] GenerateSchema(ISource desiredStructure, ISource source);

        /// <summary>
        /// Gets the structure of a source
        /// </summary>
        /// <param name="connectionInfo">The connection information.</param>
        /// <returns>The source structure</returns>
        Task<ISource?> GetSourceStructureAsync(IConnection connectionInfo);

        /// <summary>
        /// Sets up the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="connection">The connection.</param>
        Task SetupAsync(ISource source, IConnection connection);

        /// <summary>
        /// Sets up the specified source.
        /// </summary>
        /// <param name="schemaChanges">The schema changes.</param>
        /// <param name="connection">The connection.</param>
        Task SetupAsync(string[] schemaChanges, IConnection connection);

        /// <summary>
        /// Checks if a source exists
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="connectionInfo">The connection.</param>
        /// <returns>True if it exists, false otherwise</returns>
        Task<bool> SourceExistsAsync(string source, IConnection connectionInfo);

        /// <summary>
        /// Checks if a stored procedure exists
        /// </summary>
        /// <param name="storedProcedure">Stored procedure to check</param>
        /// <param name="connectionInfo">The connection information.</param>
        /// <returns>True if it exists, false otherwise</returns>
        Task<bool> StoredProcedureExistsAsync(string storedProcedure, IConnection connectionInfo);

        /// <summary>
        /// Checks if a table exists
        /// </summary>
        /// <param name="table">Table to check</param>
        /// <param name="connectionInfo">The connection information.</param>
        /// <returns>True if it exists, false otherwise</returns>
        Task<bool> TableExistsAsync(string table, IConnection connectionInfo);

        /// <summary>
        /// Checks if a trigger exists
        /// </summary>
        /// <param name="trigger">Trigger to check</param>
        /// <param name="connectionInfo">The connection information.</param>
        /// <returns>True if it exists, false otherwise</returns>
        Task<bool> TriggerExistsAsync(string trigger, IConnection connectionInfo);

        /// <summary>
        /// Checks if a view exists
        /// </summary>
        /// <param name="view">View to check</param>
        /// <param name="connectionInfo">The connection information.</param>
        /// <returns>True if it exists, false otherwise</returns>
        Task<bool> ViewExistsAsync(string view, IConnection connectionInfo);
    }
}