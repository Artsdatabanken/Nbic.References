using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Nbic.References.EFCore.Migrations.SqlServerMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reference",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ApplicationId = table.Column<int>(nullable: true),
                    UserId = table.Column<int>(nullable: true),
                    Author = table.Column<string>(unicode: false, nullable: true),
                    Year = table.Column<string>(unicode: false, nullable: true),
                    Title = table.Column<string>(unicode: false, nullable: true),
                    Summary = table.Column<string>(unicode: false, nullable: true),
                    Journal = table.Column<string>(unicode: false, nullable: true),
                    Volume = table.Column<string>(unicode: false, nullable: true),
                    Pages = table.Column<string>(unicode: false, nullable: true),
                    Bibliography = table.Column<string>(unicode: false, nullable: true),
                    Lastname = table.Column<string>(unicode: false, nullable: true),
                    Middlename = table.Column<string>(unicode: false, nullable: true),
                    Firstname = table.Column<string>(unicode: false, nullable: true),
                    URL = table.Column<string>(unicode: false, nullable: true),
                    Keywords = table.Column<string>(unicode: false, nullable: true),
                    ImportXML = table.Column<string>(type: "xml", nullable: true),
                    EditDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceUsage",
                columns: table => new
                {
                    ReferenceId = table.Column<Guid>(nullable: false),
                    ApplicationId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RF_Reference_Usage", x => new { x.ReferenceId, x.ApplicationId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ReferenceUsage_Reference",
                        column: x => x.ReferenceId,
                        principalTable: "Reference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserId_ApplicationId",
                table: "Reference",
                columns: new[] { "UserId", "ApplicationId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferenceUsage");

            migrationBuilder.DropTable(
                name: "Reference");
        }
    }
}
