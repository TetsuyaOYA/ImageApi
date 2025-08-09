using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Uploaded",
                table: "Images",
                newName: "UploadedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "Images",
                newName: "Uploaded");
        }
    }
}
