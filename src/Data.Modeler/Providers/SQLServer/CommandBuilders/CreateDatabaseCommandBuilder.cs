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
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Create database command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class CreateDatabaseCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order => 1;

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
            var Result = new List<string>();
            if (currentStructure == null)
            {
                Result.Add(string.Format(CultureInfo.CurrentCulture,
                   "CREATE DATABASE [{0}]",
                   desiredStructure.Name));
            }

            return Result;
        }
    }
}