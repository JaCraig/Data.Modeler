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
using Data.Modeler.Providers.Enums;
using Data.Modeler.Providers.Interfaces;
using System.Data;

namespace Data.Modeler.Providers
{
    /// <summary>
    /// Table class
    /// </summary>
    public class Table : TableBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="source">Source</param>
        public Table(string name, ISource source)
            : base(name, source)
        {
        }

        /// <summary>
        /// Adds a check constraint to the table.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="definition">The definition.</param>
        /// <returns>The check constraint added to the table</returns>
        public override ICheckConstraint AddCheckConstraint(string name, string definition)
        {
            return Constraints.AddAndReturn(new CheckConstraint(name, definition, this));
        }

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
        public override IColumn AddColumn<T>(string columnName, DbType columnType, int length = 0,
            bool nullable = true, bool identity = false, bool index = false,
            bool primaryKey = false, bool unique = false, string foreignKeyTable = "",
            string foreignKeyColumn = "", T defaultValue = default(T), string computedColumnSpecification = "",
            bool onDeleteCascade = false, bool onUpdateCascade = false, bool onDeleteSetNull = false)
        {
            return Columns.AddAndReturn(new Column<T>(columnName,
                columnType,
                length,
                nullable,
                identity,
                index,
                primaryKey,
                unique,
                foreignKeyTable,
                foreignKeyColumn,
                defaultValue,
                computedColumnSpecification,
                onDeleteCascade,
                onUpdateCascade,
                onDeleteSetNull,
                this));
        }

        /// <summary>
        /// Adds a foreign key
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="foreignKeyTable">Foreign key table</param>
        /// <param name="foreignKeyColumn">Foreign key column</param>
        public override void AddForeignKey(string columnName, string foreignKeyTable, string foreignKeyColumn)
        {
            if (!ContainsColumn(columnName))
                return;
            this[columnName].AddForeignKey(foreignKeyTable, foreignKeyColumn);
        }

        /// <summary>
        /// Adds a trigger to the table
        /// </summary>
        /// <param name="name">Name of the trigger</param>
        /// <param name="definition">Definition of the trigger</param>
        /// <param name="type">Trigger type</param>
        /// <returns>The trigger specified</returns>
        public override ITrigger AddTrigger(string name, string definition, TriggerType type)
        {
            return Triggers.AddAndReturn(new Trigger(name, definition, type, this));
        }
    }
}