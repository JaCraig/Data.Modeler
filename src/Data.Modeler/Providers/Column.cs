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

using BigBook.Comparison;
using Data.Modeler.Providers.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Data.Modeler.Providers
{
    /// <summary>
    /// Column class
    /// </summary>
    /// <typeparam name="T">Data type of the column</typeparam>
    public class Column<T> : IColumn
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Column()
        {
            ForeignKey = new List<IColumn>();
            ForeignKeyColumns = new List<string>();
            ForeignKeyTables = new List<string>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the column</param>
        /// <param name="columnType">The data type</param>
        /// <param name="length">The data length</param>
        /// <param name="nullable">Is it nullable?</param>
        /// <param name="identity">Is it an identity?</param>
        /// <param name="index">Is it the index?</param>
        /// <param name="primaryKey">Is it the primary key?</param>
        /// <param name="unique">Is it unique?</param>
        /// <param name="foreignKeyTable">Foreign key table</param>
        /// <param name="foreignKeyColumn">Foreign key column</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="computedColumnSpecification">The computed column specification.</param>
        /// <param name="onDeleteCascade">Cascade on delete</param>
        /// <param name="onUpdateCascade">Cascade on update</param>
        /// <param name="onDeleteSetNull">Set null on delete</param>
        /// <param name="parentTable">Parent table</param>
        public Column(string name, DbType columnType, int length, bool nullable,
            bool identity, bool index, bool primaryKey, bool unique, string foreignKeyTable,
            string foreignKeyColumn, T defaultValue, string computedColumnSpecification, bool onDeleteCascade, bool onUpdateCascade,
            bool onDeleteSetNull, ITable parentTable)
        {
            Name = name;
            ForeignKey = new List<IColumn>();
            ForeignKeyColumns = new List<string>();
            ForeignKeyTables = new List<string>();
            ParentTable = parentTable;
            DataType = columnType;
            Length = length;
            Nullable = nullable;
            AutoIncrement = identity;
            Index = index;
            PrimaryKey = primaryKey;
            Unique = unique;
            ComputedColumnSpecification = computedColumnSpecification;
            Default = new GenericEqualityComparer<T>().Equals(defaultValue, default(T)) ? "" : defaultValue.ToString();
            OnDeleteCascade = onDeleteCascade;
            OnUpdateCascade = onUpdateCascade;
            OnDeleteSetNull = onDeleteSetNull;
            AddForeignKey(foreignKeyTable, foreignKeyColumn);
        }

        /// <summary>
        /// Auto increment?
        /// </summary>
        public bool AutoIncrement { get; set; }

        /// <summary>
        /// Gets the computed column specificaation.
        /// </summary>
        /// <value>The computed column specificaation.</value>
        public string ComputedColumnSpecification { get; private set; }

        /// <summary>
        /// Data type
        /// </summary>
        public DbType DataType { get; set; }

        /// <summary>
        /// Default value
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        /// Foreign keys
        /// </summary>
        public ICollection<IColumn> ForeignKey { get; private set; }

        /// <summary>
        /// Index?
        /// </summary>
        public bool Index { get; set; }

        /// <summary>
        /// Data length
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Nullable?
        /// </summary>
        public bool Nullable { get; set; }

        /// <summary>
        /// On Delete Cascade
        /// </summary>
        public bool OnDeleteCascade { get; set; }

        /// <summary>
        /// On Delete Set Null
        /// </summary>
        public bool OnDeleteSetNull { get; set; }

        /// <summary>
        /// On Update Cascade
        /// </summary>
        public bool OnUpdateCascade { get; set; }

        /// <summary>
        /// Parent table
        /// </summary>
        public ITable ParentTable { get; set; }

        /// <summary>
        /// Primary key?
        /// </summary>
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// Unique?
        /// </summary>
        public bool Unique { get; set; }

        private List<string> ForeignKeyColumns { get; set; }

        private List<string> ForeignKeyTables { get; set; }

        /// <summary>
        /// Add foreign key
        /// </summary>
        /// <param name="foreignKeyTable">Table of the foreign key</param>
        /// <param name="foreignKeyColumn">Column of the foreign key</param>
        public void AddForeignKey(string foreignKeyTable, string foreignKeyColumn)
        {
            if (string.IsNullOrEmpty(foreignKeyTable) || string.IsNullOrEmpty(foreignKeyColumn))
                return;
            ForeignKeyColumns.Add(foreignKeyColumn);
            ForeignKeyTables.Add(foreignKeyTable);
        }

        /// <summary>
        /// Sets up the foreign key list
        /// </summary>
        public void SetupForeignKeys()
        {
            ISource TempDatabase = ParentTable.Source;
            for (int x = 0; x < ForeignKeyColumns.Count; ++x)
            {
                if (TempDatabase != null)
                {
                    foreach (Table TempTable in TempDatabase.Tables)
                    {
                        if (TempTable.Name == ForeignKeyTables[x])
                        {
                            foreach (IColumn TempColumn in TempTable.Columns)
                            {
                                if (TempColumn.Name == ForeignKeyColumns[x])
                                {
                                    ForeignKey.Add(TempColumn);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}