using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FluentMigrator;
namespace DBMigration
{
    [Migration(201212021545)]
    public class MainDatabase00003 : Migration
    {
        public override void Down()
        {
            Delete.Table("Queue");
        }

        public override void Up()
        {
            Create.Table("Queue")
                .WithColumn("Id").AsInt64().NotNullable().PrimaryKey().Identity()
                .WithColumn("UserName").AsString(200).NotNullable().WithDefaultValue("")
                .WithColumn("FilePath").AsString(1000).NotNullable().WithDefaultValue("")
                .WithColumn("FileName").AsString(1000).NotNullable().WithDefaultValue("")
                .WithColumn("Date").AsDateTime().NotNullable()
                .WithColumn("PlayOrder").AsInt32().NotNullable();

        }
    }
}
