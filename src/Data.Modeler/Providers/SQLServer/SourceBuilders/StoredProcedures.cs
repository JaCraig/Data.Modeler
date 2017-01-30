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
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Modeler.Providers.SQLServer.SourceBuilders
{
    /// <summary>
    /// StoredProcedure builder, gets info and does diffs for StoredProcedures
    /// </summary>
    public class StoredProcedures : ISourceBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order => 60;

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
                database.AddStoredProcedure(Item.NAME, Item.DEFINITION);
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
            batch.AddQuery(CommandType.Text, @"SELECT sys.procedures.name as NAME,OBJECT_DEFINITION(sys.procedures.object_id) as DEFINITION FROM sys.procedures");
        }
    }
}