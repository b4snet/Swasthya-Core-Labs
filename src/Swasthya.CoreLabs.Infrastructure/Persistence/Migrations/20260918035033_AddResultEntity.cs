using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swasthya.CoreLabs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResultEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    specimen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value_type_code = table.Column<int>(type: "integer", nullable: false),
                    numeric_value = table.Column<decimal>(type: "numeric", nullable: true),
                    textual_value = table.Column<string>(type: "text", nullable: true),
                    coded_value = table.Column<string>(type: "text", nullable: true),
                    coded_system = table.Column<string>(type: "text", nullable: true),
                    unit_ucum_code = table.Column<string>(type: "text", nullable: true),
                    observed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    entered_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    entered_by_principal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_code = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_results", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "results");
        }
    }
}
