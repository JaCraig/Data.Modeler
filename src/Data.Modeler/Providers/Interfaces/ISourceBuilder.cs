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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Modeler.Providers.Interfaces
{
    /// <summary>
    /// Builder interface Helps with simplifying schema building
    /// </summary>
    public interface ISourceBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        int Order { get; }

        /// <summary>
        /// Fills the source.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="dataSource">The data source.</param>
        void FillSource(IEnumerable<dynamic> values, ISource dataSource);

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <param name="batch">The batch.</param>
        void GetCommand(SQLHelper.SQLHelper batch);
    }
}