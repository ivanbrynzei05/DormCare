using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormCare.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FullName", "Phone", "Username" },
                values: new object[] { "Administrator", "+380000000000", "admin" });

            // Ensure existing rows get unique usernames before creating unique index
            if (migrationBuilder.ActiveProvider?.Contains("SqlServer") == true)
            {
                // SQL Server: use CONCAT and DATALENGTH to check empty TEXT columns
                migrationBuilder.Sql(@"UPDATE Users SET Username = CONCAT(Email, '_', Id) WHERE Username IS NULL OR DATALENGTH(Username) = 0;");
                migrationBuilder.Sql(@"UPDATE Users SET FullName = Email WHERE FullName IS NULL OR DATALENGTH(FullName) = 0;");
            }
            else
            {
                // Assume SQLite or other providers supporting || concatenation and '' checks
                migrationBuilder.Sql(@"UPDATE Users SET Username = Email || '_' || Id WHERE Username = '' OR Username IS NULL;");
                migrationBuilder.Sql(@"UPDATE Users SET FullName = Email WHERE FullName = '' OR FullName IS NULL;");
            }

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");
        }
    }
}
