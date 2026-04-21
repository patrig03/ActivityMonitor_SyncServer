using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SyncServer.Infrastructure.Data;

#nullable disable

namespace SyncServer.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260421152500_AddSyncCompatibilityFields")]
public class AddSyncCompatibilityFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "CategoryId",
            table: "Activities",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

        migrationBuilder.AddColumn<string>(
            name: "ClassName",
            table: "Applications",
            type: "varchar(255)",
            maxLength: 255,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<int>(
            name: "Height",
            table: "Applications",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PositionX",
            table: "Applications",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PositionY",
            table: "Applications",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ProcessName",
            table: "Applications",
            type: "varchar(255)",
            maxLength: 255,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<int>(
            name: "Width",
            table: "Applications",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "WindowId",
            table: "Applications",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "WindowTitle",
            table: "Applications",
            type: "varchar(1024)",
            maxLength: 1024,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Categories",
            type: "varchar(2048)",
            maxLength: 2048,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<Guid>(
            name: "ApplicationId",
            table: "Thresholds",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

        migrationBuilder.AddColumn<string>(
            name: "DurationType",
            table: "Thresholds",
            type: "varchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "Daily")
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<int>(
            name: "SessionLimitSec",
            table: "Thresholds",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "TargetType",
            table: "Thresholds",
            type: "varchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "Category")
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CategoryId",
            table: "Activities");

        migrationBuilder.DropColumn(
            name: "ClassName",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "Height",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "PositionX",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "PositionY",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "ProcessName",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "Width",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "WindowId",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "WindowTitle",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "Description",
            table: "Categories");

        migrationBuilder.DropColumn(
            name: "ApplicationId",
            table: "Thresholds");

        migrationBuilder.DropColumn(
            name: "DurationType",
            table: "Thresholds");

        migrationBuilder.DropColumn(
            name: "SessionLimitSec",
            table: "Thresholds");

        migrationBuilder.DropColumn(
            name: "TargetType",
            table: "Thresholds");
    }
}
