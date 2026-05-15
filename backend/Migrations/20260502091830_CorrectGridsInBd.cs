using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcComponentsApi.Migrations
{
    /// <inheritdoc />
    public partial class CorrectGridsInBd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CpuName",
                table: "OverclockProfiles",
                newName: "CpuId");

            migrationBuilder.RenameColumn(
                name: "CpuName",
                table: "CpuSupports",
                newName: "CpuId");

            migrationBuilder.CreateIndex(
                name: "IX_CpuSupports_CpuId",
                table: "CpuSupports",
                column: "CpuId");

            migrationBuilder.AddForeignKey(
                name: "FK_CpuSupports_Components_CpuId",
                table: "CpuSupports",
                column: "CpuId",
                principalTable: "Components",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CpuSupports_Components_CpuId",
                table: "CpuSupports");

            migrationBuilder.DropIndex(
                name: "IX_CpuSupports_CpuId",
                table: "CpuSupports");

            migrationBuilder.RenameColumn(
                name: "CpuId",
                table: "OverclockProfiles",
                newName: "CpuName");

            migrationBuilder.RenameColumn(
                name: "CpuId",
                table: "CpuSupports",
                newName: "CpuName");
        }
    }
}
