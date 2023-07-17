using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.SqlClient;

namespace Data.Modeler.Example
{
    /// <summary>
    /// This is an example of how to use the Data.Modeler library to generate SQL commands to create a database schema.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            // Create a service provider using the Canister configuration
            var Services = new ServiceCollection().AddCanisterModules()?.BuildServiceProvider();
            // Create a data modeler instance from the service provider
            var DataModeler = Services?.GetService<DataModeler>();

            // Create a schema provider for SQL Server using the SQL Client Factory
            var SchemaProvider = DataModeler.GetSchemaGenerator(SqlClientFactory.Instance);

            // And let's define the database schema that we want to generate
            var Source = DataModeler.CreateSource("MySource");

            // Add a table to the source with the specified name and schema
            var Table = Source.AddTable("TableName", "dbo");
            // Add a column to the table with the specified name and type
            var Column = Table.AddColumn<int>("ColumnName", DbType.Int32);
            // Check constraints can be added to a table along with keys and indexes
            var CheckConstraint = Table.AddCheckConstraint("CheckConstraintName", "Check Constraint Definition");
            // A view is treated similarly to a table
            var View = Source.AddView("ViewName", "View Creation Code", "dbo");
            // Functions can be added to the source
            var Function = Source.AddFunction("FunctionName", "Function Creation Code", "dbo");
            // We can also add a stored procedure to the source
            var StoredProcedure = Source.AddStoredProcedure("StoredProcedureName", "Stored Procedure Creation Code", "dbo");

            // Generate the SQL commands to create the database based on the source schema model
            var DatabaseGenerationCommands = SchemaProvider.GenerateSchema(Source, null);

            foreach (var Command in DatabaseGenerationCommands)
            {
                // We're writing the SQL command out but we could also execute it against a database
                Console.WriteLine(Command);
            }
        }
    }
}