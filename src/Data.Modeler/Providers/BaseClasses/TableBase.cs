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
using System.Data;

namespace Data.Modeler.Providers.BaseClasses
{
    /// <summary>
    /// Table base class
    /// </summary>
    public abstract class TableBase : ITable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the table</param>
        /// <param name="schema">The schema.</param>
        /// <param name="source">Source that the table is from</param>
        protected TableBase(string name, string schema, ISource source)
        {
            Schema = schema;
            Name = name;
            Source = source;
            Columns = new List<IColumn>();
            Triggers = new List<ITrigger>();
            Constraints = new List<ICheckConstraint>();
        }

        /// <summary>
        /// Gets a value indicating whether this is audit.
        /// </summary>
        /// <value><c>true</c> if audit; otherwise, <c>false</c>.</value>
        public bool Audit { get; set; }

        /// <summary>
        /// Columns
        /// </summary>
        public List<IColumn> Columns { get; }

        /// <summary>
        /// Gets the constraints.
        /// </summary>
        /// <value>The constraints.</value>
        public List<ICheckConstraint> Constraints { get; }

        /// <summary>
        /// Name of the table
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        /// <value>The schema.</value>
        public string Schema { get; }

        /// <summary>
        /// Source/Parent
        /// </summary>
        public ISource Source { get; }

        /// <summary>
        /// List of triggers associated with the table
        /// </summary>
        public List<ITrigger> Triggers { get; }

        /// <summary>
        /// The column specified
        /// </summary>
        /// <param name="name">Name of the column</param>
        /// <returns>The column specified</returns>
        public IColumn this[string name] => Columns.Find(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        /// Adds a check constraint to the table.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="definition">The definition.</param>
        /// <returns>The check constraint added to the table</returns>
        public abstract ICheckConstraint AddCheckConstraint(string name, string definition);

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
        public abstract IColumn AddColumn<T>(string columnName, DbType columnType, int length = 0, bool nullable = true,
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
        public abstract void AddForeignKey(string columnName, string foreignKeyTable, string foreignKeyColumn);

        /// <summary>
        /// Adds a trigger to the table
        /// </summary>
        /// <param name="name">Name of the trigger</param>
        /// <param name="definition">Definition of the trigger</param>
        /// <param name="type">Trigger type</param>
        /// <returns>The trigger specified</returns>
        public abstract ITrigger AddTrigger(string name, string definition, Enums.TriggerType type);

        /// <summary>
        /// Determines if a column exists in the table
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool ContainsColumn(string columnName) => this[columnName] != null;

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The copy of this instance.</returns>
        public abstract ITable Copy(ISource source);

        /// <summary>
        /// Sets up foreign keys
        /// </summary>
        public void SetupForeignKeys()
        {
            for (var x = 0; x < Columns.Count; ++x)
            {
                Columns[x].SetupForeignKeys();
            }
            for (var x = 0; x < Columns.Count; ++x)
            {
                for (var y = x + 1; y < Columns.Count; ++y)
                {
                    for (var z = 0; z < Columns[x].ForeignKey.Count; ++z)
                    {
                        if (Columns[y].ForeignKey.Contains(Columns[x].ForeignKey[z]))
                        {
                            Columns[y].OnDeleteSetNull = false;
                            Columns[y].OnDeleteCascade = false;
                            Columns[y].OnUpdateCascade = false;
                            Columns[x].OnDeleteSetNull = false;
                            Columns[x].OnDeleteCascade = false;
                            Columns[x].OnUpdateCascade = false;
                        }
                    }
                }
            }
        }
    }
}