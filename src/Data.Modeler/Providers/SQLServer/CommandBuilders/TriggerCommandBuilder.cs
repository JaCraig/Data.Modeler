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
using System.Text.RegularExpressions;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Trigger command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class TriggerCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order => 30;

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
                Commands.Add((CurrentTable == null) ? GetTriggerCommand(TempTable) : GetAlterTriggerCommand(TempTable, CurrentTable));
            }
            return Commands;
        }

        private static IEnumerable<string> GetAlterTriggerCommand(Table table, ITable currentTable)
        {
            if (table == null || table.Triggers == null)
                return new List<string>();
            var ReturnValue = new List<string>();
            foreach (Trigger Trigger in table.Triggers)
            {
                var Trigger2 = currentTable.Triggers.FirstOrDefault(x => Trigger.Name == x.Name);
                string Definition1 = Trigger.Definition;
                var Definition2 = Trigger2.Definition;
                if (Definition2 == null)
                {
                    var Definition = Regex.Replace(Trigger.Definition, "-- (.*)", "");
                    ReturnValue.Add(Definition.Replace("\n", " ").Replace("\r", " "));
                }
                else if (!string.Equals(Definition1, Definition2, StringComparison.OrdinalIgnoreCase))
                {
                    ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
                        "DROP TRIGGER [{0}]",
                        Trigger.Name));
                    var Definition = Regex.Replace(Trigger.Definition, "-- (.*)", "");
                    ReturnValue.Add(Definition.Replace("\n", " ").Replace("\r", " "));
                }
            }
            return ReturnValue;
        }

        private static IEnumerable<string> GetTriggerCommand(Table table)
        {
            if (table == null || table.Triggers == null)
                return new List<string>();
            var ReturnValue = new List<string>();
            foreach (Trigger Trigger in table.Triggers)
            {
                var Definition = Regex.Replace(Trigger.Definition, "-- (.*)", "");
                ReturnValue.Add(Definition.Replace("\n", " ").Replace("\r", " "));
            }
            return ReturnValue;
        }
    }
}