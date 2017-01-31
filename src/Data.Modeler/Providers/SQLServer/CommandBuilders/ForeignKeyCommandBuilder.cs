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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
        public int Order => 20;

        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <param name="desiredStructure">The desired structure.</param>
        /// <param name="currentStructure">The current structure.</param>
        /// <returns>
        /// The list of commands needed to change the structure from the current to the desired structure
        /// </returns>
        public IEnumerable<string> GetCommands(ISource desiredStructure, ISource currentStructure)
        {
            var Commands = new List<string>();
            foreach (Table TempTable in desiredStructure.Tables)
            {
                ITable CurrentTable = currentStructure[TempTable.Name];
                Commands.Add((CurrentTable == null) ? GetForeignKeyCommand(TempTable) : GetForeignKeyCommand(TempTable, CurrentTable));
            }
            return Commands;
        }

        private static IEnumerable<string> GetForeignKeyCommand(Table table)
        {
            if (table == null || table.Columns == null)
                return new List<string>();
            var ReturnValue = new List<string>();
            foreach (IColumn Column in table.Columns)
            {
                if (Column.ForeignKey.Count > 0)
                {
                    foreach (IColumn ForeignKey in Column.ForeignKey)
                    {
                        var Command = string.Format(CultureInfo.CurrentCulture,
                            "ALTER TABLE {0} ADD FOREIGN KEY ({1}) REFERENCES {2}({3})",
                            Column.ParentTable.Name,
                            Column.Name,
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

        private static IEnumerable<string> GetForeignKeyCommand(Table table, ITable currentTable)
        {
            if (table == null || table.Columns == null)
                return new List<string>();
            var ReturnValue = new List<string>();
            foreach (IColumn Column in table.Columns)
            {
                IColumn CurrentColumn = currentTable[Column.Name];
                if (Column.ForeignKey.Count > 0
                    && (CurrentColumn == null || CurrentColumn.ForeignKey.Count != Column.ForeignKey.Count))
                {
                    foreach (IColumn ForeignKey in Column.ForeignKey)
                    {
                        var Command = string.Format(CultureInfo.CurrentCulture,
                            "ALTER TABLE {0} ADD FOREIGN KEY ({1}) REFERENCES {2}({3})",
                            Column.ParentTable.Name,
                            Column.Name,
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