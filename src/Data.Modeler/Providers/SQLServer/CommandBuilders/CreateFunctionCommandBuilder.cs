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
    /// Function command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class CreateFunctionCommandBuilder : ICommandBuilder
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
        /// Gets the commands.
        /// </summary>
        /// <param name="desiredStructure">The desired structure.</param>
        /// <param name="currentStructure">The current structure.</param>
        /// <returns>
        /// The list of commands needed to change the structure from the current to the desired structure
        /// </returns>
        public string[] GetCommands(ISource desiredStructure, ISource currentStructure)
        {
            if (desiredStructure == null)
                return Array.Empty<string>();
            currentStructure = currentStructure ?? new Source(desiredStructure.Name);
            var Commands = new List<string>();
            for (int i = 0, desiredStructureFunctionsCount = desiredStructure.Functions.Count; i < desiredStructureFunctionsCount; i++)
            {
                var TempFunction = desiredStructure.Functions[i];
                var CurrentFunction = currentStructure.Functions.Find(x => x.Name == TempFunction.Name);
                Commands.Add(CurrentFunction != null ? GetAlterFunctionCommand(TempFunction, CurrentFunction) : GetFunctionCommand(TempFunction));
            }

            return Commands.ToArray();
        }

        private static IEnumerable<string> GetAlterFunctionCommand(IFunction function, IFunction currentFunction)
        {
            if (function == null || currentFunction == null)
                return Array.Empty<string>();
            if (function.Definition != currentFunction.Definition && string.IsNullOrEmpty(function.Definition))
                return Array.Empty<string>();
            if (currentFunction == null)
                return GetFunctionCommand(function);
            if (function.Definition == currentFunction.Definition)
                return Array.Empty<string>();
            return new List<string>
            {
                string.Format(CultureInfo.CurrentCulture,
                    "DROP FUNCTION [{0}].[{1}]",
                    function.Schema,
                    function.Name),
                GetFunctionCommand(function)
            };
        }

        private static string[] GetFunctionCommand(IFunction function)
        {
            if (function == null || function.Definition == null)
                return Array.Empty<string>();
            return new string[] { function.Definition.RemoveComments().Replace("\n", " ").Replace("\r", " ") };
        }
    }
}