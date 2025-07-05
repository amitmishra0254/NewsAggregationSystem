using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NewsAggregationSystem.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class M006_InsertedTheSeedDataForArticleReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Reactions",
                columns: new[] { "Id", "CreatedById", "CreatedDate", "ModifiedById", "ModifiedDate", "Name" },
                values: new object[,]
                {
                    { 1, -1, new DateTime(2025, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Like" },
                    { 2, -1, new DateTime(2025, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Dislike" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Reactions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Reactions",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
