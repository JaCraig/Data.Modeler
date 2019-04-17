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

using Data.Modeler.Providers.Interfaces;
using System;
using System.Collections.Generic;

namespace Data.Modeler.Providers.BaseClasses
{
    /// <summary>
    /// Source base class
    /// </summary>
    public abstract class SourceBase : ISource
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name.</param>
        protected SourceBase(string name)
        {
            Name = name;
            Tables = new List<ITable>();
            StoredProcedures = new List<IFunction>();
            Views = new List<IFunction>();
            Functions = new List<IFunction>();
            Schemas = new List<string>();
        }

        /// <summary>
        /// Functions with the source
        /// </summary>
        public List<IFunction> Functions { get; }

        /// <summary>
        /// Name of the source
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the schemas.
        /// </summary>
        /// <value>The schemas.</value>
        public List<string> Schemas { get; }

        /// <summary>
        /// Stored procedures within the source
        /// </summary>
        public List<IFunction> StoredProcedures { get; }

        /// <summary>
        /// Tables within the source
        /// </summary>
        public List<ITable> Tables { get; }

        /// <summary>
        /// Views within the source
        /// </summary>
        public List<IFunction> Views { get; }

        /// <summary>
        /// Gets a specific table based on the name
        /// </summary>
        /// <param name="name">Name of the table</param>
        /// <returns>The table specified</returns>
        public ITable this[string name] => Tables.Find(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        /// Adds a function to the source
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="definition">Definition of the function</param>
        /// <returns>Function that was created/added</returns>
        public abstract IFunction AddFunction(string name, string schemaName, string definition);

        /// <summary>
        /// Adds a stored procedure to the source
        /// </summary>
        /// <param name="procedureName">Procedure name</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="definition">Definition of the stored procedure</param>
        /// <returns>Stored procedure that was created/added</returns>
        public abstract IFunction AddStoredProcedure(string procedureName, string schemaName, string definition);

        /// <summary>
        /// Adds a table to the source
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <returns>Table that was created/added</returns>
        public abstract ITable AddTable(string tableName, string schemaName);

        /// <summary>
        /// Adds a view to the source
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="definition">The definition.</param>
        /// <returns>View that was created/added</returns>
        public abstract IFunction AddView(string viewName, string schemaName, string definition);

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public ISource Copy()
        {
            var Result = new Source(Name);
            for (int i = 0, FunctionsCount = Functions.Count; i < FunctionsCount; i++)
            {
                var Function = Functions[i];
                Result.Functions.Add(Function.Copy(Result));
            }

            for (int i = 0, StoredProceduresCount = StoredProcedures.Count; i < StoredProceduresCount; i++)
            {
                var StoredProcedure = StoredProcedures[i];
                Result.StoredProcedures.Add(StoredProcedure.Copy(Result));
            }

            for (int i = 0, TablesCount = Tables.Count; i < TablesCount; i++)
            {
                var Table = Tables[i];
                Result.Tables.Add(Table.Copy(Result));
            }

            for (int i = 0, ViewsCount = Views.Count; i < ViewsCount; i++)
            {
                var View = Views[i];
                Result.Views.Add(View.Copy(Result));
            }

            return Result;
        }
    }
}