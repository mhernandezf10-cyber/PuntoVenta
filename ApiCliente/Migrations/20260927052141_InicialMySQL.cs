using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace ApiCliente.Migrations
{
    /// <inheritdoc />
    public partial class InicialMySQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id_cliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CUI = table.Column<string>(type: "varchar(13)", maxLength: 13, nullable: false),
                    NIT = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Nombres = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    Apellidos = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    Direccion = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Fecha_Nacimiento = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id_cliente);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
