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

namespace Data.Modeler.Providers
{
    /// <summary>
    /// CheckConstraint class
    /// </summary>
    public class CheckConstraint : ICheckConstraint
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="definition">Definition</param>
        /// <param name="parentTable">Parent table</param>
        public CheckConstraint(string name, string definition, ITable parentTable)
        {
            Name = name;
            Definition = definition;
            ParentTable = parentTable;
        }

        /// <summary>
        /// Definition of the CheckConstraint
        /// </summary>
        public string Definition { get; set; }

        /// <summary>
        /// Name of the CheckConstraint
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parent table
        /// </summary>
        public ITable ParentTable { get; set; }
    }
}