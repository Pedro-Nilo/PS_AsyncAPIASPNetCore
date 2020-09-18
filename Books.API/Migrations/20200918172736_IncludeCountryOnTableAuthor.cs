using Microsoft.EntityFrameworkCore.Migrations;

namespace Books.API.Migrations
{
    public partial class IncludeCountryOnTableAuthor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Authors",
                maxLength: 150,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Authors");
        }
    }
}
