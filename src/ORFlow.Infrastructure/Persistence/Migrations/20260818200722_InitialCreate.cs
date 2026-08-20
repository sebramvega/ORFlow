using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SurgeryRequests",
                columns: table => new
                {
                    SurgeryRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SurgeonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatingRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcedureName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestStatus = table.Column<int>(type: "int", nullable: false),
                    RequestedEndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RequestedStartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurgeryRequests", x => x.SurgeryRequestId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurgeryRequests");
        }
    }
}
