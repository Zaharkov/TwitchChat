namespace Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddRouletteFields : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RouletteInfoes",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CurrentTry = c.Int(nullable: false),
                        Streak = c.Int(nullable: false),
                        MaxStreak = c.Int(nullable: false),
                        TryCount = c.Int(nullable: false),
                        DeathCount = c.Int(nullable: false),
                        Percent = c.Double(nullable: false),
                        MaxPercent = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ChatterInfoes", "RouletteId", c => c.Long());
            DropColumn("dbo.ChatterInfoes", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChatterInfoes", "Type", c => c.String(nullable: false, maxLength: 255));
            DropColumn("dbo.ChatterInfoes", "RouletteId");
            DropTable("dbo.RouletteInfoes");
        }
    }
}
