﻿/*
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
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Data.Modeler.Providers.SQLServer.CommandBuilders
{
    /// <summary>
    /// Function command builder
    /// </summary>
    /// <seealso cref="ICommandBuilder"/>
    public class CreateFunctionCommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFunctionCommandBuilder"/> class.
        /// </summary>
        /// <param name="objectPool">The object pool.</param>
        public CreateFunctionCommandBuilder(ObjectPool<StringBuilder> objectPool)
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
        public int Order { get; } = 40;

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
            if (desiredStructure is null)
                return Array.Empty<string>();
            currentStructure ??= new Source(desiredStructure.Name);
            var Commands = new List<string>();
            var Builder = ObjectPool.Get();
            for (int i = 0, desiredStructureFunctionsCount = desiredStructure.Functions.Count; i < desiredStructureFunctionsCount; i++)
            {
                var TempFunction = desiredStructure.Functions[i];
                var CurrentFunction = currentStructure.Functions.Find(x => x.Name == TempFunction.Name);
                Commands.Add(CurrentFunction != null ? GetAlterFunctionCommand(TempFunction, CurrentFunction, Builder) : GetFunctionCommand(TempFunction));
            }
            ObjectPool.Return(Builder);

            return Commands.ToArray();
        }

        /// <summary>
        /// Gets the alter function command.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="currentFunction">The current function.</param>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetAlterFunctionCommand(IFunction function, IFunction currentFunction, StringBuilder builder)
        {
            if (function is null
                || currentFunction is null
                || (function.Definition != currentFunction.Definition && string.IsNullOrEmpty(function.Definition))
                || function.Definition == currentFunction.Definition)
            {
                return Array.Empty<string>();
            }

            var ReturnValue = new List<string>
            {
                builder.Append("DROP FUNCTION [")
                .Append(function.Schema)
                .Append("].[")
                .Append(function.Name)
                .Append(']')
                .ToString(),
                GetFunctionCommand(function)
            };
            builder.Clear();
            return ReturnValue;
        }

        /// <summary>
        /// Gets the function command.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns></returns>
        private static string[] GetFunctionCommand(IFunction function)
        {
            if (function is null || function.Definition is null)
                return Array.Empty<string>();
            return new string[] {
                function
                    .Definition
                    .RemoveComments()
                    .Replace("\n", " ", StringComparison.Ordinal)
                    .Replace("\r", " ", StringComparison.Ordinal)
            };
        }
    }
}