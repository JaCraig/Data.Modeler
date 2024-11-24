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

using Canister.Interfaces;
using Data.Modeler.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Modeler.Registration
{
    /// <summary>
    /// Registration extension methods
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Registers the library with the bootstrapper.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The bootstrapper</returns>
        public static ICanisterConfiguration? RegisterDataModeler(this ICanisterConfiguration? bootstrapper)
        {
            return bootstrapper?.AddAssembly(typeof(RegistrationExtensions).Assembly)
                                .RegisterSQLHelper();
        }

        /// <summary>
        /// Registers the Data Modeler services with the specified service collection.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <returns>The service collection with the registered services.</returns>
        public static IServiceCollection? RegisterDataModeler(this IServiceCollection? services)
        {
            if (services.Exists<DataModeler>())
                return services;
            return services?.AddAllTransient<ICommandBuilder>()
                         ?.AddAllTransient<ISourceBuilder>()
                         ?.AddAllSingleton<ISchemaGenerator>()
                         ?.AddSingleton<DataModeler>()
                         ?.RegisterSQLHelper();
        }
    }
}