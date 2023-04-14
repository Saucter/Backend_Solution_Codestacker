using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PDF_Reader_APIs.Server.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SentencesLinkTxt",
                table: "PDFs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentencesLinkTxt",
                table: "PDFs");
        }
    }
}
