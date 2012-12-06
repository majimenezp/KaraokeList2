using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FluentMigrator;
namespace DBMigration
{
    [Migration(201211291840)]
    public class MainDatabase0001:Migration
    {

        public override void Down()
        {
            Delete.Table("KaraokeFiles");
        }

        public override void Up()
        {
            Create.Table("KaraokeFiles")
                .WithColumn("Id").AsInt64().NotNullable().PrimaryKey().Identity()
                .WithColumn("Filename").AsString(400).NotNullable().WithDefaultValue("")
                .WithColumn("FullFilePath").AsString(2000).NotNullable();
        }
    }
}
