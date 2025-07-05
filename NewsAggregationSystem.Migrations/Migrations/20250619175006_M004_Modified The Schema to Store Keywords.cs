using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAggregationSystem.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class M004_ModifiedTheSchematoStoreKeywords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationPreferences_UserNewsKeywords_UserNewsKeywordId",
                table: "NotificationPreferences");

            migrationBuilder.DropIndex(
                name: "IX_NotificationPreferences_UserNewsKeywordId",
                table: "NotificationPreferences");

            migrationBuilder.AddColumn<int>(
                name: "NewsCategoryId",
                table: "UserNewsKeywords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserNewsKeywords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserNewsKeywords_NewsCategoryId",
                table: "UserNewsKeywords",
                column: "NewsCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNewsKeywords_UserId",
                table: "UserNewsKeywords",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNewsKeywords_NewsCategories_NewsCategoryId",
                table: "UserNewsKeywords",
                column: "NewsCategoryId",
                principalTable: "NewsCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNewsKeywords_Users_UserId",
                table: "UserNewsKeywords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNewsKeywords_NewsCategories_NewsCategoryId",
                table: "UserNewsKeywords");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNewsKeywords_Users_UserId",
                table: "UserNewsKeywords");

            migrationBuilder.DropIndex(
                name: "IX_UserNewsKeywords_NewsCategoryId",
                table: "UserNewsKeywords");

            migrationBuilder.DropIndex(
                name: "IX_UserNewsKeywords_UserId",
                table: "UserNewsKeywords");

            migrationBuilder.DropColumn(
                name: "NewsCategoryId",
                table: "UserNewsKeywords");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserNewsKeywords");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserNewsKeywordId",
                table: "NotificationPreferences",
                column: "UserNewsKeywordId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationPreferences_UserNewsKeywords_UserNewsKeywordId",
                table: "NotificationPreferences",
                column: "UserNewsKeywordId",
                principalTable: "UserNewsKeywords",
                principalColumn: "Id");
        }
    }
}
