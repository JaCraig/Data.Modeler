﻿using Data.Modeler.Providers.SQLServer.CommandBuilders;
using Data.Modeler.Tests.BaseClasses;
using System;
using System.Linq;
using Xunit;

namespace Data.Modeler.Tests.Providers.SQLServer.CommandBuilders
{
    public class TableCommandBuilderTests : CommandBuilderTestBase
    {
        [Fact]
        public void Creation()
        {
            var TempCheckConstraint = new TableCommandBuilder(ObjectPool);
            Assert.NotNull(TempCheckConstraint);
            Assert.Equal(10, TempCheckConstraint.Order);
        }

        [Fact]
        public void GetCommandsNoCurrentSource()
        {
            var TempCheckConstraint = new TableCommandBuilder(ObjectPool);
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, null).ToList();
            Assert.Equal(2, Commands.Count);
            Assert.Equal("CREATE TABLE [dbo].[Table A]([Column A] Int,[Column B] NVarChar(MAX))", Commands[0]);
            Assert.Equal("CREATE TABLE [dbo].[Foreign Table]([Foreign Column] Int)", Commands[1]);
        }

        [Fact]
        public void GetCommandsNoCurrentSourceWithDefaults()
        {
            DesiredSource.Tables[0].AddColumn<TimeSpan>("OtherStuff", System.Data.DbType.DateTime, nullable: false);
            var TempCheckConstraint = new TableCommandBuilder(ObjectPool);
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, null).ToList();
            Assert.Equal(2, Commands.Count);
            Assert.Equal("CREATE TABLE [dbo].[Table A]([Column A] Int,[Column B] NVarChar(MAX),[OtherStuff] DateTime NOT NULL)", Commands[0]);
            Assert.Equal("CREATE TABLE [dbo].[Foreign Table]([Foreign Column] Int)", Commands[1]);
        }

        [Fact]
        public void GetCommandsNoCurrentSourceWithDefaults2()
        {
            DesiredSource.Tables[0].AddColumn<string>("OtherStuff", System.Data.DbType.DateTime, nullable: false, defaultValue: "");
            var TempCheckConstraint = new TableCommandBuilder(ObjectPool);
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, null).ToList();
            Assert.Equal(2, Commands.Count);
            Assert.Equal("CREATE TABLE [dbo].[Table A]([Column A] Int,[Column B] NVarChar(MAX),[OtherStuff] DateTime NOT NULL)", Commands[0]);
            Assert.Equal("CREATE TABLE [dbo].[Foreign Table]([Foreign Column] Int)", Commands[1]);
        }

        [Fact]
        public void GetCommandsWithCurrentSource()
        {
            var TempCheckConstraint = new TableCommandBuilder(ObjectPool);
            var Commands = TempCheckConstraint.GetCommands(DesiredSource, CurrentSource).ToList();
            Assert.Single(Commands);
            Assert.Equal("ALTER TABLE [dbo].[Table A] ADD [Column B] NVarChar(MAX)", Commands[0]);
        }
    }
}