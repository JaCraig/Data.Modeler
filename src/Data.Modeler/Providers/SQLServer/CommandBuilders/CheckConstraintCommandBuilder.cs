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
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// CheckConstraint command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class CheckConstraintCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckConstraintCommandBuilder"/> class.
        /// </summary>
        /// <param name="objectPool">The object pool.</param>
        public CheckConstraintCommandBuilder(ObjectPool<StringBuilder> objectPool)
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
        public int Order { get; } = 31;

        /// <summary>
        /// Provider name associated with the schema generator
        /// </summary>
        public DbProviderFactory[] Providers { get; } = new DbProviderFactory[] { SqlClientFactory.Instance, System.Data.SqlClient.SqlClientFactory.Instance };

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
                Commands.Add((CurrentTable is null) ? GetCheckConstraintCommand(TempTable, Builder) : GetAlterCheckConstraintCommand(TempTable, CurrentTable, Builder));
            }
            ObjectPool.Return(Builder);

            return Commands.ToArray();
        }

        /// <summary>
        /// Gets the alter check constraint command.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="currentTable">The current table.</param>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        private static string[] GetAlterCheckConstraintCommand(ITable table, ITable currentTable, StringBuilder builder)
        {
            if (table is null || table.Constraints is null)
                return Array.Empty<string>();
            var ReturnValue = new List<string>();
            for (int i = 0, tableConstraintsCount = table.Constraints.Count; i < tableConstraintsCount; i++)
            {
                var CheckConstraint = table.Constraints[i];
                var CheckConstraint2 = currentTable.Constraints.Find(x => CheckConstraint.Name == x.Name);
                if (CheckConstraint2 is null)
                {
                    ReturnValue.Add(GetAlterTable(builder, CheckConstraint));
                    builder.Clear();
                }
                else if (!string.Equals(CheckConstraint.Definition, CheckConstraint2.Definition, StringComparison.OrdinalIgnoreCase))
                {
                    ReturnValue.Add(builder.Append("ALTER TABLE [")
                        .Append(CheckConstraint.ParentTable.Schema)
                        .Append("].[")
                        .Append(CheckConstraint.ParentTable.Name)
                        .Append("] DROP CONSTRAINT [")
                        .Append(CheckConstraint.Name)
                        .Append(']')
                        .ToString());
                    builder.Clear();
                    ReturnValue.Add(GetAlterTable(builder, CheckConstraint));
                    builder.Clear();
                }
            }

            return ReturnValue.ToArray();
        }

        /// <summary>
        /// Gets the alter table.
        /// </summary>
        /// <param name="Builder">The builder.</param>
        /// <param name="CheckConstraint">The check constraint.</param>
        /// <returns>The alter table constraint.</returns>
        private static string GetAlterTable(StringBuilder Builder, ICheckConstraint CheckConstraint)
        {
            return Builder.Append("ALTER TABLE [").Append(CheckConstraint.ParentTable.Schema)
                                    .Append("].[")
                                    .Append(CheckConstraint.ParentTable.Name)
                                    .Append("] ADD CONSTRAINT [")
                                    .Append(CheckConstraint.Name)
                                    .Append("] CHECK (")
                                    .Append(CheckConstraint.Definition)
                                    .Append(')').ToString();
        }

        /// <summary>
        /// Gets the check constraint command.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        private static string[] GetCheckConstraintCommand(ITable table, StringBuilder builder)
        {
            if (table is null || table.Constraints is null)
                return Array.Empty<string>();
            var ReturnValue = new string[table.Constraints.Count];
            for (int i = 0, tableConstraintsCount = table.Constraints.Count; i < tableConstraintsCount; i++)
            {
                var CheckConstraint = table.Constraints[i];
                ReturnValue[i] = GetAlterTable(builder, CheckConstraint);
                builder.Clear();
            }

            return ReturnValue;
        }
    }
}