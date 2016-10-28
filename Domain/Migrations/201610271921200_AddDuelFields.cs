namespace Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDuelFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RouletteInfoes", "DuelName", c => c.String(maxLength: 255));
            AddColumn("dbo.RouletteInfoes", "DuelScore", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RouletteInfoes", "DuelScore");
            DropColumn("dbo.RouletteInfoes", "DuelName");
        }
    }
}
