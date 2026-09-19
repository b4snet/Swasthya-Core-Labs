using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swasthya.CoreLabs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosticOrderFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_order_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    patient_external_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    patient_external_identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    encounter_external_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    encounter_external_identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    requested_by_principal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requested_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    cancelled_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_principals_requested_by_principal_id",
                        column: x => x.requested_by_principal_id,
                        principalTable: "principals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    test_id = table.Column<Guid>(type: "uuid", nullable: true),
                    panel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    test_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    panel_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    test_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    test_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    panel_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    panel_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: true),
                    requested_quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    quantity_unit_ucum_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    cancelled_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.CheckConstraint("ck_order_items_single_target", "((test_id IS NOT NULL)::int + (panel_id IS NOT NULL)::int) = 1");
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_items_panel_versions_panel_version_id",
                        column: x => x.panel_version_id,
                        principalTable: "panel_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_items_panels_panel_id",
                        column: x => x.panel_id,
                        principalTable: "panels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_items_test_versions_test_version_id",
                        column: x => x.test_version_id,
                        principalTable: "test_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_items_tests_test_id",
                        column: x => x.test_id,
                        principalTable: "tests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id_sequence_number",
                table: "order_items",
                columns: new[] { "order_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id_status",
                table: "order_items",
                columns: new[] { "order_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_order_items_panel_id",
                table: "order_items",
                column: "panel_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_panel_version_id",
                table: "order_items",
                column: "panel_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_test_id",
                table: "order_items",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_test_version_id",
                table: "order_items",
                column: "test_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_facility_id",
                table: "orders",
                column: "facility_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_organization_id_external_order_id",
                table: "orders",
                columns: new[] { "organization_id", "external_order_id" },
                unique: true,
                filter: "\"external_order_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_orders_organization_id_facility_id_status",
                table: "orders",
                columns: new[] { "organization_id", "facility_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_organization_id_order_number",
                table: "orders",
                columns: new[] { "organization_id", "order_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_organization_id_status_requested_at_utc",
                table: "orders",
                columns: new[] { "organization_id", "status", "requested_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_requested_by_principal_id",
                table: "orders",
                column: "requested_by_principal_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
