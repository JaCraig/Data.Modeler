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
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Data.Modeler.Providers.SQLServer.SourceBuilders
{
    /// <summary>
    /// StoredProcedure column builder, gets info and does diffs for StoredProcedures
    /// </summary>
    public class StoredProcedureColumns : ISourceBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; } = 70;

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
            if (values?.Any() != true)
                return;
            for (int i = 0, valuesCount = values.Count; i < valuesCount; i++)
            {
                dynamic Item = values[i];
                SetupStoredProcedures((ITable)dataSource.StoredProcedures.Find(x => x.Name == Item.Procedure), Item);
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <returns>The command to get the source</returns>
        public string GetCommand()
        {
            return @"SELECT sys.procedures.name as [Procedure],sys.systypes.name as TYPE,sys.parameters.name as NAME,
sys.parameters.max_length as LENGTH,sys.parameters.default_value as [DEFAULT VALUE]
FROM sys.procedures
INNER JOIN sys.parameters on sys.procedures.object_id=sys.parameters.object_id
INNER JOIN sys.systypes on sys.systypes.xusertype=sys.parameters.system_type_id
WHERE sys.systypes.xusertype <> 256";
        }

        /// <summary>
        /// Setups the stored procedures.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <param name="item">The item.</param>
        private static void SetupStoredProcedures(ITable storedProcedure, dynamic item)
        {
            if (storedProcedure is null)
                throw new ArgumentNullException(nameof(storedProcedure));
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            string Type = item.TYPE;
            string Name = item.NAME;
            int Length = item.LENGTH;
            if (Type == "nvarchar")
                Length /= 2;
            string Default = item.DEFAULT_VALUE;
            storedProcedure.AddColumn<string>(Name, Type.To<string, SqlDbType>().To(DbType.Int32), Length, defaultValue: Default);
        }
    }
}