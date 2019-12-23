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
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Stored procedure command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class StoredProcedureCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; } = 60;

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
            if (desiredStructure == null)
                return Array.Empty<string>();
            currentStructure ??= new Source(desiredStructure.Name);
            var Commands = new List<string>();
            for (int i = 0, desiredStructureStoredProceduresCount = desiredStructure.StoredProcedures.Count; i < desiredStructureStoredProceduresCount; i++)
            {
                var TempStoredProcedure = desiredStructure.StoredProcedures[i];
                var CurrentStoredProcedure = currentStructure.StoredProcedures.Find(x => x.Name == TempStoredProcedure.Name);
                Commands.Add(CurrentStoredProcedure != null ? GetAlterStoredProcedure(TempStoredProcedure, CurrentStoredProcedure) : GetStoredProcedure(TempStoredProcedure));
            }

            return Commands.ToArray();
        }

        private static IEnumerable<string> GetAlterStoredProcedure(IFunction storedProcedure, IFunction currentStoredProcedure)
        {
            if (storedProcedure == null || currentStoredProcedure == null)
                return Array.Empty<string>();
            if (storedProcedure.Definition != currentStoredProcedure.Definition && string.IsNullOrEmpty(storedProcedure.Definition))
                return Array.Empty<string>();
            if (currentStoredProcedure == null)
                return GetStoredProcedure(storedProcedure);
            if (storedProcedure.Definition == currentStoredProcedure.Definition)
                return Array.Empty<string>();
            return new List<string>{string.Format(CultureInfo.CurrentCulture,
                    "DROP PROCEDURE [{0}].[{1}]",
                    storedProcedure.Schema,
                    storedProcedure.Name),
                GetStoredProcedure(storedProcedure)
            };
        }

        private static string[] GetStoredProcedure(IFunction storedProcedure)
        {
            if (storedProcedure == null || storedProcedure.Definition == null)
                return Array.Empty<string>();
            return new string[] { storedProcedure.Definition.RemoveComments().Replace("\n", " ").Replace("\r", " ") };
        }
    }
}