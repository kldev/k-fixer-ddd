using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace K.Fixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tax_id_country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    tax_id_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    contact_email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company", x => x.id);
                    table.UniqueConstraint("AK_company_gid", x => x.gid);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_priority_dict",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_pl = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_priority_dict", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_status_dict",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_pl = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_status_dict", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_dict",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_pl = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_dict", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "building",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    address_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    address_street = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_city = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building", x => x.id);
                    table.UniqueConstraint("AK_building_gid", x => x.gid);
                    table.ForeignKey(
                        name: "FK_building_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    gid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.UniqueConstraint("AK_users_gid", x => x.gid);
                    table.ForeignKey(
                        name: "FK_users_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_requests",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    building_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolution_note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    start_date_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    gid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_maintenance_requests_building_building_id",
                        column: x => x.building_id,
                        principalTable: "building",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintenance_requests_users_assigned_to_id",
                        column: x => x.assigned_to_id,
                        principalTable: "users",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resident_assignments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    building_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    gid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resident_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_resident_assignments_building_building_id",
                        column: x => x.building_id,
                        principalTable: "building",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_resident_assignments_users_resident_id",
                        column: x => x.resident_id,
                        principalTable: "users",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_status_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    new_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    changed_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaintenanceRequestRecordId = table.Column<long>(type: "bigint", nullable: true),
                    gid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_status_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_maintenance_status_log_maintenance_requests_MaintenanceRequ~",
                        column: x => x.MaintenanceRequestRecordId,
                        principalTable: "maintenance_requests",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_maintenance_status_log_users_changed_by_id",
                        column: x => x.changed_by_id,
                        principalTable: "users",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_gid",
                table: "admin_users",
                column: "gid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_username",
                table: "admin_users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_building_company_id",
                table: "building",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_building_created_at",
                table: "building",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_building_gid",
                table: "building",
                column: "gid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_building_is_active",
                table: "building",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_company_created_at",
                table: "company",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_company_gid",
                table: "company",
                column: "gid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_tax_id_country_tax_id_number",
                table: "company",
                columns: new[] { "tax_id_country", "tax_id_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_priority_dict_priority",
                table: "maintenance_priority_dict",
                column: "priority",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_requests_assigned_to_id",
                table: "maintenance_requests",
                column: "assigned_to_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_requests_building_id",
                table: "maintenance_requests",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_requests_completed_at_utc",
                table: "maintenance_requests",
                column: "completed_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_requests_gid",
                table: "maintenance_requests",
                column: "gid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_requests_start_date_at_utc",
                table: "maintenance_requests",
                column: "start_date_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_status_dict_status",
                table: "maintenance_status_dict",
                column: "status",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_status_log_changed_at",
                table: "maintenance_status_log",
                column: "changed_at");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_status_log_changed_by_id",
                table: "maintenance_status_log",
                column: "changed_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_status_log_gid",
                table: "maintenance_status_log",
                column: "gid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_status_log_MaintenanceRequestRecordId",
                table: "maintenance_status_log",
                column: "MaintenanceRequestRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_status_log_request_id",
                table: "maintenance_status_log",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_resident_assignments_building_id",
                table: "resident_assignments",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "IX_resident_assignments_gid",
                table: "resident_assignments",
                column: "gid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resident_assignments_period_from",
                table: "resident_assignments",
                column: "period_from");

            migrationBuilder.CreateIndex(
                name: "IX_resident_assignments_period_to",
                table: "resident_assignments",
                column: "period_to");

            migrationBuilder.CreateIndex(
                name: "IX_resident_assignments_resident_id",
                table: "resident_assignments",
                column: "resident_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_dict_role",
                table: "role_dict",
                column: "role",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_company_id",
                table: "users",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_created_at",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_gid",
                table: "users",
                column: "gid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_modified_at",
                table: "users",
                column: "modified_at");

            migrationBuilder.CreateIndex(
                name: "IX_users_role",
                table: "users",
                column: "role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "maintenance_priority_dict");

            migrationBuilder.DropTable(
                name: "maintenance_status_dict");

            migrationBuilder.DropTable(
                name: "maintenance_status_log");

            migrationBuilder.DropTable(
                name: "resident_assignments");

            migrationBuilder.DropTable(
                name: "role_dict");

            migrationBuilder.DropTable(
                name: "maintenance_requests");

            migrationBuilder.DropTable(
                name: "building");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "company");
        }
    }
}
