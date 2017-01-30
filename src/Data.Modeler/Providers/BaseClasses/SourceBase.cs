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
using System.Linq;
using System.Threading.Tasks;

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
        protected SourceBase(string name)
        {
            Name = name;
            Tables = new List<ITable>();
            StoredProcedures = new List<ITable>();
            Views = new List<ITable>();
            Functions = new List<IFunction>();
        }

        /// <summary>
        /// Functions with the source
        /// </summary>
        public ICollection<IFunction> Functions { get; private set; }

        /// <summary>
        /// Name of the source
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Stored procedures within the source
        /// </summary>
        public ICollection<ITable> StoredProcedures { get; private set; }

        /// <summary>
        /// Tables within the source
        /// </summary>
        public ICollection<ITable> Tables { get; private set; }

        /// <summary>
        /// Views within the source
        /// </summary>
        public ICollection<ITable> Views { get; private set; }

        /// <summary>
        /// Gets a specific table based on the name
        /// </summary>
        /// <param name="name">Name of the table</param>
        /// <returns>The table specified</returns>
        public ITable this[string name] { get { return Tables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase)); } }

        /// <summary>
        /// Adds a function to the source
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <param name="definition">Definition of the function</param>
        /// <returns>Function that was created/added</returns>
        public abstract IFunction AddFunction(string name, string definition);

        /// <summary>
        /// Adds a stored procedure to the source
        /// </summary>
        /// <param name="procedureName">Procedure name</param>
        /// <param name="definition">Definition of the stored procedure</param>
        /// <returns>Stored procedure that was created/added</returns>
        public abstract ITable AddStoredProcedure(string procedureName, string definition);

        /// <summary>
        /// Adds a table to the source
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Table that was created/added</returns>
        public abstract ITable AddTable(string tableName);

        /// <summary>
        /// Adds a view to the source
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <returns>View that was created/added</returns>
        public abstract ITable AddView(string viewName);
    }
}