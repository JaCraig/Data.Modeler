﻿/*
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
            if (desiredStructure == null)
                return Array.Empty<string>();
            currentStructure ??= new Source(desiredStructure.Name);
            var Commands = new List<string>();
            for (int i = 0, desiredStructureTablesCount = desiredStructure.Tables.Count; i < desiredStructureTablesCount; i++)
            {
                var TempTable = desiredStructure.Tables[i];
                var CurrentTable = currentStructure[TempTable.Name];
                Commands.Add((CurrentTable == null) ? GetTableCommand(TempTable) : GetAlterTableCommand(TempTable, CurrentTable));
            }

            return Commands.ToArray();
        }

        private static IEnumerable<string> GetAlterTableCommand(ITable table, ITable currentTable)
        {
            if (table == null || table.Columns == null)
                return Array.Empty<string>();
            var ReturnValue = new List<string>();
            for (int i = 0, tableColumnsCount = table.Columns.Count; i < tableColumnsCount; i++)
            {
                var Column = table.Columns[i];
                var CurrentColumn = currentTable[Column.Name];
                string Command;
                if (CurrentColumn == null)
                {
                    if (string.IsNullOrEmpty(Column.ComputedColumnSpecification))
                    {
                        Command = string.Format(CultureInfo.InvariantCulture,
                            "ALTER TABLE [{0}].[{1}] ADD [{2}] {3}",
                            table.Schema,
                            table.Name,
                            Column.Name,
                            Column.DataType.To(SqlDbType.Int).ToString());
                        if (Column.DataType == SqlDbType.VarChar.To(DbType.Int32)
                            || Column.DataType == SqlDbType.NVarChar.To(DbType.Int32)
                            || Column.DataType == SqlDbType.Binary.To(DbType.Int32))
                        {
                            Command += (Column.Length <= 0 || Column.Length >= 4000) ?
                                            "(MAX)" :
                                            "(" + Column.Length.ToString(CultureInfo.InvariantCulture) + ")";
                        }
                        else if (Column.DataType == SqlDbType.Decimal.To(DbType.Int32))
                        {
                            var Precision = (Column.Length * 2).Clamp(38, 18);
                            Command += "(" + Precision.ToString(CultureInfo.InvariantCulture) + "," + Column.Length.Clamp(38, 0).ToString(CultureInfo.InvariantCulture) + ")";
                        }
                    }
                    else
                    {
                        Command = string.Format(CultureInfo.InvariantCulture,
                            "ALTER TABLE [{0}].[{1}] ADD [{2}] AS ({3})",
                            table.Schema,
                            table.Name,
                            Column.Name,
                            Column.ComputedColumnSpecification);
                    }
                    ReturnValue.Add(Command);
                }
                else if (CurrentColumn.DataType.To(SqlDbType.Int) != Column.DataType.To(SqlDbType.Int)
                    || (CurrentColumn.DataType.To(SqlDbType.Int) == Column.DataType.To(SqlDbType.Int)
                        && CurrentColumn.DataType == SqlDbType.NVarChar.To(DbType.Int32)
                        && CurrentColumn.Length != Column.Length
                        && CurrentColumn.Length.Between(1, 4000)
                        && Column.Length.Between(1, 4000)))
                {
                    Command = string.Format(CultureInfo.InvariantCulture,
                        "ALTER TABLE [{0}].[{1}] ALTER COLUMN [{2}] {3}",
                        table.Schema,
                        table.Name,
                        Column.Name,
                        Column.DataType.To(SqlDbType.Int).ToString());
                    if (Column.DataType == SqlDbType.VarChar.To(DbType.Int32)
                        || Column.DataType == SqlDbType.NVarChar.To(DbType.Int32)
                        || Column.DataType == SqlDbType.Binary.To(DbType.Int32))
                    {
                        Command += (Column.Length <= 0 || Column.Length >= 4000) ?
                                        "(MAX)" :
                                        "(" + Column.Length.ToString(CultureInfo.InvariantCulture) + ")";
                    }
                    else if (Column.DataType == SqlDbType.Decimal.To(DbType.Int32))
                    {
                        var Precision = (Column.Length * 2).Clamp(38, 18);
                        Command += "(" + Precision.ToString(CultureInfo.InvariantCulture) + "," + Column.Length.Clamp(38, 0).ToString(CultureInfo.InvariantCulture) + ")";
                    }
                    ReturnValue.Add(Command);
                }
            }

            return ReturnValue;
        }

        private static IEnumerable<string> GetTableCommand(ITable table)
        {
            if (table == null || table.Columns == null)
                return Array.Empty<string>();
            var ReturnValue = new List<string>();
            var Builder = new StringBuilder();
            Builder.Append("CREATE TABLE [").Append(table.Schema).Append("].[").Append(table.Name).Append("](");
            var Splitter = "";
            for (int i = 0, tableColumnsCount = table.Columns.Count; i < tableColumnsCount; i++)
            {
                var Column = table.Columns[i];
                Builder
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
                    Builder.Append((Column.Length <= 0 || Column.Length >= 4000) ?
                                    "(MAX)" :
                                    "(" + Column.Length.ToString(CultureInfo.InvariantCulture) + ")");
                }
                else if (Column.DataType == SqlDbType.Decimal.To(DbType.Int32))
                {
                    var Precision = (Column.Length * 2).Clamp(38, 18);
                    Builder.Append("(").Append(Precision).Append(",").Append(Column.Length.Clamp(38, 0)).Append(")");
                }
                if (!Column.Nullable)
                {
                    Builder.Append(" NOT NULL");
                }
                if (Column.Unique)
                {
                    Builder.Append(" UNIQUE");
                }
                if (Column.PrimaryKey)
                {
                    Builder.Append(" PRIMARY KEY");
                }
                if (!string.IsNullOrEmpty(Column.Default))
                {
                    Builder.Append(" DEFAULT ").Append(Column.Default);
                }
                if (Column.AutoIncrement)
                {
                    Builder.Append(" IDENTITY");
                }
                if (!string.IsNullOrEmpty(Column.ComputedColumnSpecification))
                {
                    Builder.AppendFormat(CultureInfo.InvariantCulture, " AS ({0})", Column.ComputedColumnSpecification);
                }
                Splitter = ",";
            }

            if (table.Audit)
            {
                Builder.Append(", SysStartTime datetime2 GENERATED ALWAYS AS ROW START NOT NULL, SysEndTime datetime2 GENERATED ALWAYS AS ROW END NOT NULL, PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime)");
            }
            Builder.Append(")");
            if (table.Audit)
            {
                Builder.AppendFormat(CultureInfo.InvariantCulture, "WITH ( SYSTEM_VERSIONING = ON (HISTORY_TABLE = [{0}].[{1}]) ) ", table.Schema, table.Name + "_Audit");
            }
            ReturnValue.Add(Builder.ToString());
            var Counter = 0;
            for (int i = 0, tableColumnsCount = table.Columns.Count; i < tableColumnsCount; i++)
            {
                var Column = table.Columns[i];
                if (!Column.PrimaryKey)
                {
                    if (Column.Index && Column.Unique)
                    {
                        ReturnValue.Add(string.Format(CultureInfo.InvariantCulture,
                            "CREATE UNIQUE INDEX [Index_{0}{1}] ON [{2}].[{3}]([{4}])",
                            Column.Name,
                            Counter.ToString(CultureInfo.InvariantCulture),
                            Column.ParentTable.Schema,
                            Column.ParentTable.Name,
                            Column.Name));
                    }
                    else if (Column.Index)
                    {
                        ReturnValue.Add(string.Format(CultureInfo.InvariantCulture,
                            "CREATE INDEX [Index_{0}{1}] ON [{2}].[{3}]([{4}])",
                            Column.Name,
                            Counter.ToString(CultureInfo.InvariantCulture),
                            Column.ParentTable.Schema,
                            Column.ParentTable.Name,
                            Column.Name));
                    }
                    ++Counter;
                }
            }

            return ReturnValue;
        }
    }
}