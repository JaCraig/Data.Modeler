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
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Create view command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class CreateViewCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order => 50;

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
            if (desiredStructure == null)
                return new List<string>();
            currentStructure = currentStructure ?? new Source(desiredStructure.Name);
            var Commands = new List<string>();
            foreach (View TempView in desiredStructure.Views)
            {
                var CurrentView = (View)currentStructure.Views.FirstOrDefault(x => x.Name == TempView.Name);
                Commands.Add(CurrentView != null ? GetAlterViewCommand(TempView, CurrentView) : GetViewCommand(TempView));
            }
            return Commands;
        }

        private static IEnumerable<string> GetAlterViewCommand(View view, View currentView)
        {
            if (view == null || currentView == null)
                return new List<string>();
            if (view.Definition != currentView.Definition && string.IsNullOrEmpty(view.Definition))
                return new List<string>();
            var ReturnValue = new List<string>();
            if (currentView == null)
            {
                ReturnValue.Add(GetViewCommand(view));
            }
            else if (view.Definition != currentView.Definition)
            {
                ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
                    "DROP VIEW [{0}]",
                    view.Name));
                ReturnValue.Add(GetViewCommand(view));
            }
            return ReturnValue;
        }

        private static IEnumerable<string> GetViewCommand(View view)
        {
            if (view == null || view.Definition == null)
                return new List<string>();
            var Definition = Regex.Replace(view.Definition, "-- (.*)", "");
            return new string[] { Definition.Replace("\n", " ").Replace("\r", " ") };
        }
    }
}