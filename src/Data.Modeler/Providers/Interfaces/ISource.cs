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

using System.Collections.Generic;

namespace Data.Modeler.Providers.Interfaces
{
    /// <summary>
    /// Interface for source objects (like databases)
    /// </summary>
    public interface ISource
    {
        /// <summary>
        /// List of functions
        /// </summary>
        List<IFunction> Functions { get; }

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the schemas.
        /// </summary>
        /// <value>The schemas.</value>
        List<string> Schemas { get; }

        /// <summary>
        /// List of stored procedures
        /// </summary>
        List<IFunction> StoredProcedures { get; }

        /// <summary>
        /// List of tables
        /// </summary>
        List<ITable> Tables { get; }

        /// <summary>
        /// List of views
        /// </summary>
        List<IFunction> Views { get; }

        /// <summary>
        /// Returns a table with the given name
        /// </summary>
        /// <param name="name">Table name</param>
        /// <returns>The table specified</returns>
        ITable this[string name] { get; }

        /// <summary>
        /// Adds a function to the database
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="definition">Function definition</param>
        /// <returns></returns>
        IFunction AddFunction(string name, string schemaName, string definition);

        /// <summary>
        /// Adds a stored procedure to the database
        /// </summary>
        /// <param name="procedureName">Procedure name</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="definition">Definition</param>
        /// <returns></returns>
        IFunction AddStoredProcedure(string procedureName, string schemaName, string definition);

        /// <summary>
        /// Adds a table to the database
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <returns></returns>
        ITable AddTable(string tableName, string schemaName);

        /// <summary>
        /// Adds a view to the database
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="definition">The definition.</param>
        /// <returns>The view that is created</returns>
        IFunction AddView(string viewName, string schemaName, string definition);

        /// <summary>
        /// Copies this instance
        /// </summary>
        /// <returns>The copy</returns>
        ISource Copy();
    }
}