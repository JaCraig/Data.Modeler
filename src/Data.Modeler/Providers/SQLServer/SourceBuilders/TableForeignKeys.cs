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

using Data.Modeler.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;

namespace Data.Modeler.Providers.SQLServer.SourceBuilders
{
    /// <summary>
    /// Table foreign keys, gets info and does diffs for tables
    /// </summary>
    public class TableForeignKeys : ISourceBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; } = 40;

        /// <summary>
        /// Provider name associated with the schema generator
        /// </summary>
        public DbProviderFactory Provider { get; } = SqlClientFactory.Instance;

        /// <summary>
        /// Fills the database.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="dataSource">The database.</param>
        public void FillSource(List<dynamic> values, ISource dataSource)
        {
            if (dataSource is null)
                throw new ArgumentNullException(nameof(dataSource));
            for (int i = 0, dataSourceTablesCount = dataSource.Tables.Count; i < dataSourceTablesCount; i++)
            {
                var TempTable = dataSource.Tables[i];
                TempTable.SetupForeignKeys();
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <returns>The command to get the source</returns>
        public string GetCommand() => "SELECT sys.tables.name as [Table] FROM sys.tables";
    }
}