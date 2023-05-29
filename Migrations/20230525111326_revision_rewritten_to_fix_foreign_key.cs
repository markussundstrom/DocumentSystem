using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentSystem.Migrations
{
    public partial class revision_rewritten_to_fix_foreign_key : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Revisions_Nodes_DocumentId",
                table: "Revisions");

            migrationBuilder.DropForeignKey(
                name: "FK_Revisions_Nodes_NodeId",
                table: "Revisions");

            migrationBuilder.DropIndex(
                name: "IX_Revisions_NodeId",
                table: "Revisions");

            migrationBuilder.DropColumn(
                name: "NodeId",
                table: "Revisions");

            migrationBuilder.AlterColumn<Guid>(
                name: "DocumentId",
                table: "Revisions",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Revisions_Nodes_DocumentId",
                table: "Revisions",
                column: "DocumentId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Revisions_Nodes_DocumentId",
                table: "Revisions");

            migrationBuilder.AlterColumn<Guid>(
                name: "DocumentId",
                table: "Revisions",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "NodeId",
                table: "Revisions",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Revisions_NodeId",
                table: "Revisions",
                column: "NodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Revisions_Nodes_DocumentId",
                table: "Revisions",
                column: "DocumentId",
                principalTable: "Nodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Revisions_Nodes_NodeId",
                table: "Revisions",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
