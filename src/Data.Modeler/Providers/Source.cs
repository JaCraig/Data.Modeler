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
using System.Linq;

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
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="definition">Function definition</param>
        /// <returns>Function that was created/added</returns>
        public override IFunction AddFunction(string name, string schemaName, string definition) => Functions.AddAndReturn(new Function(name, schemaName, definition, this));

        /// <summary>
        /// Adds a stored procedure to the database
        /// </summary>
        /// <param name="procedureName">Procedure name</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="definition">Definition</param>
        /// <returns>The stored procedure</returns>
        public override IFunction AddStoredProcedure(string procedureName, string schemaName, string definition) => StoredProcedures.AddAndReturn(new StoredProcedure(procedureName, schemaName, definition, this));

        /// <summary>
        /// Adds a table to the database
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <returns>Table that was created/added</returns>
        public override ITable AddTable(string tableName, string schemaName) => Tables.AddAndReturn(new Table(tableName, schemaName, this));

        /// <summary>
        /// Adds a view to the database
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="definition">The definition.</param>
        /// <returns>The resulting view object</returns>
        public override IFunction AddView(string viewName, string schemaName, string definition) => Views.AddAndReturn(new View(viewName, schemaName, definition, this));

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Source Item))
                return false;
            return Functions.All(x => Item.Functions.Contains(x))
                && Name == Item.Name
                && StoredProcedures.All(x => Item.StoredProcedures.Contains(x))
                && Tables.All(x => Item.Tables.Contains(x))
                && Views.All(x => Item.Views.Contains(x));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures
        /// like a hash table.
        /// </returns>
        public override int GetHashCode() => Name.GetHashCode();
    }
}