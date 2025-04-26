using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learning_platform.Migrations
{
    /// <inheritdoc />
    public partial class faivoratelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1db33115-f262-4af5-a83e-3ca93020fa89");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Votes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorites_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "ec36c830-f0a1-4538-a324-c756bc02ce61");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "4733cea8-0e4b-446f-a61e-3eb628dbe578", "18b65823-ceee-474e-959b-9d9fda75ff48", "User", "user" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "f597f480-119f-4003-9f3b-746c131ad3a9", "AQAAAAIAAYagAAAAEORs451fAqVDBISFq4gw0ov4E/xQETylE/2fbLUnddiOxOlheAmYogQswT3wO0YMjw==" });

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_CourseId",
                table: "Favorites",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4733cea8-0e4b-446f-a61e-3eb628dbe578");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Votes");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "8be650b8-64fc-43f2-9729-3f5cdb3c1bdc");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "1db33115-f262-4af5-a83e-3ca93020fa89", "97baf32f-a29a-4ec7-b1cc-2fdf5c7c6969", "User", "user" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "b219b702-92dc-421a-a69b-11fa9be29dae", "AQAAAAIAAYagAAAAEKbGXB8XdYpvl9do0Fluuj6aiKDtNeVNvDOZiaXoBeoMPdD4QI0EW0joIvasUJUk/A==" });
        }
    }
}
