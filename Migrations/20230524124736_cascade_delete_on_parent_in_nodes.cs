using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentSystem.Migrations
{
    public partial class cascade_delete_on_parent_in_nodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Nodes_ParentId",
                table: "Nodes");

            migrationBuilder.AddColumn<Guid>(
                name: "FolderId",
                table: "Nodes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_FolderId",
                table: "Nodes",
                column: "FolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Nodes_FolderId",
                table: "Nodes",
                column: "FolderId",
                principalTable: "Nodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Nodes_ParentId",
                table: "Nodes",
                column: "ParentId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Nodes_FolderId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Nodes_ParentId",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_FolderId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "Nodes");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Nodes_ParentId",
                table: "Nodes",
                column: "ParentId",
                principalTable: "Nodes",
                principalColumn: "Id");
        }
    }
}
