namespace Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUnusedColumn : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ChatterInfoes", "ChatterId", "dbo.Chatters");
            DropIndex("dbo.ChatterInfoes", new[] { "ChatterId" });
            DropPrimaryKey("dbo.ChatterInfoes");
            AddPrimaryKey("dbo.ChatterInfoes", new[] { "Name", "ChatName" });
            DropColumn("dbo.ChatterInfoes", "ChatterId");
            DropTable("dbo.Chatters");
        }
        
        public override void Down()
        {
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
            
            AddColumn("dbo.ChatterInfoes", "ChatterId", c => c.Int(nullable: false));
            DropPrimaryKey("dbo.ChatterInfoes");
            AddPrimaryKey("dbo.ChatterInfoes", "ChatterId");
            CreateIndex("dbo.ChatterInfoes", "ChatterId");
            AddForeignKey("dbo.ChatterInfoes", "ChatterId", "dbo.Chatters", "Id");
        }
    }
}
