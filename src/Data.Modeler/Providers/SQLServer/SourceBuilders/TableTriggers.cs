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
using Data.Modeler.Providers.Enums;
using Data.Modeler.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Data.Modeler.Providers.SQLServer.SourceBuilders
{
    /// <summary>
    /// Table trigger builder, gets info and does diffs for tables
    /// </summary>
    public class TableTriggers : ISourceBuilder
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
        /// Fills the database.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="database">The database.</param>
        public void FillSource(IEnumerable<dynamic> values, ISource database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (values == null || !values.Any())
                return;
            foreach (dynamic Item in values)
            {
                SetupTriggers(database.Tables.FirstOrDefault(x => x.Name == Item.Table), Item);
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <returns>The command to get the source</returns>
        public string GetCommand()
        {
            return @"SELECT sys.tables.name as [Table],sys.triggers.name as Name,sys.trigger_events.type as Type,
                                                                OBJECT_DEFINITION(sys.triggers.object_id) as Definition
                                                                FROM sys.triggers
                                                                INNER JOIN sys.trigger_events ON sys.triggers.object_id=sys.trigger_events.object_id
                                                                INNER JOIN sys.tables on sys.triggers.parent_id=sys.tables.object_id";
        }

        /// <summary>
        /// Setups the columns.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="item">The item.</param>
        private static void SetupTriggers(ITable table, dynamic item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            string Name = item.Name;
            int Type = item.Type;
            string Definition = item.Definition;
            table.AddTrigger(Name, Definition, Type.ToString(CultureInfo.InvariantCulture).To<string, TriggerType>());
        }
    }
}