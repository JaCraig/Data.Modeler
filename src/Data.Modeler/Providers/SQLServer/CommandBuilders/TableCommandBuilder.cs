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
using Data.Modeler.Providers.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Table command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class TableCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableCommandBuilder"/> class.
        /// </summary>
        /// <param name="objectPool">The object pool.</param>
        public TableCommandBuilder(ObjectPool<StringBuilder> objectPool)
        {
            ObjectPool = objectPool;
        }

        /// <summary>
        /// Gets the object pool.
        /// </summary>
        /// <value>The object pool.</value>
        public ObjectPool<StringBuilder> ObjectPool { get; }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; } = 10;

        /// <summary>
        /// Provider name associated with the schema generator
        /// </summary>
        public DbProviderFactory Provider { get; } = SqlClientFactory.Instance;

        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <param name="desiredStructure">The desired structure.</param>
        /// <param name="currentStructure">The current structure.</param>
        /// <returns>
        /// The list of commands needed to change the structure from the current to the desired structure
        /// </returns>
        public string[] GetCommands(ISource desiredStructure, ISource? currentStructure)
        {
            if (desiredStructure is null)
                return Array.Empty<string>();
            currentStructure ??= new Source(desiredStructure.Name);
            var Commands = new List<string>();
            var Builder = ObjectPool.Get();
            for (int i = 0, desiredStructureTablesCount = desiredStructure.Tables.Count; i < desiredStructureTablesCount; i++)
            {
                var TempTable = desiredStructure.Tables[i];
                var CurrentTable = currentStructure[TempTable.Name];
                Commands.Add((CurrentTable is null) ? GetTableCommand(TempTable, Builder) : GetAlterTableCommand(TempTable, CurrentTable, Builder));
            }
            ObjectPool.Return(Builder);

            return Commands.ToArray();
        }

        /// <summary>
        /// Gets the alter table command.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="currentTable">The current table.</param>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetAlterTableCommand(ITable table, ITable currentTable, StringBuilder builder)
        {
            if (table is null || table.Columns is null)
                return Array.Empty<string>();
            var ReturnValue = new List<string>();
            for (int i = 0, tableColumnsCount = table.Columns.Count; i < tableColumnsCount; i++)
            {
                var Column = table.Columns[i];
                var CurrentColumn = currentTable[Column.Name];
                if (CurrentColumn is null)
                {
                    if (string.IsNullOrEmpty(Column.ComputedColumnSpecification))
                    {
                        builder.Append("ALTER TABLE [")
                            .Append(table.Schema)
                            .Append("].[")
                            .Append(table.Name)
                            .Append("] ADD [")
                            .Append(Column.Name)
                            .Append("] ")
                            .Append(Column.DataType.To(SqlDbType.Int).ToString());
                        if (Column.DataType == SqlDbType.VarChar.To(DbType.Int32)
                            || Column.DataType == SqlDbType.NVarChar.To(DbType.Int32)
                            || Column.DataType == SqlDbType.Binary.To(DbType.Int32))
                        {
                            GetColumnLength(builder, Column);
                        }
                        else if (Column.DataType == SqlDbType.Decimal.To(DbType.Int32))
                        {
                            GetColumnPrecision(builder, Column);
                        }
                    }
                    else
                    {
                        builder.Append("ALTER TABLE [")
                            .Append(table.Schema)
                            .Append("].[")
                            .Append(table.Name)
                            .Append("] ADD [")
                            .Append(Column.Name)
                            .Append("] AS (")
                            .Append(Column.ComputedColumnSpecification)
                            .Append(")");
                    }
                    ReturnValue.Add(builder.ToString());
                    builder.Clear();
                }
                else if (CurrentColumn.DataType.To(SqlDbType.Int) != Column.DataType.To(SqlDbType.Int)
                    || (CurrentColumn.DataType.To(SqlDbType.Int) == Column.DataType.To(SqlDbType.Int)
                        && CurrentColumn.DataType == SqlDbType.NVarChar.To(DbType.Int32)
                        && CurrentColumn.Length != Column.Length
                        && CurrentColumn.Length.Between(1, 4000)
                        && Column.Length.Between(1, 4000)))
                {
                    builder.Append("ALTER TABLE [")
                        .Append(table.Schema)
                        .Append("].[")
                        .Append(table.Name)
                        .Append("] ALTER COLUMN [")
                        .Append(Column.Name)
                        .Append("] ")
                        .Append(Column.DataType.To(SqlDbType.Int).ToString());
                    if (Column.DataType == SqlDbType.VarChar.To(DbType.Int32)
                        || Column.DataType == SqlDbType.NVarChar.To(DbType.Int32)
                        || Column.DataType == SqlDbType.Binary.To(DbType.Int32))
                    {
                        GetColumnLength(builder, Column);
                    }
                    else if (Column.DataType == SqlDbType.Decimal.To(DbType.Int32))
                    {
                        GetColumnPrecision(builder, Column);
                    }
                    ReturnValue.Add(builder.ToString());
                    builder.Clear();
                }
            }

            return ReturnValue;
        }

        /// <summary>
        /// Gets the length of the column.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="Column">The column.</param>
        private static void GetColumnLength(StringBuilder builder, IColumn Column)
        {
            builder.Append("(")
                    .Append((Column.Length <= 0 || Column.Length >= 4000) ? "MAX" : Column.Length.ToString(CultureInfo.InvariantCulture))
                    .Append(")");
        }

        /// <summary>
        /// Gets the column precision.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="Column">The column.</param>
        private static void GetColumnPrecision(StringBuilder builder, IColumn Column)
        {
            var Precision = (Column.Length * 2).Clamp(38, 18);
            builder.Append("(")
                .Append(Precision.ToString(CultureInfo.InvariantCulture))
                .Append(",")
                .Append(Column.Length.Clamp(38, 0).ToString(CultureInfo.InvariantCulture))
                .Append(")");
        }

        private static IEnumerable<string> GetTableCommand(ITable table, StringBuilder builder)
        {
            if (table is null || table.Columns is null)
                return Array.Empty<string>();
            var ReturnValue = new List<string>();
            builder.Append("CREATE TABLE [").Append(table.Schema).Append("].[").Append(table.Name).Append("](");
            var Splitter = string.Empty;
            for (int i = 0, tableColumnsCount = table.Columns.Count; i < tableColumnsCount; i++)
            {
                var Column = table.Columns[i];
                builder
                    .Append(Splitter)
                    .Append("[")
                    .Append(Column.Name)
                    .Append("]")
                    .Append(" ")
                    .Append(Column.DataType.To(SqlDbType.Int).ToString());
                if (Column.DataType == SqlDbType.VarChar.To(DbType.Int32)
                        || Column.DataType == SqlDbType.NVarChar.To(DbType.Int32)
                        || Column.DataType == SqlDbType.Binary.To(DbType.Int32))
                {
                    GetColumnLength(builder, Column);
                }
                else if (Column.DataType == SqlDbType.Decimal.To(DbType.Int32))
                {
                    GetColumnPrecision(builder, Column);
                }
                if (!Column.Nullable)
                {
                    builder.Append(" NOT NULL");
                }
                if (Column.Unique)
                {
                    builder.Append(" UNIQUE");
                }
                if (Column.PrimaryKey)
                {
                    builder.Append(" PRIMARY KEY");
                }
                if (!string.IsNullOrEmpty(Column.Default))
                {
                    builder.Append(" DEFAULT ").Append(Column.Default);
                }
                if (Column.AutoIncrement)
                {
                    builder.Append(" IDENTITY");
                }
                if (!string.IsNullOrEmpty(Column.ComputedColumnSpecification))
                {
                    builder.Append(" AS (").Append(Column.ComputedColumnSpecification).Append(")");
                }
                Splitter = ",";
            }

            if (table.Audit)
            {
                builder.Append(", SysStartTime datetime2 GENERATED ALWAYS AS ROW START NOT NULL, SysEndTime datetime2 GENERATED ALWAYS AS ROW END NOT NULL, PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime)");
            }
            builder.Append(")");
            if (table.Audit)
            {
                builder.Append("WITH ( SYSTEM_VERSIONING = ON (HISTORY_TABLE = [").Append(table.Schema).Append("].[").Append(table.Name).Append("_Audit]) ) ");
            }
            ReturnValue.Add(builder.ToString());
            builder.Clear();
            var Counter = 0;
            for (int i = 0, tableColumnsCount = table.Columns.Count; i < tableColumnsCount; i++)
            {
                var Column = table.Columns[i];
                if (!Column.PrimaryKey)
                {
                    if (Column.Index)
                    {
                        ReturnValue.Add(builder.Append("CREATE")
                            .Append(Column.Unique ? " UNIQUE" : string.Empty)
                            .Append(" INDEX [Index_")
                            .Append(Column.Name)
                            .Append(Counter.ToString(CultureInfo.InvariantCulture))
                            .Append("] ON [")
                            .Append(Column.ParentTable.Schema)
                            .Append("].[")
                            .Append(Column.ParentTable.Name)
                            .Append("]([")
                            .Append(Column.Name)
                            .Append("])")
                            .ToString());
                        builder.Clear();
                    }
                    ++Counter;
                }
            }

            return ReturnValue;
        }
    }
}