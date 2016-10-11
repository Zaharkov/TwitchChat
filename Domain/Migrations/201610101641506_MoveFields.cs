namespace Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoveFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChatterInfoes", "Name", c => c.String(nullable: false, maxLength: 255));
            AddColumn("dbo.ChatterInfoes", "ChatName", c => c.String(nullable: false, maxLength: 255));
            AddColumn("dbo.ChatterInfoes", "Type", c => c.String(nullable: false, maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChatterInfoes", "Type");
            DropColumn("dbo.ChatterInfoes", "ChatName");
            DropColumn("dbo.ChatterInfoes", "Name");
        }
    }
}
