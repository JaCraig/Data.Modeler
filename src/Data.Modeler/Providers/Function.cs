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

namespace Data.Modeler.Providers
{
    /// <summary>
    /// Function class
    /// </summary>
    public class Function : IFunction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="schema">The schema.</param>
        /// <param name="definition">Definition</param>
        /// <param name="source">Source</param>
        public Function(string name, string schema, string definition, ISource source)
        {
            Schema = schema;
            Name = name;
            Definition = definition;
            Source = source;
        }

        /// <summary>
        /// Definition of the Function
        /// </summary>
        public string Definition { get; set; }

        /// <summary>
        /// Name of the Function
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the name of the schema.
        /// </summary>
        /// <value>The name of the schema.</value>
        public string Schema { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISource Source { get; set; }

        /// <summary>
        /// Copies the specified instance
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The copy</returns>
        public IFunction Copy(ISource source) => new Function(Name, Schema, Definition, source);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is Function Item)
                && Definition == Item.Definition
                && Name == Item.Name;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.
        /// </returns>
        public override int GetHashCode() => Name.GetHashCode(StringComparison.InvariantCulture);
    }
}