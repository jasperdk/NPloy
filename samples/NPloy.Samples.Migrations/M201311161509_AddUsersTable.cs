using FluentMigrator;

namespace NPloy.Samples.Migrations
{
    [Migration(201311161509)]
    public class M201311161509_AddUsersTable : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255).NotNullable().WithDefaultValue("Anonymous");
        }

        public override void Down()
        {
            Delete.Table("Users");
        }
    }
}