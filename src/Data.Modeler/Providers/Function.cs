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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// <param name="definition">Definition</param>
        /// <param name="source">Source</param>
        public Function(string name, string definition, ISource source)
        {
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
        /// Source
        /// </summary>
        public ISource Source { get; set; }
    }
}