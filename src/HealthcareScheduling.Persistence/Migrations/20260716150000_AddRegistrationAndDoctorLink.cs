using System;
using HealthcareScheduling.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareScheduling.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260716150000_AddRegistrationAndDoctorLink")]
public partial class AddRegistrationAndDoctorLink : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Status",
            table: "Users",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<string>(
            name: "VerificationCode",
            table: "Users",
            type: "nvarchar(4)",
            maxLength: 4,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "VerificationCodeExpiresAt",
            table: "Users",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "DoctorId",
            table: "Users",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_DoctorId",
            table: "Users",
            column: "DoctorId");

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Doctors_DoctorId",
            table: "Users",
            column: "DoctorId",
            principalTable: "Doctors",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_Doctors_DoctorId",
            table: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Users_DoctorId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Status",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "VerificationCode",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "VerificationCodeExpiresAt",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "DoctorId",
            table: "Users");
    }
}
