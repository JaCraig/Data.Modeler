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
    /// Create view command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class CreateViewCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; } = 50;

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
            for (int i = 0, desiredStructureViewsCount = desiredStructure.Views.Count; i < desiredStructureViewsCount; i++)
            {
                IFunction TempView = desiredStructure.Views[i];
                var CurrentView = (View)currentStructure.Views.Find(x => x.Name == TempView.Name);
                Commands.Add(CurrentView != null ? GetAlterViewCommand(TempView, CurrentView) : GetViewCommand(TempView));
            }

            return Commands.ToArray();
        }

        private static IEnumerable<string> GetAlterViewCommand(IFunction view, IFunction currentView)
        {
            if (view == null || currentView == null)
                return Array.Empty<string>();
            if (view.Definition != currentView.Definition && string.IsNullOrEmpty(view.Definition))
                return Array.Empty<string>();
            if (currentView == null)
                return GetViewCommand(view);
            if (view.Definition == currentView.Definition)
                return Array.Empty<string>();
            return new List<string> {
                string.Format(CultureInfo.CurrentCulture,
                    "DROP VIEW [{0}].[{1}]",
                    view.Schema,
                    view.Name),
                GetViewCommand(view)
            };
        }

        private static IEnumerable<string> GetViewCommand(IFunction view)
        {
            if (view == null || view.Definition == null)
                return Array.Empty<string>();
            return new string[] { view.Definition.RemoveComments().Replace("\n", " ").Replace("\r", " ") };
        }
    }
}