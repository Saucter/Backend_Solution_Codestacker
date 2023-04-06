using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PDF_Reader_APIs.Server.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PDFs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeOfUpload = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileSize = table.Column<double>(type: "float", nullable: false),
                    NumberOfPages = table.Column<int>(type: "int", nullable: false),
                    FileLink = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDFs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Sentences",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PDFid = table.Column<int>(type: "int", nullable: false),
                    Sentence = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sentences", x => x.id);
                    table.ForeignKey(
                        name: "FK_Sentences_PDFs_PDFid",
                        column: x => x.PDFid,
                        principalTable: "PDFs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sentences_PDFid",
                table: "Sentences",
                column: "PDFid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sentences");

            migrationBuilder.DropTable(
                name: "PDFs");
        }
    }
}
