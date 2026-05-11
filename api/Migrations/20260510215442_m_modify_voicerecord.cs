using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class m_modify_voicerecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "VoiceRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecognizeJson",
                table: "VoiceRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "VoiceRecordId",
                table: "Tasks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "VoiceRecords");

            migrationBuilder.DropColumn(
                name: "RecognizeJson",
                table: "VoiceRecords");

            migrationBuilder.AlterColumn<Guid>(
                name: "VoiceRecordId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
