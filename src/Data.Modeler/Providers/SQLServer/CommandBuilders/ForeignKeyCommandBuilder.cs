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
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Foreign key command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class ForeignKeyCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyCommandBuilder"/> class.
        /// </summary>
        /// <param name="objectPool">The object pool.</param>
        public ForeignKeyCommandBuilder(ObjectPool<StringBuilder> objectPool)
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
        public int Order { get; } = 20;

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
                Commands.Add((CurrentTable is null) ? GetForeignKeyCommand(TempTable, Builder) : GetForeignKeyCommand(TempTable, CurrentTable, Builder));
            }
            ObjectPool.Return(Builder);

            return Commands.ToArray();
        }

        /// <summary>
        /// Gets the alter table.
        /// </summary>
        /// <param name="Column">The column.</param>
        /// <param name="ForeignKey">The foreign key.</param>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        private static string GetAlterTable(IColumn Column, IColumn ForeignKey, StringBuilder builder)
        {
            var Command = builder.Append("ALTER TABLE [")
                .Append(Column.ParentTable.Schema)
                .Append("].[")
                .Append(Column.ParentTable.Name)
                .Append("] ADD FOREIGN KEY ([")
                .Append(Column.Name)
                .Append("]) REFERENCES [")
                .Append(ForeignKey.ParentTable.Schema)
                .Append("].[")
                .Append(ForeignKey.ParentTable.Name)
                .Append("]([")
                .Append(ForeignKey.Name)
                .Append("])")
                .Append(Column.OnDeleteCascade ? " ON DELETE CASCADE" : string.Empty)
                .Append(Column.OnUpdateCascade ? " ON UPDATE CASCADE" : string.Empty)
                .Append(Column.OnDeleteSetNull ? " ON DELETE SET NULL" : string.Empty)
                .ToString();
            builder.Clear();
            return Command;
        }

        /// <summary>
        /// Gets the foreign key command.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetForeignKeyCommand(ITable table, StringBuilder builder)
        {
            if (table is null || table.Columns is null)
                return Array.Empty<string>();
            var ReturnValue = new List<string>();
            for (int i = 0, tableColumnsCount = table.Columns.Count; i < tableColumnsCount; i++)
            {
                var Column = table.Columns[i];
                if (Column.ForeignKey.Count > 0)
                {
                    for (int j = 0, ColumnForeignKeyCount = Column.ForeignKey.Count; j < ColumnForeignKeyCount; j++)
                    {
                        var ForeignKey = Column.ForeignKey[j];
                        ReturnValue.Add(GetAlterTable(Column, ForeignKey, builder));
                    }
                }
            }

            return ReturnValue;
        }

        /// <summary>
        /// Gets the foreign key command.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="currentTable">The current table.</param>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetForeignKeyCommand(ITable table, ITable currentTable, StringBuilder builder)
        {
            if (table is null || table.Columns is null)
                return Array.Empty<string>();
            var ReturnValue = new List<string>();
            for (int i = 0, tableColumnsCount = table.Columns.Count; i < tableColumnsCount; i++)
            {
                var Column = table.Columns[i];
                var CurrentColumn = currentTable[Column.Name];
                if (Column.ForeignKey.Count > 0
                    && (CurrentColumn is null || !Column.Equals(CurrentColumn)))
                {
                    foreach (var ForeignKey in Column.ForeignKey.Where(x => CurrentColumn?.ForeignKey.Any(y => y.Name == x.Name
                                                                                                                && y.ParentTable.Name == x.ParentTable.Name) != true))
                    {
                        ReturnValue.Add(GetAlterTable(Column, ForeignKey, builder));
                    }
                }
            }

            return ReturnValue;
        }
    }
}