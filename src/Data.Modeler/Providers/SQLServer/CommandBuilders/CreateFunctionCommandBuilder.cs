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
    /// Function command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class CreateFunctionCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order => 40;

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
            foreach (Function TempFunction in desiredStructure.Functions)
            {
                var CurrentFunction = (Function)currentStructure.Functions.FirstOrDefault(x => x.Name == TempFunction.Name);
                Commands.Add(CurrentFunction != null ? GetAlterFunctionCommand(TempFunction, CurrentFunction) : GetFunctionCommand(TempFunction));
            }
            return Commands;
        }

        private static IEnumerable<string> GetAlterFunctionCommand(Function function, Function currentFunction)
        {
            if (function == null || currentFunction == null)
                return new List<string>();
            if (function.Definition != currentFunction.Definition && string.IsNullOrEmpty(function.Definition))
                return new List<string>();
            var ReturnValue = new List<string>();
            if (currentFunction == null)
            {
                ReturnValue.Add(GetFunctionCommand(function));
            }
            else if (function.Definition != currentFunction.Definition)
            {
                ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
                    "DROP FUNCTION {0}",
                    function.Name));
                ReturnValue.Add(GetFunctionCommand(function));
            }
            return ReturnValue;
        }

        private static IEnumerable<string> GetFunctionCommand(Function function)
        {
            if (function == null || function.Definition == null)
                return new List<string>();
            var Definition = Regex.Replace(function.Definition, "-- (.*)", "");
            return new string[] { Definition.Replace("\n", " ").Replace("\r", " ") };
        }
    }
}