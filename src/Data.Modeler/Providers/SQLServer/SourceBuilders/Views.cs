﻿/*
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
using System.Linq;

namespace Data.Modeler.Providers.SQLServer.SourceBuilders
{
    /// <summary>
    /// View builder, gets info and does diffs for Views
    /// </summary>
    public class Views : ISourceBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order => 50;

        /// <summary>
        /// Fills the database.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="database">The database.</param>
        public void FillSource(IEnumerable<dynamic> values, ISource database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (values == null || values.Count() == 0)
                return;
            foreach (dynamic Item in values)
            {
                SetupViews((View)database.Views.FirstOrDefault(x => x.Name == Item.View), Item);
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <param name="batch">The batch.</param>
        public void GetCommand(SQLHelper.SQLHelper batch)
        {
            if (batch == null)
                throw new ArgumentNullException(nameof(batch));
            batch.AddQuery(CommandType.Text, @"SELECT sys.views.name as [View],OBJECT_DEFINITION(sys.views.object_id) as Definition,
                                                        sys.columns.name AS [Column], sys.systypes.name AS [COLUMN_TYPE],
                                                        sys.columns.max_length as [MAX_LENGTH], sys.columns.is_nullable as [IS_NULLABLE]
                                                        FROM sys.views
                                                        INNER JOIN sys.columns on sys.columns.object_id=sys.views.object_id
                                                        INNER JOIN sys.systypes ON sys.systypes.xtype = sys.columns.system_type_id
                                                        WHERE sys.systypes.xusertype <> 256");
        }

        /// <summary>
        /// Setups the views.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="item">The item.</param>
        private static void SetupViews(View table, dynamic item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            var View = table;
            View.Definition = item.Definition;
            string ColumnName = item.Column;
            string ColumnType = item.COLUMN_TYPE;
            int MaxLength = item.MAX_LENGTH;
            if (ColumnType == "nvarchar")
                MaxLength /= 2;
            bool Nullable = item.IS_NULLABLE;
            View.AddColumn<string>(ColumnName, ColumnType.To<string, SqlDbType>().To(DbType.Int32), MaxLength, Nullable);
        }
    }
}