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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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
        public int Order => 60;

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
            var Commands = new List<string>();
            foreach (StoredProcedure TempStoredProcedure in desiredStructure.StoredProcedures)
            {
                var CurrentStoredProcedure = (StoredProcedure)currentStructure.StoredProcedures.FirstOrDefault(x => x.Name == TempStoredProcedure.Name);
                Commands.Add(CurrentStoredProcedure != null ? GetAlterStoredProcedure(TempStoredProcedure, CurrentStoredProcedure) : GetStoredProcedure(TempStoredProcedure));
            }
            return Commands;
        }

        private static IEnumerable<string> GetAlterStoredProcedure(StoredProcedure storedProcedure, StoredProcedure currentStoredProcedure)
        {
            if (storedProcedure == null || currentStoredProcedure == null)
                return new List<string>();
            if (storedProcedure.Definition != currentStoredProcedure.Definition && string.IsNullOrEmpty(storedProcedure.Definition))
                return new List<string>();
            var ReturnValue = new List<string>();
            if (currentStoredProcedure == null)
            {
                ReturnValue.Add(GetStoredProcedure(storedProcedure));
            }
            else if (storedProcedure.Definition != currentStoredProcedure.Definition)
            {
                ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
                    "DROP PROCEDURE {0}",
                    storedProcedure.Name));
                ReturnValue.Add(GetStoredProcedure(storedProcedure));
            }
            return ReturnValue;
        }

        private static IEnumerable<string> GetStoredProcedure(StoredProcedure storedProcedure)
        {
            if (storedProcedure == null || storedProcedure.Definition == null)
                return new List<string>();
            var Definition = Regex.Replace(storedProcedure.Definition, "-- (.*)", "");
            return new string[] { Definition.Replace("\n", " ").Replace("\r", " ") };
        }
    }
}