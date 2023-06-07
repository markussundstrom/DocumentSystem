using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentSystem.Migrations
{
    public partial class make_ownerid_column_nullable_for_nodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Users_OwnerId",
                table: "Nodes");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Nodes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Users_OwnerId",
                table: "Nodes",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Users_OwnerId",
                table: "Nodes");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Nodes",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Users_OwnerId",
                table: "Nodes",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
