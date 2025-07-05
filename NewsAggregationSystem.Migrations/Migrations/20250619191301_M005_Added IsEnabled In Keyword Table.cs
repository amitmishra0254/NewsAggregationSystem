using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAggregationSystem.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class M005_AddedIsEnabledInKeywordTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserNewsKeywordId",
                table: "NotificationPreferences");

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "UserNewsKeywords",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "UserNewsKeywords");

            migrationBuilder.AddColumn<int>(
                name: "UserNewsKeywordId",
                table: "NotificationPreferences",
                type: "int",
                nullable: true);
        }
    }
}
