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
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Stored procedure command builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ICommandBuilder"/>
    public class StoredProcedureCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoredProcedureCommandBuilder"/> class.
        /// </summary>
        /// <param name="objectPool">The object pool.</param>
        public StoredProcedureCommandBuilder(ObjectPool<StringBuilder> objectPool)
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
        public int Order { get; } = 60;

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
        public string[] GetCommands(ISource desiredStructure, ISource? currentStructure)
        {
            if (desiredStructure is null)
                return Array.Empty<string>();
            currentStructure ??= new Source(desiredStructure.Name);
            var Commands = new List<string>();
            var Builder = ObjectPool.Get();
            for (int i = 0, desiredStructureStoredProceduresCount = desiredStructure.StoredProcedures.Count; i < desiredStructureStoredProceduresCount; i++)
            {
                var TempStoredProcedure = desiredStructure.StoredProcedures[i];
                var CurrentStoredProcedure = currentStructure.StoredProcedures.Find(x => x.Name == TempStoredProcedure.Name);
                Commands.Add(CurrentStoredProcedure != null ? GetAlterStoredProcedure(TempStoredProcedure, CurrentStoredProcedure, Builder) : GetStoredProcedure(TempStoredProcedure));
            }
            ObjectPool.Return(Builder);

            return Commands.ToArray();
        }

        /// <summary>
        /// Gets the alter stored procedure.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <param name="currentStoredProcedure">The current stored procedure.</param>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetAlterStoredProcedure(IFunction storedProcedure, IFunction currentStoredProcedure, StringBuilder builder)
        {
            if (storedProcedure is null
                || currentStoredProcedure is null
                || (storedProcedure.Definition != currentStoredProcedure.Definition && string.IsNullOrEmpty(storedProcedure.Definition))
                || storedProcedure.Definition == currentStoredProcedure.Definition)
            {
                return Array.Empty<string>();
            }

            var Result = new List<string>{
                 builder.Append("DROP PROCEDURE [")
                 .Append(storedProcedure.Schema)
                 .Append("].[")
                 .Append(storedProcedure.Name)
                 .Append("]")
                 .ToString(),
                GetStoredProcedure(storedProcedure)
            };
            builder.Clear();
            return Result;
        }

        /// <summary>
        /// Gets the stored procedure.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <returns></returns>
        private static string[] GetStoredProcedure(IFunction storedProcedure)
        {
            if (storedProcedure is null || storedProcedure.Definition is null)
                return Array.Empty<string>();
            return new string[] {
                storedProcedure
                    .Definition
                    .RemoveComments()
                    .Replace("\n", " ",StringComparison.Ordinal)
                    .Replace("\r", " ", StringComparison.Ordinal)
            };
        }
    }
}