using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentSystem.Migrations
{
    public partial class added_fileid_guid_to_revisions_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FileId",
                table: "Revisions",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Revisions");
        }
    }
}
