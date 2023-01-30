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
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Data.Common;
using System.Text;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Create database command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class CreateDatabaseCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateDatabaseCommandBuilder"/> class.
        /// </summary>
        /// <param name="objectPool">The object pool.</param>
        public CreateDatabaseCommandBuilder(ObjectPool<StringBuilder> objectPool)
        {
            ObjectPool = objectPool;
        }

        /// <summary>
        /// Gets the object pool.
        /// </summary>
        /// <value>The object pool.</value>
        public ObjectPool<StringBuilder> ObjectPool { get; }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; } = 1;

        /// <summary>
        /// Provider name associated with the schema generator
        /// </summary>
        public DbProviderFactory[] Providers { get; } = new DbProviderFactory[] { SqlClientFactory.Instance, System.Data.SqlClient.SqlClientFactory.Instance };

        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <param name="desiredStructure">The desired structure.</param>
        /// <param name="currentStructure">The current structure.</param>
        /// <returns>
        /// The list of commands needed to change the structure from the current to the desired structure
        /// </returns>
        public string[] GetCommands(ISource desiredStructure, ISource? currentStructure)
        {
            if (currentStructure != null || desiredStructure is null)
                return Array.Empty<string>();
            var Builder = ObjectPool.Get();
            var Result = new string[] {
                Builder.Append("CREATE DATABASE [")
                .Append(desiredStructure.Name)
                .Append(']')
                .ToString()
            };
            ObjectPool.Return(Builder);
            return Result;
        }
    }
}