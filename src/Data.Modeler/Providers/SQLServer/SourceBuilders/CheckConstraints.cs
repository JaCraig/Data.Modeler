using Data.Modeler.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Data.Modeler.Providers.SQLServer.SourceBuilders
{
    /// <summary>
    /// Check constraints source builder
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ISourceBuilder"/>
    public class CheckConstraints : ISourceBuilder
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; } = 31;

        /// <summary>
        /// Provider name associated with the schema generator
        /// </summary>
        public DbProviderFactory Provider { get; } = SqlClientFactory.Instance;

        /// <summary>
        /// Fills the database.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="dataSource">The database.</param>
        public void FillSource(IEnumerable<dynamic> values, ISource dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (values?.Any() != true)
                return;
            foreach (dynamic Item in values)
            {
                SetupConstraint(dataSource.Tables.Find(x => x.Name == Item.Table), Item);
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <returns>The command to get the source</returns>
        public string GetCommand()
        {
            return @"SELECT sys.tables.name as [Table],sys.check_constraints.name as [Name],OBJECT_DEFINITION(sys.check_constraints.object_id) as [Definition]
FROM sys.check_constraints
INNER JOIN sys.tables ON sys.tables.object_id=sys.check_constraints.parent_object_id";
        }

        private void SetupConstraint(ITable table, dynamic item)
        {
            var FinalDefinition = ((string)item.Definition).Remove(0, 1);
            table.AddCheckConstraint(item.Name, FinalDefinition.Remove(FinalDefinition.Length - 1));
        }
    }
}