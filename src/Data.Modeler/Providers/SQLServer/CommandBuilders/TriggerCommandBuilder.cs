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
using System.Text;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Trigger command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class TriggerCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerCommandBuilder"/> class.
        /// </summary>
        /// <param name="objectPool">The object pool.</param>
        public TriggerCommandBuilder(ObjectPool<StringBuilder> objectPool)
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
        public int Order { get; } = 30;

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
                Commands.Add((CurrentTable is null) ? GetTriggerCommand(TempTable) : GetAlterTriggerCommand(TempTable, CurrentTable, Builder));
            }
            ObjectPool.Return(Builder);

            return Commands.ToArray();
        }

        private static IEnumerable<string> GetAlterTriggerCommand(ITable table, ITable currentTable, StringBuilder builder)
        {
            if (table is null || table.Triggers is null)
                return Array.Empty<string>();
            var ReturnValue = new List<string>();
            for (int i = 0, tableTriggersCount = table.Triggers.Count; i < tableTriggersCount; i++)
            {
                var Trigger = table.Triggers[i];
                var Trigger2 = currentTable.Triggers.Find(x => Trigger.Name == x.Name);
                var Definition1 = Trigger.Definition;
                var Definition2 = Trigger2.Definition;
                if (Definition2 is null)
                {
                    ReturnValue.Add(Trigger
                        .Definition
                        .RemoveComments()
                        .Replace("\n", " ", StringComparison.Ordinal)
                        .Replace("\r", " ", StringComparison.Ordinal));
                }
                else if (!string.Equals(Definition1, Definition2, StringComparison.OrdinalIgnoreCase))
                {
                    ReturnValue.Add(builder.Append("DROP TRIGGER [").Append(Trigger.Name).Append("]").ToString());
                    ReturnValue.Add(Trigger
                        .Definition
                        .RemoveComments()
                        .Replace("\n", " ", StringComparison.Ordinal)
                        .Replace("\r", " ", StringComparison.Ordinal));
                    builder.Clear();
                }
            }

            return ReturnValue;
        }

        private static IEnumerable<string> GetTriggerCommand(ITable table)
        {
            if (table is null || table.Triggers is null)
                return Array.Empty<string>();
            var ReturnValue = new List<string>();
            for (int i = 0, tableTriggersCount = table.Triggers.Count; i < tableTriggersCount; i++)
            {
                var Trigger = table.Triggers[i];
                ReturnValue.Add(Trigger
                    .Definition
                    .RemoveComments()
                    .Replace("\n", " ", StringComparison.Ordinal)
                    .Replace("\r", " ", StringComparison.Ordinal));
            }

            return ReturnValue;
        }
    }
}