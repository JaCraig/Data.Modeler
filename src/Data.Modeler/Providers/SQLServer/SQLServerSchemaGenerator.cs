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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace Data.Modeler.Providers.SQLServer
{
    /// <summary>
    /// SQL Server schema generator
    /// </summary>
    /// <seealso cref="Data.Modeler.Providers.Interfaces.ISchemaGenerator"/>
    public class SQLServerSchemaGenerator : ISchemaGenerator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryBuilders">The query builders.</param>
        /// <param name="commandBuilders">The command builders.</param>
        public SQLServerSchemaGenerator(IEnumerable<ISourceBuilder> queryBuilders, IEnumerable<ICommandBuilder> commandBuilders)
        {
            CommandBuilders = commandBuilders.OrderBy(x => x.Order).ToArray();
            QueryBuilders = queryBuilders.OrderBy(x => x.Order).ToArray();
        }

        /// <summary>
        /// Provider name associated with the schema generator
        /// </summary>
        public DbProviderFactory Provider { get { return SqlClientFactory.Instance; } }

        /// <summary>
        /// Gets the command builders.
        /// </summary>
        /// <value>The command builders.</value>
        private ICommandBuilder[] CommandBuilders { get; set; }

        /// <summary>
        /// Gets or sets the query builders.
        /// </summary>
        /// <value>The query builders.</value>
        private ISourceBuilder[] QueryBuilders { get; set; }

        /// <summary>
        /// Checks if a constraint exists
        /// </summary>
        /// <param name="constraint">The constraint to check.</param>
        /// <param name="source">Source to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool ConstraintExists(string constraint, IConnection source)
        {
            return Exists("SELECT * FROM sys.check_constraints WHERE name=@0", constraint, source);
        }

        /// <summary>
        /// Generates a list of commands used to modify the source. If it does not exist prior, the
        /// commands will create the source from scratch. Otherwise the commands will only add new
        /// fields, tables, etc. It does not delete old fields.
        /// </summary>
        /// <param name="desiredStructure">Desired source structure</param>
        /// <param name="source">Source to use</param>
        /// <returns>List of commands generated</returns>
        public IEnumerable<string> GenerateSchema(ISource desiredStructure, ISource source)
        {
            var Commands = new List<string>();
            desiredStructure = desiredStructure ?? new Source("");
            foreach (var CommandBuilder in CommandBuilders)
            {
                Commands.Add(CommandBuilder.GetCommands(desiredStructure, source));
            }
            return Commands;
        }

        /// <summary>
        /// Gets the structure of a source
        /// </summary>
        /// <param name="source">Source to use</param>
        /// <returns>The source structure</returns>
        public ISource GetSourceStructure(IConnection source)
        {
            var DatabaseName = source.DatabaseName;
            var DatabaseSource = new Connection(source.Configuration, source.Factory, Regex.Replace(source.ConnectionString, "Initial Catalog=(.*?;)", ""));
            if (!SourceExists(DatabaseName, DatabaseSource))
                return null;
            var Temp = new Source(DatabaseName);
            var Batch = new SQLHelper.SQLHelper(DatabaseSource.Configuration, DatabaseSource.Factory, DatabaseSource.ConnectionString)
                                     .CreateBatch();
            QueryBuilders.ForEach(x => Batch.AddQuery(CommandType.Text, x.GetCommand()));
            var Results = Batch.Execute();
            QueryBuilders.For(0, QueryBuilders.Length - 1, (x, y) => x.FillSource(Results[y], Temp));
            return Temp;
        }

        /// <summary>
        /// Sets up the specified database schema
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="connection">The connection.</param>
        public void Setup(ISource source, IConnection connection)
        {
            var CurrentSource = GetSourceStructure(connection);
            var Commands = GenerateSchema(source, CurrentSource).ToArray();

            var DatabaseSource = new Connection(connection.Configuration, connection.Factory, Regex.Replace(connection.ConnectionString, "Initial Catalog=(.*?;)", ""));
            var Batch = new SQLHelper.SQLHelper(connection.Configuration, connection.Factory, connection.ConnectionString);
            for (int x = 0; x < Commands.Length; ++x)
            {
                if (Commands[x].ToUpperInvariant().Contains("CREATE DATABASE"))
                {
                    Batch.CreateBatch()
                         .AddQuery(CommandType.Text, Commands[x])
                         .Execute();
                }
                else if (Commands[x].ToUpperInvariant().Contains("CREATE TRIGGER")
                    || Commands[x].ToUpperInvariant().Contains("CREATE FUNCTION"))
                {
                    if (Batch.Count > 0)
                    {
                        Batch.Execute();
                        Batch.CreateBatch();
                    }
                    Batch.AddQuery(CommandType.Text, Commands[x]);
                    if (x < Commands.Length - 1)
                    {
                        Batch.Execute();
                        Batch.CreateBatch();
                    }
                }
                else
                {
                    Batch.AddQuery(CommandType.Text, Commands[x]);
                }
            }
            Batch.Execute();
        }

        /// <summary>
        /// Checks if a source exists
        /// </summary>
        /// <param name="source">Source to check</param>
        /// <param name="info">Source info to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool SourceExists(string source, IConnection info)
        {
            return Exists("SELECT * FROM Master.sys.Databases WHERE name=@0", source, info);
        }

        /// <summary>
        /// Checks if a stored procedure exists
        /// </summary>
        /// <param name="storedProcedure">Stored procedure to check</param>
        /// <param name="source">Source to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool StoredProcedureExists(string storedProcedure, IConnection source)
        {
            return Exists("SELECT * FROM sys.Procedures WHERE name=@0", storedProcedure, source);
        }

        /// <summary>
        /// Checks if a table exists
        /// </summary>
        /// <param name="table">Table to check</param>
        /// <param name="source">Source to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool TableExists(string table, IConnection source)
        {
            return Exists("SELECT * FROM sys.Tables WHERE name=@0", table, source);
        }

        /// <summary>
        /// Checks if a trigger exists
        /// </summary>
        /// <param name="trigger">Trigger to check</param>
        /// <param name="source">Source to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool TriggerExists(string trigger, IConnection source)
        {
            return Exists("SELECT * FROM sys.triggers WHERE name=@0", trigger, source);
        }

        /// <summary>
        /// Checks if a view exists
        /// </summary>
        /// <param name="view">View to check</param>
        /// <param name="source">Source to use</param>
        /// <returns>True if it exists, false otherwise</returns>
        public bool ViewExists(string view, IConnection source)
        {
            return Exists("SELECT * FROM sys.views WHERE name=@0", view, source);
        }

        //private static IEnumerable<string> BuildCommands(ISource DesiredStructure, ISource CurrentStructure)
        //{
        //    var Commands = new List<string>();
        //    DesiredStructure = DesiredStructure.Check(new Database(""));
        //    if (CurrentStructure == null)
        //        Commands.Add(string.Format(CultureInfo.CurrentCulture,
        //            "EXEC dbo.sp_executesql @statement = N'CREATE DATABASE {0}'",
        //            DesiredStructure.Name));
        //    CurrentStructure = CurrentStructure.Check(new Database(DesiredStructure.Name));
        //    foreach (Table Table in DesiredStructure.Tables)
        //    {
        //        ITable CurrentTable = CurrentStructure[Table.Name];
        //        Commands.Add((CurrentTable == null) ? GetTableCommand(Table) : GetAlterTableCommand(Table, CurrentTable));
        //    }
        //    foreach (Table Table in DesiredStructure.Tables)
        //    {
        //        ITable CurrentTable = CurrentStructure[Table.Name];
        //        Commands.Add((CurrentTable == null) ? GetForeignKeyCommand(Table) : GetForeignKeyCommand(Table, CurrentTable));
        //        Commands.Add((CurrentTable == null) ? GetTriggerCommand(Table) : GetAlterTriggerCommand(Table, CurrentTable));
        //    }
        //    foreach (Function Function in DesiredStructure.Functions)
        //    {
        //        var CurrentFunction = (Function)CurrentStructure.Functions.FirstOrDefault(x => x.Name == Function.Name);
        //        Commands.Add(CurrentFunction != null ? GetAlterFunctionCommand(Function, CurrentFunction) : GetFunctionCommand(Function));
        //    }
        //    foreach (View View in DesiredStructure.Views)
        //    {
        //        var CurrentView = (View)CurrentStructure.Views.FirstOrDefault(x => x.Name == View.Name);
        //        Commands.Add(CurrentView != null ? GetAlterViewCommand(View, CurrentView) : GetViewCommand(View));
        //    }
        //    foreach (StoredProcedure StoredProcedure in DesiredStructure.StoredProcedures)
        //    {
        //        var CurrentStoredProcedure = (StoredProcedure)CurrentStructure.StoredProcedures.FirstOrDefault(x => x.Name == StoredProcedure.Name);
        //        Commands.Add(CurrentStoredProcedure != null ? GetAlterStoredProcedure(StoredProcedure, CurrentStoredProcedure) : GetStoredProcedure(StoredProcedure));
        //    }
        //    return Commands;
        //}

        //private static IEnumerable<string> GetAlterFunctionCommand(Function Function, Function CurrentFunction)
        //{
        //    Contract.Requires<ArgumentNullException>(Function != null, "Function");
        //    Contract.Requires<ArgumentNullException>(CurrentFunction != null, "CurrentFunction");
        //    Contract.Requires<ArgumentException>(Function.Definition == CurrentFunction.Definition || !string.IsNullOrEmpty(Function.Definition));
        //    var ReturnValue = new List<string>();
        //    if (Function.Definition != CurrentFunction.Definition)
        //    {
        //        ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
        //            "EXEC dbo.sp_executesql @statement = N'DROP FUNCTION {0}'",
        //            Function.Name));
        //        ReturnValue.Add(GetFunctionCommand(Function));
        //    }
        //    return ReturnValue;
        //}

        //private static IEnumerable<string> GetAlterStoredProcedure(StoredProcedure StoredProcedure, StoredProcedure CurrentStoredProcedure)
        //{
        //    Contract.Requires<ArgumentNullException>(StoredProcedure != null, "StoredProcedure");
        //    Contract.Requires<ArgumentNullException>(CurrentStoredProcedure != null, "CurrentStoredProcedure");
        //    Contract.Requires<ArgumentException>(StoredProcedure.Definition == CurrentStoredProcedure.Definition || !string.IsNullOrEmpty(StoredProcedure.Definition));
        //    var ReturnValue = new List<string>();
        //    if (StoredProcedure.Definition != CurrentStoredProcedure.Definition)
        //    {
        //        ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
        //            "EXEC dbo.sp_executesql @statement = N'DROP PROCEDURE {0}'",
        //            StoredProcedure.Name));
        //        ReturnValue.Add(GetStoredProcedure(StoredProcedure));
        //    }
        //    return ReturnValue;
        //}

        //private static IEnumerable<string> GetAlterTableCommand(Table Table, ITable CurrentTable)
        //{
        //    Contract.Requires<ArgumentNullException>(Table != null, "Table");
        //    Contract.Requires<ArgumentNullException>(Table.Columns != null, "Table.Columns");
        //    var ReturnValue = new List<string>();
        //    foreach (IColumn Column in Table.Columns)
        //    {
        //        IColumn CurrentColumn = CurrentTable[Column.Name];
        //        string Command = "";
        //        if (CurrentColumn == null)
        //        {
        //            Command = string.Format(CultureInfo.CurrentCulture,
        //                "EXEC dbo.sp_executesql @statement = N'ALTER TABLE {0} ADD {1} {2}",
        //                Table.Name,
        //                Column.Name,
        //                Column.DataType.To(SqlDbType.Int).ToString());
        //            if (Column.DataType == SqlDbType.VarChar.To(DbType.Int32)
        //                || Column.DataType == SqlDbType.NVarChar.To(DbType.Int32)
        //                || Column.DataType == SqlDbType.Binary.To(DbType.Int32))
        //            {
        //                Command += (Column.Length < 0 || Column.Length >= 4000) ?
        //                                "(MAX)" :
        //                                "(" + Column.Length.ToString(CultureInfo.InvariantCulture) + ")";
        //            }
        //            else if (Column.DataType == SqlDbType.Decimal.To(DbType.Int32))
        //            {
        //                var Precision = (Column.Length * 2).Clamp(38, 18);
        //                Command += "(" + Precision.ToString(CultureInfo.InvariantCulture) + "," + Column.Length.Clamp(38, 0).ToString(CultureInfo.InvariantCulture) + ")";
        //            }
        //            Command += "'";
        //            ReturnValue.Add(Command);
        //            foreach (IColumn ForeignKey in Column.ForeignKey)
        //            {
        //                Command = string.Format(CultureInfo.CurrentCulture,
        //                    "EXEC dbo.sp_executesql @statement = N'ALTER TABLE {0} ADD FOREIGN KEY ({1}) REFERENCES {2}({3}){4}{5}{6}'",
        //                    Table.Name,
        //                    Column.Name,
        //                    ForeignKey.ParentTable.Name,
        //                    ForeignKey.Name,
        //                    Column.OnDeleteCascade ? " ON DELETE CASCADE" : "",
        //                    Column.OnUpdateCascade ? " ON UPDATE CASCADE" : "",
        //                    Column.OnDeleteSetNull ? " ON DELETE SET NULL" : "");
        //                ReturnValue.Add(Command);
        //            }
        //        }
        //        else if (CurrentColumn.DataType != Column.DataType
        //            || (CurrentColumn.DataType == Column.DataType
        //                && CurrentColumn.DataType == SqlDbType.NVarChar.To(DbType.Int32)
        //                && CurrentColumn.Length != Column.Length
        //                && CurrentColumn.Length.Between(0, 4000)
        //                && Column.Length.Between(0, 4000)))
        //        {
        //            Command = string.Format(CultureInfo.CurrentCulture,
        //                "EXEC dbo.sp_executesql @statement = N'ALTER TABLE {0} ALTER COLUMN {1} {2}",
        //                Table.Name,
        //                Column.Name,
        //                Column.DataType.To(SqlDbType.Int).ToString());
        //            if (Column.DataType == SqlDbType.VarChar.To(DbType.Int32)
        //                || Column.DataType == SqlDbType.NVarChar.To(DbType.Int32)
        //                || Column.DataType == SqlDbType.Binary.To(DbType.Int32))
        //            {
        //                Command += (Column.Length < 0 || Column.Length >= 4000) ?
        //                                "(MAX)" :
        //                                "(" + Column.Length.ToString(CultureInfo.InvariantCulture) + ")";
        //            }
        //            else if (Column.DataType == SqlDbType.Decimal.To(DbType.Int32))
        //            {
        //                var Precision = (Column.Length * 2).Clamp(38, 18);
        //                Command += "(" + Precision.ToString(CultureInfo.InvariantCulture) + "," + Column.Length.Clamp(38, 0).ToString(CultureInfo.InvariantCulture) + ")";
        //            }
        //            Command += "'";
        //            ReturnValue.Add(Command);
        //        }
        //    }
        //    return ReturnValue;
        //}

        //private static IEnumerable<string> GetAlterTriggerCommand(Table Table, ITable CurrentTable)
        //{
        //    Contract.Requires<ArgumentNullException>(Table != null, "Table");
        //    Contract.Requires<ArgumentNullException>(Table.Triggers != null, "Table.Triggers");
        //    var ReturnValue = new List<string>();
        //    foreach (Trigger Trigger in Table.Triggers)
        //    {
        //        foreach (Trigger Trigger2 in CurrentTable.Triggers)
        //        {
        //            string Definition1 = Trigger.Definition;
        //            var Definition2 = Trigger2.Definition.Replace("Command0", "");
        //            if (Trigger.Name == Trigger2.Name && string.Equals(Definition1, Definition2, StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
        //                    "EXEC dbo.sp_executesql @statement = N'DROP TRIGGER {0}'",
        //                    Trigger.Name));
        //                var Definition = Regex.Replace(Trigger.Definition, "-- (.*)", "");
        //                ReturnValue.Add(Definition.Replace("\n", " ").Replace("\r", " "));
        //                break;
        //            }
        //        }
        //    }
        //    return ReturnValue;
        //}

        //private static IEnumerable<string> GetAlterViewCommand(View View, View CurrentView)
        //{
        //    Contract.Requires<ArgumentNullException>(View != null, "View");
        //    Contract.Requires<ArgumentNullException>(CurrentView != null, "CurrentView");
        //    Contract.Requires<ArgumentException>(View.Definition == CurrentView.Definition || !string.IsNullOrEmpty(View.Definition));
        //    var ReturnValue = new List<string>();
        //    if (View.Definition != CurrentView.Definition)
        //    {
        //        ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
        //            "EXEC dbo.sp_executesql @statement = N'DROP VIEW {0}'",
        //            View.Name));
        //        ReturnValue.Add(GetViewCommand(View));
        //    }
        //    return ReturnValue;
        //}

        //private static IEnumerable<string> GetForeignKeyCommand(Table Table, ITable CurrentTable)
        //{
        //    Contract.Requires<ArgumentNullException>(Table != null, "Table");
        //    Contract.Requires<ArgumentNullException>(Table.Columns != null, "Table.Columns");
        //    var ReturnValue = new List<string>();
        //    foreach (IColumn Column in Table.Columns)
        //    {
        //        IColumn CurrentColumn = CurrentTable[Column.Name];
        //        if (Column.ForeignKey.Count > 0
        //            && (CurrentColumn == null || CurrentColumn.ForeignKey.Count != Column.ForeignKey.Count))
        //        {
        //            foreach (IColumn ForeignKey in Column.ForeignKey)
        //            {
        //                var Command = string.Format(CultureInfo.CurrentCulture,
        //                    "EXEC dbo.sp_executesql @statement = N'ALTER TABLE {0} ADD FOREIGN KEY ({1}) REFERENCES {2}({3})",
        //                    Column.ParentTable.Name,
        //                    Column.Name,
        //                    ForeignKey.ParentTable.Name,
        //                    ForeignKey.Name);
        //                if (Column.OnDeleteCascade)
        //                    Command += " ON DELETE CASCADE";
        //                if (Column.OnUpdateCascade)
        //                    Command += " ON UPDATE CASCADE";
        //                if (Column.OnDeleteSetNull)
        //                    Command += " ON DELETE SET NULL";
        //                Command += "'";
        //                ReturnValue.Add(Command);
        //            }
        //        }
        //    }
        //    return ReturnValue;
        //}

        //private static IEnumerable<string> GetForeignKeyCommand(Table Table)
        //{
        //    Contract.Requires<ArgumentNullException>(Table != null, "Table");
        //    Contract.Requires<ArgumentNullException>(Table.Columns != null, "Table.Columns");
        //    var ReturnValue = new List<string>();
        //    foreach (IColumn Column in Table.Columns)
        //    {
        //        if (Column.ForeignKey.Count > 0)
        //        {
        //            foreach (IColumn ForeignKey in Column.ForeignKey)
        //            {
        //                var Command = string.Format(CultureInfo.CurrentCulture,
        //                    "EXEC dbo.sp_executesql @statement = N'ALTER TABLE {0} ADD FOREIGN KEY ({1}) REFERENCES {2}({3})",
        //                    Column.ParentTable.Name,
        //                    Column.Name,
        //                    ForeignKey.ParentTable.Name,
        //                    ForeignKey.Name);
        //                if (Column.OnDeleteCascade)
        //                    Command += " ON DELETE CASCADE";
        //                if (Column.OnUpdateCascade)
        //                    Command += " ON UPDATE CASCADE";
        //                if (Column.OnDeleteSetNull)
        //                    Command += " ON DELETE SET NULL";
        //                Command += "'";
        //                ReturnValue.Add(Command);
        //            }
        //        }
        //    }
        //    return ReturnValue;
        //}

        //private static IEnumerable<string> GetFunctionCommand(Function Function)
        //{
        //    Contract.Requires<ArgumentNullException>(Function != null, "Function");
        //    Contract.Requires<ArgumentNullException>(Function.Definition != null, "Function.Definition");
        //    var Definition = Regex.Replace(Function.Definition, "-- (.*)", "");
        //    return new string[] { Definition.Replace("\n", " ").Replace("\r", " ") };
        //}

        //private static IEnumerable<string> GetStoredProcedure(StoredProcedure StoredProcedure)
        //{
        //    Contract.Requires<ArgumentNullException>(StoredProcedure != null, "StoredProcedure");
        //    Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(StoredProcedure.Definition), "StoredProcedure.Definition");
        //    var Definition = Regex.Replace(StoredProcedure.Definition, "-- (.*)", "");
        //    return new string[] { Definition.Replace("\n", " ").Replace("\r", " ") };
        //}

        //private static IEnumerable<string> GetTableCommand(Table Table)
        //{
        //    Contract.Requires<ArgumentNullException>(Table != null, "Table");
        //    Contract.Requires<ArgumentNullException>(Table.Columns != null, "Table.Columns");
        //    var ReturnValue = new List<string>();
        //    var Builder = new StringBuilder();
        //    Builder.Append("EXEC dbo.sp_executesql @statement = N'CREATE TABLE ").Append(Table.Name).Append("(");
        //    string Splitter = "";
        //    foreach (IColumn Column in Table.Columns)
        //    {
        //        Builder.Append(Splitter).Append(Column.Name).Append(" ").Append(Column.DataType.To(SqlDbType.Int).ToString());
        //        if (Column.DataType == SqlDbType.VarChar.To(DbType.Int32)
        //                || Column.DataType == SqlDbType.NVarChar.To(DbType.Int32)
        //                || Column.DataType == SqlDbType.Binary.To(DbType.Int32))
        //        {
        //            Builder.Append((Column.Length < 0 || Column.Length >= 4000) ?
        //                            "(MAX)" :
        //                            "(" + Column.Length.ToString(CultureInfo.InvariantCulture) + ")");
        //        }
        //        else if (Column.DataType == SqlDbType.Decimal.To(DbType.Int32))
        //        {
        //            var Precision = (Column.Length * 2).Clamp(38, 18);
        //            Builder.Append("(").Append(Precision).Append(",").Append(Column.Length.Clamp(38, 0)).Append(")");
        //        }
        //        if (!Column.Nullable)
        //        {
        //            Builder.Append(" NOT NULL");
        //        }
        //        if (Column.Unique)
        //        {
        //            Builder.Append(" UNIQUE");
        //        }
        //        if (Column.PrimaryKey)
        //        {
        //            Builder.Append(" PRIMARY KEY");
        //        }
        //        if (!string.IsNullOrEmpty(Column.Default))
        //        {
        //            Builder.Append(" DEFAULT ").Append(Column.Default.Replace("(", "").Replace(")", "").Replace("'", "''"));
        //        }
        //        if (Column.AutoIncrement)
        //        {
        //            Builder.Append(" IDENTITY");
        //        }
        //        Splitter = ",";
        //    }
        //    Builder.Append(")'");
        //    ReturnValue.Add(Builder.ToString());
        //    int Counter = 0;
        //    foreach (IColumn Column in Table.Columns)
        //    {
        //        if (Column.Index && Column.Unique)
        //        {
        //            ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
        //                "EXEC dbo.sp_executesql @statement = N'CREATE UNIQUE INDEX Index_{0}{1} ON {2}({3})'",
        //                Column.Name,
        //                Counter.ToString(CultureInfo.InvariantCulture),
        //                Column.ParentTable.Name,
        //                Column.Name));
        //        }
        //        else if (Column.Index)
        //        {
        //            ReturnValue.Add(string.Format(CultureInfo.CurrentCulture,
        //                "EXEC dbo.sp_executesql @statement = N'CREATE INDEX Index_{0}{1} ON {2}({3})'",
        //                Column.Name,
        //                Counter.ToString(CultureInfo.InvariantCulture),
        //                Column.ParentTable.Name,
        //                Column.Name));
        //        }
        //        ++Counter;
        //    }
        //    return ReturnValue;
        //}

        //private static IEnumerable<string> GetTriggerCommand(Table Table)
        //{
        //    Contract.Requires<ArgumentNullException>(Table != null, "Table");
        //    Contract.Requires<ArgumentNullException>(Table.Triggers != null, "Table.Triggers");
        //    var ReturnValue = new List<string>();
        //    foreach (Trigger Trigger in Table.Triggers)
        //    {
        //        var Definition = Regex.Replace(Trigger.Definition, "-- (.*)", "");
        //        ReturnValue.Add(Definition.Replace("\n", " ").Replace("\r", " "));
        //    }
        //    return ReturnValue;
        //}

        //private static IEnumerable<string> GetViewCommand(View View)
        //{
        //    Contract.Requires<ArgumentNullException>(View != null, "View");
        //    Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(View.Definition), "View.Definition");
        //    var Definition = Regex.Replace(View.Definition, "-- (.*)", "");
        //    return new string[] { Definition.Replace("\n", " ").Replace("\r", " ") };
        //}

        //private static ITable SetupAuditTables(ITable Table)
        //{
        //    Contract.Requires<ArgumentNullException>(Table != null, "Table");
        //    var AuditTable = new Schema.Default.Database.Table(Table.Name + "Audit", Table.Source);
        //    string IDName = Table.Columns.Any(x => string.Equals(x.Name, "ID", StringComparison.InvariantCultureIgnoreCase)) ? "AuditID" : "ID";
        //    AuditTable.AddColumn(IDName, DbType.Int32, 0, false, true, true, true, false, "", "", 0);
        //    AuditTable.AddColumn("AuditType", SqlDbType.NVarChar.To(DbType.Int32), 1, false, false, false, false, false, "", "", "");
        //    foreach (IColumn Column in Table.Columns)
        //        AuditTable.AddColumn(Column.Name, Column.DataType, Column.Length, Column.Nullable, false, false, false, false, "", "", "");
        //    return AuditTable;
        //}

        //private static void SetupAuditTables(IDatabase Key, Schema.Default.Database.Database TempDatabase)
        //{
        //    Contract.Requires<ArgumentNullException>(Key != null, "Key");
        //    Contract.Requires<ArgumentNullException>(TempDatabase != null, "TempDatabase");
        //    Contract.Requires<ArgumentNullException>(TempDatabase.Tables != null, "TempDatabase.Tables");
        //    if (!Key.Audit)
        //        return;
        //    var TempTables = new List<ITable>();
        //    foreach (ITable Table in TempDatabase.Tables)
        //    {
        //        TempTables.Add(SetupAuditTables(Table));
        //        SetupInsertUpdateTrigger(Table);
        //        SetupDeleteTrigger(Table);
        //    }
        //    TempDatabase.Tables.Add(TempTables);
        //}

        //private static void SetupDeleteTrigger(ITable Table)
        //{
        //    Contract.Requires<ArgumentNullException>(Table != null, "Table");
        //    Contract.Requires<ArgumentNullException>(Table.Columns != null, "Table.Columns");
        //    var Columns = new StringBuilder();
        //    var Builder = new StringBuilder();
        //    Builder.Append("CREATE TRIGGER dbo.").Append(Table.Name).Append("_Audit_D ON dbo.")
        //        .Append(Table.Name).Append(" FOR DELETE AS IF @@rowcount=0 RETURN")
        //        .Append(" INSERT INTO dbo.").Append(Table.Name).Append("Audit").Append("(");
        //    string Splitter = "";
        //    foreach (IColumn Column in Table.Columns)
        //    {
        //        Columns.Append(Splitter).Append(Column.Name);
        //        Splitter = ",";
        //    }
        //    Builder.Append(Columns.ToString());
        //    Builder.Append(",AuditType) SELECT ");
        //    Builder.Append(Columns.ToString());
        //    Builder.Append(",'D' FROM deleted");
        //    Table.AddTrigger(Table.Name + "_Audit_D", Builder.ToString(), TriggerType.Delete);
        //}

        //private static void SetupInsertUpdateTrigger(ITable Table)
        //{
        //    Contract.Requires<ArgumentNullException>(Table != null, "Table");
        //    Contract.Requires<ArgumentNullException>(Table.Columns != null, "Table.Columns");
        //    var Columns = new StringBuilder();
        //    var Builder = new StringBuilder();
        //    Builder.Append("CREATE TRIGGER dbo.").Append(Table.Name).Append("_Audit_IU ON dbo.")
        //        .Append(Table.Name).Append(" FOR INSERT,UPDATE AS IF @@rowcount=0 RETURN declare @AuditType")
        //        .Append(" char(1) declare @DeletedCount int SELECT @DeletedCount=count(*) FROM DELETED IF @DeletedCount=0")
        //        .Append(" BEGIN SET @AuditType='I' END ELSE BEGIN SET @AuditType='U' END")
        //        .Append(" INSERT INTO dbo.").Append(Table.Name).Append("Audit").Append("(");
        //    string Splitter = "";
        //    foreach (IColumn Column in Table.Columns)
        //    {
        //        Columns.Append(Splitter).Append(Column.Name);
        //        Splitter = ",";
        //    }
        //    Builder.Append(Columns.ToString());
        //    Builder.Append(",AuditType) SELECT ");
        //    Builder.Append(Columns.ToString());
        //    Builder.Append(",@AuditType FROM inserted");
        //    Table.AddTrigger(Table.Name + "_Audit_IU", Builder.ToString(), TriggerType.Insert);
        //}

        //private static void SetupJoiningTables(ListMapping<IDatabase, IMapping> Mappings, IDatabase Key, Schema.Default.Database.Database TempDatabase)
        //{
        //    Contract.Requires<NullReferenceException>(Mappings != null, "Mappings");
        //    foreach (IMapping Mapping in Mappings[Key].OrderBy(x => x.Order))
        //    {
        //        foreach (IProperty Property in Mapping.Properties)
        //        {
        //            if (Property is IMap)
        //            {
        //                var MapMapping = Mappings[Key].FirstOrDefault(x => x.ObjectType == Property.Type);
        //                foreach (IProperty IDProperty in MapMapping.IDProperties)
        //                {
        //                    TempDatabase[Mapping.TableName].AddColumn(Property.FieldName,
        //                        IDProperty.Type.To(DbType.Int32),
        //                        IDProperty.MaxLength,
        //                        !Property.NotNull,
        //                        false,
        //                        Property.Index,
        //                        false,
        //                        false,
        //                        MapMapping.TableName,
        //                        IDProperty.FieldName,
        //                        "",
        //                        false,
        //                        false,
        //                        Mapping.Properties.Count(x => x.Type == Property.Type) == 1 && Mapping.ObjectType != Property.Type);
        //                }
        //            }
        //            else if (Property is IMultiMapping || Property is ISingleMapping)
        //            {
        //                SetupJoiningTablesEnumerable(Mappings, Mapping, Property, Key, TempDatabase);
        //            }
        //        }
        //    }
        //}

        //private static void SetupJoiningTablesEnumerable(ListMapping<IDatabase, IMapping> Mappings, IMapping Mapping, IProperty Property, IDatabase Key, Schema.Default.Database.Database TempDatabase)
        //{
        //    Contract.Requires<ArgumentNullException>(TempDatabase != null, "TempDatabase");
        //    Contract.Requires<ArgumentNullException>(TempDatabase.Tables != null, "TempDatabase.Tables");
        //    if (TempDatabase.Tables.FirstOrDefault(x => x.Name == Property.TableName) != null)
        //        return;
        //    var MapMapping = Mappings[Key].FirstOrDefault(x => x.ObjectType == Property.Type);
        //    if (MapMapping == null)
        //        return;
        //    if (MapMapping == Mapping)
        //    {
        //        TempDatabase.AddTable(Property.TableName);
        //        TempDatabase[Property.TableName].AddColumn("ID_", DbType.Int32, 0, false, true, true, true, false, "", "", "");
        //        TempDatabase[Property.TableName].AddColumn(Mapping.TableName + Mapping.IDProperties.First().FieldName,
        //            Mapping.IDProperties.First().Type.To(DbType.Int32),
        //            Mapping.IDProperties.First().MaxLength,
        //            false,
        //            false,
        //            false,
        //            false,
        //            false,
        //            Mapping.TableName,
        //            Mapping.IDProperties.First().FieldName,
        //            "",
        //            false,
        //            false,
        //            false);
        //        TempDatabase[Property.TableName].AddColumn(MapMapping.TableName + MapMapping.IDProperties.First().FieldName + "2",
        //            MapMapping.IDProperties.First().Type.To(DbType.Int32),
        //            MapMapping.IDProperties.First().MaxLength,
        //            false,
        //            false,
        //            false,
        //            false,
        //            false,
        //            MapMapping.TableName,
        //            MapMapping.IDProperties.First().FieldName,
        //            "",
        //            false,
        //            false,
        //            false);
        //    }
        //    else
        //    {
        //        TempDatabase.AddTable(Property.TableName);
        //        TempDatabase[Property.TableName].AddColumn("ID_", DbType.Int32, 0, false, true, true, true, false, "", "", "");
        //        TempDatabase[Property.TableName].AddColumn(Mapping.TableName + Mapping.IDProperties.First().FieldName,
        //            Mapping.IDProperties.First().Type.To(DbType.Int32),
        //            Mapping.IDProperties.First().MaxLength,
        //            false,
        //            false,
        //            false,
        //            false,
        //            false,
        //            Mapping.TableName,
        //            Mapping.IDProperties.First().FieldName,
        //            "",
        //            true,
        //            false,
        //            false);
        //        TempDatabase[Property.TableName].AddColumn(MapMapping.TableName + MapMapping.IDProperties.First().FieldName,
        //            MapMapping.IDProperties.First().Type.To(DbType.Int32),
        //            MapMapping.IDProperties.First().MaxLength,
        //            false,
        //            false,
        //            false,
        //            false,
        //            false,
        //            MapMapping.TableName,
        //            MapMapping.IDProperties.First().FieldName,
        //            "",
        //            true,
        //            false,
        //            false);
        //    }
        //}

        //private static void SetupProperties(ITable Table, IMapping Mapping)
        //{
        //    Contract.Requires<ArgumentNullException>(Mapping != null, "Mapping");
        //    Contract.Requires<ArgumentNullException>(Table != null, "Table");
        //    Contract.Requires<ArgumentNullException>(Mapping.IDProperties != null, "Mapping.IDProperties");
        //    Mapping.IDProperties
        //           .ForEach(x =>
        //           {
        //               Table.AddColumn(x.FieldName,
        //                   x.Type.To(DbType.Int32),
        //                   x.MaxLength,
        //                   x.NotNull,
        //                   x.AutoIncrement,
        //                   x.Index,
        //                   true,
        //                   x.Unique,
        //                   "",
        //                   "",
        //                   "");
        //           });
        //    Mapping.Properties
        //           .Where(x => !(x is IMultiMapping || x is ISingleMapping || x is IMap))
        //           .ForEach(x =>
        //           {
        //               Table.AddColumn(x.FieldName,
        //               x.Type.To(DbType.Int32),
        //               x.MaxLength,
        //               !x.NotNull,
        //               x.AutoIncrement,
        //               x.Index,
        //               false,
        //               x.Unique,
        //               "",
        //               "",
        //               "");
        //           });
        //}

        //private static void SetupTables(ListMapping<IDatabase, IMapping> Mappings, IDatabase Key, Database TempDatabase, Graph<IMapping> Structure)
        //{
        //    Contract.Requires<NullReferenceException>(Mappings != null, "Mappings");
        //    foreach (IMapping Mapping in Mappings[Key].OrderBy(x => x.Order))
        //    {
        //        TempDatabase.AddTable(Mapping.TableName);
        //        SetupProperties(TempDatabase[Mapping.TableName], Mapping);
        //    }
        //    foreach (Vertex<IMapping> Vertex in Structure.Where(x => x.OutgoingEdges.Count > 0))
        //    {
        //        var Mapping = Vertex.Data;
        //        var ForeignMappings = Vertex.OutgoingEdges.Select(x => x.Sink.Data);
        //        foreach (var Property in Mapping.IDProperties)
        //        {
        //            foreach (var ForeignMapping in ForeignMappings)
        //            {
        //                var ForeignProperty = ForeignMapping.IDProperties.FirstOrDefault(x => x.Name == Property.Name);
        //                if (ForeignProperty != null)
        //                {
        //                    TempDatabase[Mapping.TableName].AddForeignKey(Property.FieldName, ForeignMapping.TableName, ForeignProperty.FieldName);
        //                }
        //            }
        //        }
        //    }
        //}

        private bool Exists(string command, string value, IConnection source)
        {
            if (source == null || value == null || command == null)
                return false;
            return new SQLHelper.SQLHelper(source.Configuration, source.Factory, source.ConnectionString)
                           .AddQuery(CommandType.Text, command, value)
                           .Execute()[0]
                           .Count() > 0;
        }
    }
}