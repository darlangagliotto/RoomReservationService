using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnEquipmentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoomEquipment_EquipmentId",
                table: "RoomEquipment");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEquipment_EquipmentId",
                table: "RoomEquipment",
                column: "EquipmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoomEquipment_EquipmentId",
                table: "RoomEquipment");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEquipment_EquipmentId",
                table: "RoomEquipment",
                column: "EquipmentId");
        }
    }
}
