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
    /// Table column builder, gets info and does diffs for tables
    /// </summary>
    public class TableColumns : ISourceBuilder
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
        /// Fills the database.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="dataSource">The database.</param>
        public void FillSource(List<dynamic> values, ISource dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (values?.Any() != true)
                return;
            for (int i = 0, valuesCount = values.Count; i < valuesCount; i++)
            {
                dynamic Item = values[i];
                SetupColumns(dataSource.Tables.Find(x => x.Name == Item.Table), Item);
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <returns>The command to get the source</returns>
        public string GetCommand()
        {
            return @"SELECT sys.tables.name as [Table],sys.columns.name AS [Column], sys.systypes.name AS [COLUMN_TYPE],
                                                        sys.columns.max_length as [MAX_LENGTH], sys.columns.is_nullable as [IS_NULLABLE],
                                                        sys.columns.is_identity as [IS_IDENTITY], sys.index_columns.index_id as [IS_INDEX],
                                                        key_constraints.name as [PRIMARY_KEY], key_constraints_1.name as [UNIQUE],
                                                        tables_1.name as [FOREIGN_KEY_TABLE], columns_1.name as [FOREIGN_KEY_COLUMN],
                                                        sys.default_constraints.definition as [DEFAULT_VALUE],sys.computed_columns.definition as [ComputedColumnSpecification]
                                                        FROM sys.tables
                                                        INNER JOIN sys.columns on sys.columns.object_id=sys.tables.object_id
                                                        INNER JOIN sys.systypes ON sys.systypes.xtype = sys.columns.system_type_id
                                                        LEFT OUTER JOIN sys.index_columns on sys.index_columns.object_id=sys.tables.object_id and sys.index_columns.column_id=sys.columns.column_id
                                                        LEFT OUTER JOIN sys.key_constraints on sys.key_constraints.parent_object_id=sys.tables.object_id and sys.key_constraints.parent_object_id=sys.index_columns.object_id and sys.index_columns.index_id=sys.key_constraints.unique_index_id and sys.key_constraints.type='PK'
                                                        LEFT OUTER JOIN sys.foreign_key_columns on sys.foreign_key_columns.parent_object_id=sys.tables.object_id and sys.foreign_key_columns.parent_column_id=sys.columns.column_id
                                                        LEFT OUTER JOIN sys.tables as tables_1 on tables_1.object_id=sys.foreign_key_columns.referenced_object_id
                                                        LEFT OUTER JOIN sys.columns as columns_1 on columns_1.column_id=sys.foreign_key_columns.referenced_column_id and columns_1.object_id=tables_1.object_id
                                                        LEFT OUTER JOIN sys.key_constraints as key_constraints_1 on key_constraints_1.parent_object_id=sys.tables.object_id and key_constraints_1.parent_object_id=sys.index_columns.object_id and sys.index_columns.index_id=key_constraints_1.unique_index_id and key_constraints_1.type='UQ'
                                                        LEFT OUTER JOIN sys.default_constraints on sys.default_constraints.object_id=sys.columns.default_object_id
                                                        LEFT OUTER JOIN sys.computed_columns ON sys.computed_columns.object_id=sys.columns.object_id and sys.computed_columns.column_id=sys.columns.column_id
                                                        WHERE sys.systypes.xusertype <> 256";
        }

        /// <summary>
        /// Setups the columns.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="item">The item.</param>
        private static void SetupColumns(ITable table, dynamic item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (table.ContainsColumn(item.Column))
            {
                table.AddForeignKey(item.Column, item.FOREIGN_KEY_TABLE, item.FOREIGN_KEY_COLUMN);
            }
            else
            {
                string ColumnType = item.COLUMN_TYPE;
                table.AddColumn<string>(item.Column,
                    ColumnType.To<string, SqlDbType>().To(DbType.Int32),
                    (item.COLUMN_TYPE == "nvarchar") ? item.MAX_LENGTH / 2 : item.MAX_LENGTH,
                    item.IS_NULLABLE,
                    item.IS_IDENTITY,
                    !(item.IS_INDEX is null),
                    !string.IsNullOrEmpty(item.PRIMARY_KEY),
                    !string.IsNullOrEmpty(item.UNIQUE),
                    item.FOREIGN_KEY_TABLE,
                    item.FOREIGN_KEY_COLUMN,
                    item.DEFAULT_VALUE,
                    item.ComputedColumnSpecification);
            }
        }
    }
}