using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubtitleAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacySubtitleExports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SrtContent",
                table: "Transcripts");

            migrationBuilder.DropColumn(
                name: "VttContent",
                table: "Transcripts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SrtContent",
                table: "Transcripts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VttContent",
                table: "Transcripts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
