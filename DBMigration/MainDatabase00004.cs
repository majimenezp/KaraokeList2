using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FluentMigrator;
namespace DBMigration
{
    [Migration(201212021851)]
    public class MainDatabase00004 : Migration
    {
        public override void Down()
        {
            Delete.Column("Played");
        }

        public override void Up()
        {
            Create.Column("Played").OnTable("Queue").AsBoolean().NotNullable().WithDefaultValue(false);
        }
    }
}
