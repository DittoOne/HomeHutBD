using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeHutBD.Migrations
{
    /// <inheritdoc />
    public partial class verificationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Properties_nid_verification",
                table: "Properties");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "nid_verification",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_nid_verification",
                table: "Properties",
                column: "nid_verification",
                unique: true,
                filter: "[nid_verification] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Properties_nid_verification",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "nid_verification",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_nid_verification",
                table: "Properties",
                column: "nid_verification",
                unique: true);
        }
    }
}
