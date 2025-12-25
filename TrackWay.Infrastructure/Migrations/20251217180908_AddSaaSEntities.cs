using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrackWay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSaaSEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "saas");

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                schema: "saas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PrecioMensual = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxUsuarios = table.Column<int>(type: "int", nullable: false),
                    MaxVehiculos = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "saas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RUC = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmailContacto = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaDesactivacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalUsuarios = table.Column<int>(type: "int", nullable: false),
                    TotalVehiculos = table.Column<int>(type: "int", nullable: false),
                    TotalMantenimientos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantSubscriptions",
                schema: "saas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionPlanId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantSubscriptions_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalSchema: "saas",
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantSubscriptions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "saas",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "saas",
                table: "SubscriptionPlans",
                columns: new[] { "Id", "Activo", "ColorHex", "Descripcion", "MaxUsuarios", "MaxVehiculos", "Nombre", "PrecioMensual", "Tier" },
                values: new object[,]
                {
                    { 1, true, "#95a5a6", "Plan gratuito con funcionalidades básicas", 3, 5, "Free", 0m, "Free" },
                    { 2, true, "#6c5ce7", "Plan profesional para empresas en crecimiento", 15, 30, "Pro", 49m, "Pro" },
                    { 3, true, "#00d4aa", "Plan empresarial sin límites", 100, 500, "Enterprise", 199m, "Enterprise" }
                });

            migrationBuilder.InsertData(
                schema: "saas",
                table: "Tenants",
                columns: new[] { "Id", "Activo", "Direccion", "EmailContacto", "FechaCreacion", "FechaDesactivacion", "LogoUrl", "Nombre", "RUC", "Telefono", "TotalMantenimientos", "TotalUsuarios", "TotalVehiculos" },
                values: new object[,]
                {
                    { 1, true, null, "contacto@transportesrapidos.com", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Transportes Rápidos SAC", "20123456789", "01-2345678", 45, 15, 30 },
                    { 2, true, null, "admin@logisticaexpress.pe", new DateTime(2024, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Logística Express EIRL", "20234567890", "01-3456789", 12, 5, 10 },
                    { 3, true, null, "operaciones@cargapesada.com", new DateTime(2024, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Carga Pesada Corp", "20345678901", null, 8, 3, 5 },
                    { 4, false, null, "ventas@limanorte.com", new DateTime(2024, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Distribuidora Lima Norte", "20456789012", "01-4567890", 20, 8, 15 },
                    { 5, true, null, "info@mudanzasperu.pe", new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Mudanzas Perú SRL", "20567890123", null, 320, 100, 450 }
                });

            migrationBuilder.InsertData(
                schema: "saas",
                table: "TenantSubscriptions",
                columns: new[] { "Id", "FechaFin", "FechaInicio", "SubscriptionPlanId", "TenantId" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 1 },
                    { 2, null, new DateTime(2024, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 2 },
                    { 3, null, new DateTime(2024, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 3 },
                    { 4, new DateTime(2024, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 4 },
                    { 5, null, new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Nombre",
                schema: "saas",
                table: "SubscriptionPlans",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_RUC",
                schema: "saas",
                table: "Tenants",
                column: "RUC",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubscriptions_SubscriptionPlanId",
                schema: "saas",
                table: "TenantSubscriptions",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubscriptions_TenantId",
                schema: "saas",
                table: "TenantSubscriptions",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantSubscriptions",
                schema: "saas");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans",
                schema: "saas");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "saas");
        }
    }
}
