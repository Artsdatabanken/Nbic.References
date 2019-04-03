using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Nbic.References.EFCore.Migrations.SqlServerMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RF_Reference",
                columns: table => new
                {
                    PK_ReferenceID = table.Column<Guid>(nullable: false),
                    Application_ID = table.Column<int>(nullable: true),
                    FK_UserID = table.Column<int>(nullable: true),
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
                    table.PrimaryKey("PK_RF_ReferenceNew", x => x.PK_ReferenceID);
                });

            migrationBuilder.CreateTable(
                name: "RF_Reference_Usage",
                columns: table => new
                {
                    FK_ReferenceId = table.Column<Guid>(nullable: false),
                    FK_Application_ID = table.Column<int>(nullable: false),
                    FK_UserID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RF_Reference_Usage", x => new { x.FK_ReferenceId, x.FK_Application_ID, x.FK_UserID });
                    table.ForeignKey(
                        name: "FK_RF_Reference_Usage_RF_Reference",
                        column: x => x.FK_ReferenceId,
                        principalTable: "RF_Reference",
                        principalColumn: "PK_ReferenceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "NonClusteredIndex-20190323-220323",
                table: "RF_Reference",
                columns: new[] { "FK_UserID", "Application_ID" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RF_Reference_Usage");

            migrationBuilder.DropTable(
                name: "RF_Reference");
        }
    }
}
