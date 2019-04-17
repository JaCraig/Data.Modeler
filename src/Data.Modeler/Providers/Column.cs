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
using BigBook.Comparison;
using Data.Modeler.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
            SetDefaultValue(defaultValue);
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
        public string ComputedColumnSpecification { get; }

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
        public List<IColumn> ForeignKey { get; }

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

        /// <summary>
        /// Gets or sets the foreign key columns.
        /// </summary>
        /// <value>The foreign key columns.</value>
        private List<string> ForeignKeyColumns { get; set; }

        /// <summary>
        /// Gets or sets the foreign key tables.
        /// </summary>
        /// <value>The foreign key tables.</value>
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
        /// Copies this instance
        /// </summary>
        /// <param name="parentTable">The new parent table.</param>
        /// <returns>The copy</returns>
        public IColumn Copy(ITable parentTable)
        {
            return new Column<T>(Name, DataType, Length,
                Nullable, AutoIncrement, Index,
                PrimaryKey, Unique, "",
                "", Default.To<string, T>(), ComputedColumnSpecification,
                OnDeleteCascade, OnUpdateCascade, OnDeleteSetNull,
                parentTable)
            {
                ForeignKeyColumns = ForeignKeyColumns.ToList(),
                ForeignKeyTables = ForeignKeyTables.ToList()
            };
        }

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
            if (!(obj is Column<T> Item))
                return false;
            return AutoIncrement == Item.AutoIncrement
                && ComputedColumnSpecification == Item.ComputedColumnSpecification
                && DataType == Item.DataType
                && Default == Item.Default
                && ForeignKeyColumns.All(x => Item.ForeignKeyColumns.Contains(x))
                && ForeignKeyTables.All(x => Item.ForeignKeyTables.Contains(x))
                && Index == Item.Index
                && Length == Item.Length
                && Name == Item.Name
                && Nullable == Item.Nullable
                && OnDeleteCascade == Item.OnDeleteCascade
                && OnDeleteSetNull == Item.OnDeleteSetNull
                && OnUpdateCascade == Item.OnUpdateCascade
                && PrimaryKey == Item.PrimaryKey
                && Unique == Item.Unique
                && ParentTable.Name == Item.ParentTable.Name;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures
        /// like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
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
                    for (int i = 0, TempDatabaseTablesCount = TempDatabase.Tables.Count; i < TempDatabaseTablesCount; i++)
                    {
                        ITable TempTable = TempDatabase.Tables[i];
                        if (TempTable.Name == ForeignKeyTables[x])
                        {
                            for (int j = 0, TempTableColumnsCount = TempTable.Columns.Count; j < TempTableColumnsCount; j++)
                            {
                                IColumn TempColumn = TempTable.Columns[j];
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

        private void SetDefaultValue(T defaultValue)
        {
            if (new GenericEqualityComparer<T>().Equals(defaultValue, default))
            {
                Default = "";
                return;
            }
            Default = defaultValue.ToString().Replace("(", "").Replace(")", "").Replace("'", "''");
            if (string.IsNullOrEmpty(Default))
                return;
            if (defaultValue is bool boolDefault)
                Default = boolDefault ? "1" : "0";
            else if (defaultValue is DateTime)
                Default = $"\'{Default}\'";
            else if (defaultValue is TimeSpan)
                Default = $"\'{Default}\'";
            else if (defaultValue is string)
                Default = $"\'{Default}\'";
            else if (defaultValue is char)
                Default = $"\'{Default}\'";
        }
    }
}