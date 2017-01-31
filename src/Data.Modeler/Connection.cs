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
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Data.Modeler
{
    /// <summary>
    /// Connection object
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.IConnection"/>
    public class Connection : IConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="connection">Name of the database.</param>
        public Connection(IConfiguration configuration, DbProviderFactory factory, string connection)
        {
            Configuration = configuration;
            Factory = factory;

            var TempConfig = configuration.GetConnectionString(connection);
            if (!string.IsNullOrEmpty(TempConfig))
            {
                ConnectionString = TempConfig;
            }
            else
            {
                ConnectionString = connection;
            }
            if (factory == SqlClientFactory.Instance)
            {
                DatabaseName = Regex.Match(ConnectionString, @"Initial Catalog=([^;]*)").Groups[1].Value;
            }
            else
            {
                DatabaseName = connection;
            }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Connection string
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>The name of the database.</value>
        public string DatabaseName { get; private set; }

        /// <summary>
        /// Gets the factory.
        /// </summary>
        /// <value>The factory.</value>
        public DbProviderFactory Factory { get; private set; }
    }
}