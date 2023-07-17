# Data.Modeler

[![Build status](https://ci.appveyor.com/api/projects/status/15bedpade50j03f2?svg=true)](https://ci.appveyor.com/project/JaCraig/data-modeler)

Data.Modeler is a library used to interact with model database schemas in C#. Works with .Net Core.

## Basic Usage

In order to use Data.Modeler, you need to first wire up the system with your ServiceCollection. In order to do this, all you need to do is make one method call:

    serviceCollection.AddCanisterModules();
					
This line is required prior to using the DataModeler class for the first time. Once Canister is set up, you can call the DataModeler class provided:

    var SchemaProvider = DataModeler.GetSchemaGenerator(SqlClientFactory.Instance);
	
Note that the above gets the schema provider for SQL Server but for other databases you must provide the DbProviderFactory associated with it. You can also start creating a schema:

    var Source = DataModeler.CreateSource("MySource");
	
The "MySource" string is the database name that you wish to use.

## Creating a Schema

Once you have your ISource object, you can start adding on to it:

    var Table = Source.AddTable("TableName", "dbo");
	var Column = Table.AddColumn<int>("ColumnName",DbType.Int32);
	var CheckConstraint = Table.AddCheckConstraint("CheckConstraintName", "Check Constraint Definition");
	var View = Source.AddView("ViewName","View Creation Code", "dbo");
	var Function = Source.AddFunction("FunctionName","Function Creation Code", "dbo");
	var StoredProcedure = Source.AddStoredProcedure("StoredProcedureName","Stored Procedure Creation Code", "dbo");
	
From there the schema provider can be used to either generate the commands needed to create the database or what commands are needed to alter an existing database to the desired schema:

    var MyCommands = SchemaProvider.GenerateSchema(DesiredSchema, SourceSchema);
	
If SourceSchema is null, then it will treat it as the database doesn't exist. If the SourceSchema is not null, it will act as though the database exists and the DesiredSchema is what you want the final schema to look like. Note that deleting columns, tables, etc. is not done by the system. It will, however, generate calls to drop functions, stored procedures, views, constraints, etc. if they need to be updated. Another thing you can do is have the system apply those changes for you:

    SchemaProvider.Setup(DesiredSchema,new Connection(Configuration, SqlClientFactory.Instance, "Default"));
	
The connection object must be fed a IConfiguration object, a DbProviderFactory for the database type you wish it to connect to, and either the name of your connection string in the configuration object or a connection string:

    SchemaProvider.Setup(DesiredSchema,new Connection(Configuration, SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=ExampleDatabase;Integrated Security=SSPI;Pooling=false"));

## Adding a ISchemaGenerator

The schema generator is what the system uses to generate the individual commands. Data.Modeler comes with one for SQL Server but in order to add your own you must create a class that inherits from ISchemaGenerator. From there the system will automatically pick up the schema generator and allow you to use it:

    var SchemaProvider = DataModeler.GetSchemaGenerator(MyDbFactoryProvider);
	
Note that you can also create one for SQL Server and the system will actually return the one that you create instead of the one built into the system.

## Installation

The library is available via Nuget with the package name "Data.Modeler". To install it run the following command in the Package Manager Console:

Install-Package Data.Modeler

## Build Process

In order to build the library you will require the following as a minimum:

1. Visual Studio 2022

Other than that, just clone the project and you should be able to load the solution and build without too much effort.