using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FluentMigrator;
namespace DBMigration
{
    [Migration(201211291923)]
    public class MainDatabase00002 : Migration
    {
        public override void Down()
        {
            Delete.Table("Directories");
        }

        public override void Up()
        {
            Create.Table("Directories")
                .WithColumn("Id").AsInt64().NotNullable().PrimaryKey().Identity()
                .WithColumn("Directory").AsString(1000).NotNullable().WithDefaultValue("");

        }
    }
}
