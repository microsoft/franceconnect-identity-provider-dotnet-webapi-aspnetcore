using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApiIdentityProviderDotNet.Migrations
{
    public partial class CreateDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    UserId = table.Column<string>(maxLength: 256, nullable: false),
                    PublicKeyHash = table.Column<string>(nullable: false),
                    ActiveChallenge = table.Column<string>(nullable: true),
                    DeviceName = table.Column<string>(nullable: false),
                    PublicKey = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => new { x.UserId, x.PublicKeyHash });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credentials");
        }
    }
}
