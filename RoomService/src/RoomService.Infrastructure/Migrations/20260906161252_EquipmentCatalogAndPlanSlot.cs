using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EquipmentCatalogAndPlanSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlanSlot",
                table: "Rooms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Placement",
                table: "RoomEquipment",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Normaliza o texto livre para o vocabulario ANTES de estreitar a
            // coluna: correspondencia sem diferenciar caixa, e o que nao casar
            // vira Outro. Nenhuma linha e perdida. Ver docs/specs/004.
            migrationBuilder.Sql("""
                UPDATE "Equipments" SET "Type" = CASE
                    WHEN lower(trim("Type")) IN ('tv', 'televisao', 'televisão') THEN 'Tv'
                    WHEN lower(trim("Type")) IN ('monitor', 'tela')              THEN 'Monitor'
                    WHEN lower(trim("Type")) IN ('quadrobranco', 'quadro branco') THEN 'QuadroBranco'
                    WHEN lower(trim("Type")) IN ('projetor', 'datashow')          THEN 'Projetor'
                    WHEN lower(trim("Type")) IN ('arcondicionado', 'ar condicionado') THEN 'ArCondicionado'
                    WHEN lower(trim("Type")) IN ('telefone')                      THEN 'Telefone'
                    WHEN lower(trim("Type")) IN ('notebook', 'laptop')            THEN 'Notebook'
                    WHEN lower(trim("Type")) IN ('dock', 'dockstation')           THEN 'Dock'
                    WHEN lower(trim("Type")) IN ('flipchart')                     THEN 'Flipchart'
                    WHEN lower(trim("Type")) IN ('cadeira')                       THEN 'Cadeira'
                    ELSE 'Outro'
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Equipments",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_PlanSlot",
                table: "Rooms",
                column: "PlanSlot",
                unique: true,
                filter: "\"PlanSlot\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_PlanSlot",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "PlanSlot",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Placement",
                table: "RoomEquipment");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Equipments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);
        }
    }
}
