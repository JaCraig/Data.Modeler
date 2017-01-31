using Data.Modeler.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
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
        public int Order => 31;

        /// <summary>
        /// Fills the database.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="database">The database.</param>
        public void FillSource(IEnumerable<dynamic> values, ISource database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (values == null || values.Count() == 0)
                return;
            foreach (dynamic Item in values)
            {
                SetupConstraint(database.Tables.FirstOrDefault(x => x.Name == Item.Table), Item);
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <param name="batch">The batch.</param>
        public void GetCommand(SQLHelper.SQLHelper batch)
        {
            if (batch == null)
                throw new ArgumentNullException(nameof(batch));
            batch.AddQuery(CommandType.Text, @"SELECT sys.tables.name as [Table],sys.check_constraints.name as [Name],OBJECT_DEFINITION(sys.check_constraints.object_id) as [Definition]
FROM sys.check_constraints
INNER JOIN sys.tables ON sys.tables.object_id=sys.check_constraints.parent_object_id");
        }

        private void SetupConstraint(ITable table, dynamic item)
        {
            var FinalDefinition = ((string)item.Definition).Remove(0, 1);
            table.AddCheckConstraint(item.Name, FinalDefinition.Remove(FinalDefinition.Length - 2));
        }
    }
}