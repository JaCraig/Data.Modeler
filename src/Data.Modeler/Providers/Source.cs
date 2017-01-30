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
using Data.Modeler.Providers.BaseClasses;
using Data.Modeler.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Modeler.Providers
{
    /// <summary>
    /// Database class
    /// </summary>
    public class Source : SourceBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the database</param>
        public Source(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Adds a function to the database
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="definition">Function definition</param>
        public override IFunction AddFunction(string name, string definition)
        {
            return Functions.AddAndReturn(new Function(name, definition, this));
        }

        /// <summary>
        /// Adds a stored procedure to the database
        /// </summary>
        /// <param name="procedureName">Procedure name</param>
        /// <param name="definition">Definition</param>
        public override ITable AddStoredProcedure(string procedureName, string definition)
        {
            return StoredProcedures.AddAndReturn(new StoredProcedure(procedureName, definition, this));
        }

        /// <summary>
        /// Adds a table to the database
        /// </summary>
        /// <param name="tableName">Table name</param>
        public override ITable AddTable(string tableName)
        {
            return Tables.AddAndReturn(new Table(tableName, this));
        }

        /// <summary>
        /// Adds a view to the database
        /// </summary>
        /// <param name="viewName">View name</param>
        public override ITable AddView(string viewName)
        {
            return Views.AddAndReturn(new View(viewName, this));
        }
    }
}