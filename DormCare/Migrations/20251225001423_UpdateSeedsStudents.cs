using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DormCare.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedsStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Requests",
                keyColumn: "Id",
                keyValue: 2,
                column: "Status",
                value: "Done");


            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Capacity", "Number" },
                values: new object[] { 3, 2, "103C" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "Password", "Phone", "Role", "Username" },
                values: new object[,]
                {
                    { 2, "ivanov@student.local", "Ivan Ivanov", "password1", "+380501111111", "Student", "ivanov" },
                    { 3, "petrenko@student.local", "Olena Petrenko", "password2", "+380502222222", "Student", "petrenko" },
                    { 4, "shevchenko@student.local", "Taras Shevchenko", "password3", "+380503333333", "Student", "shevchenko" },
                    { 5, "koval@student.local", "Anna Koval", "password4", "+380504444444", "Student", "koval" },
                    { 6, "melnyk@student.local", "Oksana Melnyk", "password5", "+380505555555", "Student", "melnyk" }
                });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "RoomId", "UserId" },
                values: new object[] { 1, 3 });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "RoomId", "UserId" },
                values: new object[,]
                {
                    { 3, 2, 4 },
                    { 4, 2, 5 },
                    { 5, 3, 6 }
                });

            // Ensure any existing request with Id=3 is removed before inserting (avoid unique constraint failures)
            migrationBuilder.DeleteData(
                table: "Requests",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.InsertData(
                table: "Requests",
                columns: new[] { "Id", "CreatedAt", "Status", "StudentId", "Text", "Type" },
                values: new object[] { 3, new DateTime(2025, 12, 22, 9, 0, 0, 0, DateTimeKind.Unspecified), "New", 3, "Broken window latch", "Maintenance" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Requests",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Requests",
                keyColumn: "Id",
                keyValue: 2,
                column: "Status",
                value: "Completed");

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "RoomId", "UserId" },
                values: new object[] { 2, 1 });
        }
    }
}
