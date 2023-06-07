using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentSystem.Migrations
{
    public partial class add_hinting_for_permissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Nodes_FolderId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Revisions_RevisionId",
                table: "Permissions");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Nodes_FolderId",
                table: "Permissions",
                column: "FolderId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Revisions_RevisionId",
                table: "Permissions",
                column: "RevisionId",
                principalTable: "Revisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Nodes_FolderId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Revisions_RevisionId",
                table: "Permissions");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Nodes_FolderId",
                table: "Permissions",
                column: "FolderId",
                principalTable: "Nodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Revisions_RevisionId",
                table: "Permissions",
                column: "RevisionId",
                principalTable: "Revisions",
                principalColumn: "Id");
        }
    }
}
