using System;
using HealthcareScheduling.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareScheduling.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260716140000_AddUserRole")]
public partial class AddUserRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Role",
            table: "Users",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.Sql(
            """
            UPDATE Users
            SET Role = 0
            WHERE Email = 'admin@healthcare.local'
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Role",
            table: "Users");
    }
}
