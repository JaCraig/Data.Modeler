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
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Data.Modeler.Providers.SQLServer.SourceBuilders
{
    /// <summary>
    /// Schemas
    /// </summary>
    /// <seealso cref="ISourceBuilder"/>
    public class Schemas : ISourceBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; } = 5;

        /// <summary>
        /// Provider name associated with the schema generator
        /// </summary>
        public DbProviderFactory[] Providers { get; } = new DbProviderFactory[] { SqlClientFactory.Instance, System.Data.SqlClient.SqlClientFactory.Instance };

        /// <summary>
        /// Fills the source.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="dataSource">The data source.</param>
        /// <exception cref="ArgumentNullException">dataSource</exception>
        public void FillSource(List<dynamic> values, ISource dataSource)
        {
            if (dataSource is null)
                throw new ArgumentNullException(nameof(dataSource));
            if (values?.Any() != true)
                return;
            for (int i = 0, valuesCount = values.Count; i < valuesCount; i++)
            {
                dynamic Item = values[i];
                dataSource.Schemas.Add(Item.Name);
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <returns>The command to get the source</returns>
        public string GetCommand() => "SELECT name as [Name] FROM sys.schemas WHERE schema_id < 16384 AND schema_id > 4";
    }
}