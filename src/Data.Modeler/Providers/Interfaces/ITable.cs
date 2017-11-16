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

using Data.Modeler.Providers.Enums;
using System.Collections.Generic;
using System.Data;

namespace Data.Modeler.Providers.Interfaces
{
    /// <summary>
    /// Interface for table like structures
    /// </summary>
    public interface ITable
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="ITable"/> is audit.
        /// </summary>
        /// <value><c>true</c> if audit; otherwise, <c>false</c>.</value>
        bool Audit { get; }

        /// <summary>
        /// Columns
        /// </summary>
        /// <value>The columns.</value>
        IList<IColumn> Columns { get; }

        /// <summary>
        /// Gets the constraints.
        /// </summary>
        /// <value>The constraints.</value>
        ICollection<ICheckConstraint> Constraints { get; }

        /// <summary>
        /// Name
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        /// <value>The schema.</value>
        string Schema { get; }

        /// <summary>
        /// Parent of the table structure
        /// </summary>
        /// <value>The source.</value>
        ISource Source { get; }

        /// <summary>
        /// Triggers associated with the table (if source supports them)
        /// </summary>
        /// <value>The triggers.</value>
        ICollection<ITrigger> Triggers { get; }

        /// <summary>
        /// Returns the specified column
        /// </summary>
        /// <param name="Name">Name of the column</param>
        /// <returns>Column specified</returns>
        IColumn this[string Name] { get; }

        /// <summary>
        /// Adds a check constraint to the table.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="definition">The definition.</param>
        /// <returns>The check constraint added to the table</returns>
        ICheckConstraint AddCheckConstraint(string name, string definition);

        /// <summary>
        /// Adds a column
        /// </summary>
        /// <typeparam name="T">Column type</typeparam>
        /// <param name="columnName">Column Name</param>
        /// <param name="columnType">Data type</param>
        /// <param name="length">Data length</param>
        /// <param name="nullable">Nullable?</param>
        /// <param name="identity">Identity?</param>
        /// <param name="index">Index?</param>
        /// <param name="primaryKey">Primary key?</param>
        /// <param name="unique">Unique?</param>
        /// <param name="foreignKeyTable">Foreign key table</param>
        /// <param name="foreignKeyColumn">Foreign key column</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="computedColumnSpecification">The computed column specification.</param>
        /// <param name="onDeleteCascade">On Delete Cascade</param>
        /// <param name="onUpdateCascade">On Update Cascade</param>
        /// <param name="onDeleteSetNull">On Delete Set Null</param>
        /// <returns></returns>
        IColumn AddColumn<T>(string columnName, DbType columnType, int length = 0, bool nullable = true,
            bool identity = false, bool index = false, bool primaryKey = false, bool unique = false,
            string foreignKeyTable = "", string foreignKeyColumn = "", T defaultValue = default,
            string computedColumnSpecification = "", bool onDeleteCascade = false, bool onUpdateCascade = false,
            bool onDeleteSetNull = false);

        /// <summary>
        /// Adds a foreign key
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="foreignKeyTable">Foreign key table</param>
        /// <param name="foreignKeyColumn">Foreign key column</param>
        void AddForeignKey(string columnName, string foreignKeyTable, string foreignKeyColumn);

        /// <summary>
        /// Adds a trigger to the table
        /// </summary>
        /// <param name="name">Name of the trigger</param>
        /// <param name="definition">Trigger definition</param>
        /// <param name="type">Trigger type</param>
        /// <returns>Trigger added to the table</returns>
        ITrigger AddTrigger(string name, string definition, TriggerType type);

        /// <summary>
        /// Determines if a column exists in the table
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>True if it exists, false otherwise</returns>
        bool ContainsColumn(string columnName);

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The copy of this instance.</returns>
        ITable Copy(ISource source);

        /// <summary>
        /// Sets up foreign keys
        /// </summary>
        void SetupForeignKeys();
    }
}