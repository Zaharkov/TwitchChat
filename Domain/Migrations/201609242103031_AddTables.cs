using System.Data.Entity.Migrations;

namespace Domain.Migrations
{
    public partial class AddTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccessTokens",
                c => new
                    {
                        Type = c.Int(nullable: false),
                        Value = c.String(nullable: false, maxLength: 255),
                        Expire = c.DateTime(),
                    })
                .PrimaryKey(t => t.Type);
            
            CreateTable(
                "dbo.Chatters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        ChatName = c.String(nullable: false, maxLength: 255),
                        Type = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChatterInfoes",
                c => new
                    {
                        ChatterId = c.Int(nullable: false),
                        SteamId = c.Long(),
                        Seconds = c.Long(nullable: false),
                        QuizScore = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ChatterId)
                .ForeignKey("dbo.Chatters", t => t.ChatterId)
                .Index(t => t.ChatterId);
            
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Time = c.DateTime(nullable: false),
                        Message = c.String(nullable: false, maxLength: 4000),
                        Exception = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatterInfoes", "ChatterId", "dbo.Chatters");
            DropIndex("dbo.ChatterInfoes", new[] { "ChatterId" });
            DropTable("dbo.Logs");
            DropTable("dbo.ChatterInfoes");
            DropTable("dbo.Chatters");
            DropTable("dbo.AccessTokens");
        }
    }
}
