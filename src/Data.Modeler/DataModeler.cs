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

using Data.Modeler.Providers;
using Data.Modeler.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Data.Modeler
{
    /// <summary>
    /// Data modeler class.
    /// </summary>
    public class DataModeler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataModeler"/> class.
        /// </summary>
        /// <param name="generators">The generators.</param>
        public DataModeler(IEnumerable<ISchemaGenerator> generators)
        {
            generators ??= Array.Empty<ISchemaGenerator>();
            Generators = new Dictionary<DbProviderFactory, ISchemaGenerator>();
            foreach (var Item in generators.Where(x => x.GetType().Assembly != typeof(DataModeler).Assembly))
            {
                if (!Generators.ContainsKey(Item.Provider))
                    Generators.Add(Item.Provider, Item);
            }
            foreach (var Item in generators.Where(x => x.GetType().Assembly == typeof(DataModeler).Assembly))
            {
                if (!Generators.ContainsKey(Item.Provider))
                    Generators.Add(Item.Provider, Item);
            }
        }

        /// <summary>
        /// Gets or sets the generators.
        /// </summary>
        /// <value>The generators.</value>
        private Dictionary<DbProviderFactory, ISchemaGenerator> Generators { get; }

        /// <summary>
        /// Creates the source.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The source requested.</returns>
        public static ISource CreateSource(string name) => new Source(name);

        /// <summary>
        /// Gets the schema generator based on the DbProviderFactory sent in.
        /// </summary>
        /// <param name="factory">The DbProviderFactory.</param>
        /// <returns>The requested schema generator</returns>
        public ISchemaGenerator? GetSchemaGenerator(DbProviderFactory factory)
        {
            Generators.TryGetValue(factory, out var Result);
            return Result;
        }
    }
}