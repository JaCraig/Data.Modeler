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
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Foreign key command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class ForeignKeyCommandBuilder : ICommandBuilder
    {
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
            if (desiredStructure == null)
                return Array.Empty<string>();
            currentStructure ??= new Source(desiredStructure.Name);
            var Commands = new List<string>();
            for (int i = 0, desiredStructureTablesCount = desiredStructure.Tables.Count; i < desiredStructureTablesCount; i++)
            {
                var TempTable = desiredStructure.Tables[i];
                var CurrentTable = currentStructure[TempTable.Name];
                Commands.Add((CurrentTable == null) ? GetForeignKeyCommand(TempTable) : GetForeignKeyCommand(TempTable, CurrentTable));
            }

            return Commands.ToArray();
        }

        private static IEnumerable<string> GetForeignKeyCommand(ITable table)
        {
            if (table == null || table.Columns == null)
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
                        var Command = string.Format(CultureInfo.InvariantCulture,
                                    "ALTER TABLE [{0}].[{1}] ADD FOREIGN KEY ([{2}]) REFERENCES [{3}].[{4}]([{5}])",
                                    Column.ParentTable.Schema,
                                    Column.ParentTable.Name,
                                    Column.Name,
                                    ForeignKey.ParentTable.Schema,
                                    ForeignKey.ParentTable.Name,
                                    ForeignKey.Name);
                        if (Column.OnDeleteCascade)
                            Command += " ON DELETE CASCADE";
                        if (Column.OnUpdateCascade)
                            Command += " ON UPDATE CASCADE";
                        if (Column.OnDeleteSetNull)
                            Command += " ON DELETE SET NULL";
                        ReturnValue.Add(Command);
                    }
                }
            }

            return ReturnValue;
        }

        private static IEnumerable<string> GetForeignKeyCommand(ITable table, ITable currentTable)
        {
            if (table == null || table.Columns == null)
                return Array.Empty<string>();
            var ReturnValue = new List<string>();
            for (int i = 0, tableColumnsCount = table.Columns.Count; i < tableColumnsCount; i++)
            {
                var Column = table.Columns[i];
                var CurrentColumn = currentTable[Column.Name];
                if (Column.ForeignKey.Count > 0
                    && (CurrentColumn == null || !Column.Equals(CurrentColumn)))
                {
                    foreach (var ForeignKey in Column.ForeignKey.Where(x => CurrentColumn?.ForeignKey.Any(y => y.Name == x.Name
                                                                                                                && y.ParentTable.Name == x.ParentTable.Name) != true))
                    {
                        var Command = string.Format(CultureInfo.InvariantCulture,
                            "ALTER TABLE [{0}].[{1}] ADD FOREIGN KEY ([{2}]) REFERENCES [{3}].[{4}]([{5}])",
                            Column.ParentTable.Schema,
                            Column.ParentTable.Name,
                            Column.Name,
                            ForeignKey.ParentTable.Schema,
                            ForeignKey.ParentTable.Name,
                            ForeignKey.Name);
                        if (Column.OnDeleteCascade)
                            Command += " ON DELETE CASCADE";
                        if (Column.OnUpdateCascade)
                            Command += " ON UPDATE CASCADE";
                        if (Column.OnDeleteSetNull)
                            Command += " ON DELETE SET NULL";
                        ReturnValue.Add(Command);
                    }
                }
            }

            return ReturnValue;
        }
    }
}