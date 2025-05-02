using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanArchitectureMigrationV13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Destinations_FromDestinationId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Destinations_ToDestinationId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_FromDestinationId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ToDestinationId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "FromDestinationId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ToDestinationId",
                table: "Reservations");

            migrationBuilder.AddColumn<string>(
                name: "FromWhere",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToWhere",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromWhere",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ToWhere",
                table: "Reservations");

            migrationBuilder.AddColumn<string>(
                name: "FromDestinationId",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToDestinationId",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DestinationName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_FromDestinationId",
                table: "Reservations",
                column: "FromDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ToDestinationId",
                table: "Reservations",
                column: "ToDestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Destinations_FromDestinationId",
                table: "Reservations",
                column: "FromDestinationId",
                principalTable: "Destinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Destinations_ToDestinationId",
                table: "Reservations",
                column: "ToDestinationId",
                principalTable: "Destinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
