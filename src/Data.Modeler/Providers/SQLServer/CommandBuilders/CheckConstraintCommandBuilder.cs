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
    /// CheckConstraint command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class CheckConstraintCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order => 31;

        /// <summary>
        /// Provider name associated with the schema generator
        /// </summary>
        public DbProviderFactory Provider => SqlClientFactory.Instance;

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
            if (desiredStructure == null)
                return new List<string>();
            currentStructure = currentStructure ?? new Source(desiredStructure.Name);
            var Commands = new List<string>();
            foreach (Table TempTable in desiredStructure.Tables)
            {
                ITable CurrentTable = currentStructure[TempTable.Name];
                Commands.Add((CurrentTable == null) ? GetCheckConstraintCommand(TempTable) : GetAlterCheckConstraintCommand(TempTable, CurrentTable));
            }
            return Commands;
        }

        private static IEnumerable<string> GetAlterCheckConstraintCommand(Table table, ITable currentTable)
        {
            if (table == null || table.Constraints == null)
                return new List<string>();
            var ReturnValue = new List<string>();
            foreach (CheckConstraint CheckConstraint in table.Constraints)
            {
                var CheckConstraint2 = currentTable.Constraints.FirstOrDefault(x => CheckConstraint.Name == x.Name);
                if (CheckConstraint2 == null)
                {
                    ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
                            "ALTER TABLE [{0}] ADD CONSTRAINT [{1}] CHECK ({2})",
                            CheckConstraint.ParentTable.Name,
                            CheckConstraint.Name,
                            CheckConstraint.Definition));
                }
                else if (!string.Equals(CheckConstraint.Definition, CheckConstraint2.Definition, StringComparison.OrdinalIgnoreCase))
                {
                    ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
                        "ALTER TABLE [{0}] DROP CONSTRAINT [{1}]",
                        CheckConstraint.ParentTable.Name,
                        CheckConstraint.Name));
                    ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
                        "ALTER TABLE [{0}] ADD CONSTRAINT [{1}] CHECK ({2})",
                        CheckConstraint.ParentTable.Name,
                        CheckConstraint.Name,
                        CheckConstraint.Definition));
                }
            }
            return ReturnValue;
        }

        private static IEnumerable<string> GetCheckConstraintCommand(Table table)
        {
            if (table == null || table.Constraints == null)
                return new List<string>();
            var ReturnValue = new List<string>();
            foreach (CheckConstraint CheckConstraint in table.Constraints)
            {
                ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
                            "ALTER TABLE [{0}] ADD CONSTRAINT [{1}] CHECK ({2})",
                            CheckConstraint.ParentTable.Name,
                            CheckConstraint.Name,
                            CheckConstraint.Definition));
            }
            return ReturnValue;
        }
    }
}