﻿using Data.Modeler.Providers;
using Data.Modeler.Providers.Enums;
using Data.Modeler.Providers.Interfaces;
using System.Data;

namespace Data.Modeler.Tests.BaseClasses
{
    public abstract class CommandBuilderTestBase : TestingFixture
    {
        protected CommandBuilderTestBase()
        {
            CurrentSource = new Source("My Data");
            CurrentSource.AddFunction("Function A", "My Definition");
            CurrentSource.AddStoredProcedure("Stored Procedure A", "My Definition");
            var Table = CurrentSource.AddTable("Table A");
            Table.AddCheckConstraint("Constraint A", "My Definition");
            Table.AddColumn<int>("Column A", DbType.Int32);
            Table.AddTrigger("Trigger A", "My Definition", TriggerType.Insert);
            //Table.AddForeignKey("Column A", "Foreign Table", "Foreign Column");
            CurrentSource.AddView("View A", "My Definition");
            var ForeignTable = CurrentSource.AddTable("Foreign Table");
            ForeignTable.AddColumn<int>("Foreign Column", DbType.Int32);
            Table.SetupForeignKeys();

            DesiredSource = new Source("My Data");
            DesiredSource.AddFunction("Function A", "My Definition 2");
            DesiredSource.AddStoredProcedure("Stored Procedure A", "My Definition 2");
            Table = DesiredSource.AddTable("Table A");
            Table.AddCheckConstraint("Constraint A", "My Definition2");
            Table.AddColumn<int>("Column A", DbType.Int32);
            Table.AddColumn<string>("Column B", DbType.String);
            Table.AddTrigger("Trigger A", "My Definition 2", TriggerType.Update);
            Table.AddForeignKey("Column A", "Foreign Table", "Foreign Column");
            DesiredSource.AddView("View A", "My Definition 2");
            ForeignTable = DesiredSource.AddTable("Foreign Table");
            ForeignTable.AddColumn<int>("Foreign Column", DbType.Int32);
            Table.SetupForeignKeys();
        }

        protected ISource CurrentSource { get; set; }
        protected ISource DesiredSource { get; set; }
    }
}