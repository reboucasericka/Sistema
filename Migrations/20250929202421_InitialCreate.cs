using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasServicos",
                columns: table => new
                {
                    CategoriaServicoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasServicos", x => x.CategoriaServicoId);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    ClientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllergyHistory = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    ConfiguracaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeClinica = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TelefoneFixo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TelemovelWhatsApp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Icone = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogoRelatorio = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TipoRelatorio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TipoComissao = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    ComissaoPadraoExtensao = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ComissaoPadraoDesign = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PastaImagens = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HorarioFuncionamento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DuracaoPadraoServico = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.ConfiguracaoId);
                });

            migrationBuilder.CreateTable(
                name: "Esteticas",
                columns: table => new
                {
                    EsteticaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Endereco = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telemovel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Esteticas", x => x.EsteticaId);
                });

            migrationBuilder.CreateTable(
                name: "FormasPagamento",
                columns: table => new
                {
                    FormaPagamentoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormasPagamento", x => x.FormaPagamentoId);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Nif = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveryTime = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.SupplierId);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    ProductCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.ProductCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.ProfileId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServicoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CategoriaServicoId = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Foto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DiasRetorno = table.Column<int>(type: "int", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    Comissao = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServicoId);
                    table.ForeignKey(
                        name: "FK_Services_CategoriasServicos_CategoriaServicoId",
                        column: x => x.CategoriaServicoId,
                        principalTable: "CategoriasServicos",
                        principalColumn: "CategoriaServicoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Planos",
                columns: table => new
                {
                    PlanoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    QuantidadeSessoes = table.Column<int>(type: "int", nullable: false),
                    ValidadeDias = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planos", x => x.PlanoId);
                    table.ForeignKey(
                        name: "FK_Planos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClientId");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ImageId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 200, nullable: false),
                    ProfileId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "ProfileId");
                });

            migrationBuilder.CreateTable(
                name: "AvaliacoesAtendimento",
                columns: table => new
                {
                    AvaliacaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false),
                    DataAvaliacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Satisfacao = table.Column<int>(type: "int", nullable: false),
                    Comentarios = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacoesAtendimento", x => x.AvaliacaoId);
                    table.ForeignKey(
                        name: "FK_AvaliacoesAtendimento_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvaliacoesAtendimento_Services_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Services",
                        principalColumn: "ServicoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Caixa",
                columns: table => new
                {
                    CaixaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "date", nullable: false),
                    ValorInicial = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValorFinal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UsuarioAbertura = table.Column<int>(type: "int", nullable: false),
                    UsuarioAberturaRefId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UsuarioFechamento = table.Column<int>(type: "int", nullable: true),
                    UsuarioFechamentoRefId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caixa", x => x.CaixaId);
                    table.ForeignKey(
                        name: "FK_Caixa_AspNetUsers_UsuarioAberturaRefId",
                        column: x => x.UsuarioAberturaRefId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Caixa_AspNetUsers_UsuarioFechamentoRefId",
                        column: x => x.UsuarioFechamentoRefId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Faturacao",
                columns: table => new
                {
                    FaturacaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "date", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    QuantidadeServicos = table.Column<int>(type: "int", nullable: false),
                    QuantidadeProdutos = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faturacao", x => x.FaturacaoId);
                    table.ForeignKey(
                        name: "FK_Faturacao_AspNetUsers_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    ImageId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 200, nullable: false),
                    MinimumStockLevel = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Produtos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Produtos_Fornecedores_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Fornecedores",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_Produtos_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Profissionais",
                columns: table => new
                {
                    ProfissionalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId1 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Especialidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ComissaoPadrao = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profissionais", x => x.ProfissionalId);
                    table.ForeignKey(
                        name: "FK_Profissionais_AspNetUsers_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistroAcessos",
                columns: table => new
                {
                    RegistroAcessosId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Acao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistroAcessos", x => x.RegistroAcessosId);
                    table.ForeignKey(
                        name: "FK_RegistroAcessos_AspNetUsers_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EstoqueEntradas",
                columns: table => new
                {
                    EntradaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FornecedorId = table.Column<int>(type: "int", nullable: true),
                    Data = table.Column<DateTime>(type: "date", nullable: false),
                    TipoMovimentacao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Lote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DataValidade = table.Column<DateTime>(type: "date", nullable: true),
                    ValorUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstoqueEntradas", x => x.EntradaId);
                    table.ForeignKey(
                        name: "FK_EstoqueEntradas_AspNetUsers_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EstoqueEntradas_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_EstoqueEntradas_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstoqueMovimentacoes",
                columns: table => new
                {
                    MovimentacaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    TipoMovimentacao = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FornecedorId = table.Column<int>(type: "int", nullable: true),
                    DataMovimentacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataValidade = table.Column<DateTime>(type: "date", nullable: true),
                    Lote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstoqueMovimentacoes", x => x.MovimentacaoId);
                    table.ForeignKey(
                        name: "FK_EstoqueMovimentacoes_AspNetUsers_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EstoqueMovimentacoes_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_EstoqueMovimentacoes_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagar",
                columns: table => new
                {
                    PagarId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DataLanc = table.Column<DateTime>(type: "date", nullable: false),
                    DataVenc = table.Column<DateTime>(type: "date", nullable: false),
                    DataPgto = table.Column<DateTime>(type: "date", nullable: true),
                    UsuarioLanc = table.Column<int>(type: "int", nullable: false),
                    UsuarioLancadorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UsuarioBaixa = table.Column<int>(type: "int", nullable: true),
                    UsuarioBaixadorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Foto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PessoaId = table.Column<int>(type: "int", nullable: true),
                    Pago = table.Column<bool>(type: "bit", nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    FormaPagamentoId = table.Column<int>(type: "int", nullable: true),
                    Parcela = table.Column<int>(type: "int", nullable: false),
                    TotalParcelas = table.Column<int>(type: "int", nullable: false),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagar", x => x.PagarId);
                    table.ForeignKey(
                        name: "FK_Pagar_AspNetUsers_UsuarioBaixadorId",
                        column: x => x.UsuarioBaixadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pagar_AspNetUsers_UsuarioLancadorId",
                        column: x => x.UsuarioLancadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pagar_FormasPagamento_FormaPagamentoId",
                        column: x => x.FormaPagamentoId,
                        principalTable: "FormasPagamento",
                        principalColumn: "FormaPagamentoId");
                    table.ForeignKey(
                        name: "FK_Pagar_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "Receber",
                columns: table => new
                {
                    ReceberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DataLanc = table.Column<DateTime>(type: "date", nullable: false),
                    DataVenc = table.Column<DateTime>(type: "date", nullable: false),
                    DataPgto = table.Column<DateTime>(type: "date", nullable: true),
                    UsuarioLanc = table.Column<int>(type: "int", nullable: false),
                    UsuarioLancadorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UsuarioBaixa = table.Column<int>(type: "int", nullable: true),
                    UsuarioBaixadorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Foto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PessoaId = table.Column<int>(type: "int", nullable: true),
                    Pago = table.Column<bool>(type: "bit", nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    FormaPagamentoId = table.Column<int>(type: "int", nullable: true),
                    Parcela = table.Column<int>(type: "int", nullable: false),
                    TotalParcelas = table.Column<int>(type: "int", nullable: false),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receber", x => x.ReceberId);
                    table.ForeignKey(
                        name: "FK_Receber_AspNetUsers_UsuarioBaixadorId",
                        column: x => x.UsuarioBaixadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Receber_AspNetUsers_UsuarioLancadorId",
                        column: x => x.UsuarioLancadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Receber_FormasPagamento_FormaPagamentoId",
                        column: x => x.FormaPagamentoId,
                        principalTable: "FormasPagamento",
                        principalColumn: "FormaPagamentoId");
                    table.ForeignKey(
                        name: "FK_Receber_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "Agendamentos",
                columns: table => new
                {
                    AgendamentoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false),
                    ProfissionalId = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateTime>(type: "date", nullable: false),
                    Horario = table.Column<TimeSpan>(type: "time", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LembreteEnviado = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendamentos", x => x.AgendamentoId);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "ProfissionalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Services_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Services",
                        principalColumn: "ServicoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoProcedimentos",
                columns: table => new
                {
                    ProcedimentoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false),
                    ProfissionalId = table.Column<int>(type: "int", nullable: false),
                    DataProcedimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaterialUsado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ObservacoesTecnicas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoProcedimentos", x => x.ProcedimentoId);
                    table.ForeignKey(
                        name: "FK_HistoricoProcedimentos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricoProcedimentos_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "ProfissionalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricoProcedimentos_Services_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Services",
                        principalColumn: "ServicoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Horarios",
                columns: table => new
                {
                    HorarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfissionalId = table.Column<int>(type: "int", nullable: false),
                    Hora = table.Column<TimeSpan>(type: "time", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: true),
                    DataEspecifica = table.Column<DateTime>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horarios", x => x.HorarioId);
                    table.ForeignKey(
                        name: "FK_Horarios_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "ProfissionalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfissionalServicos",
                columns: table => new
                {
                    ProfissionalServicoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfissionalId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false),
                    Comissao = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfissionalServicos", x => x.ProfissionalServicoId);
                    table.ForeignKey(
                        name: "FK_ProfissionalServicos_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "ProfissionalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfissionalServicos_Services_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Services",
                        principalColumn: "ServicoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstoqueSaidas",
                columns: table => new
                {
                    SaidaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AgendamentoId = table.Column<int>(type: "int", nullable: true),
                    Data = table.Column<DateTime>(type: "date", nullable: false),
                    TipoMovimentacao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstoqueSaidas", x => x.SaidaId);
                    table.ForeignKey(
                        name: "FK_EstoqueSaidas_Agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "Agendamentos",
                        principalColumn: "AgendamentoId");
                    table.ForeignKey(
                        name: "FK_EstoqueSaidas_AspNetUsers_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EstoqueSaidas_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lembretes",
                columns: table => new
                {
                    LembreteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgendamentoId = table.Column<int>(type: "int", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MeioEnvio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExportadoExcel = table.Column<bool>(type: "bit", nullable: false),
                    ExportadoPdf = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lembretes", x => x.LembreteId);
                    table.ForeignKey(
                        name: "FK_Lembretes_Agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "Agendamentos",
                        principalColumn: "AgendamentoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanoAgendamentos",
                columns: table => new
                {
                    PlanoAgendamentoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanoId = table.Column<int>(type: "int", nullable: false),
                    AgendamentoId = table.Column<int>(type: "int", nullable: false),
                    SessaoUsada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanoAgendamentos", x => x.PlanoAgendamentoId);
                    table.ForeignKey(
                        name: "FK_PlanoAgendamentos_Agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "Agendamentos",
                        principalColumn: "AgendamentoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanoAgendamentos_Planos_PlanoId",
                        column: x => x.PlanoId,
                        principalTable: "Planos",
                        principalColumn: "PlanoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_ClienteId",
                table: "Agendamentos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_ProfissionalId",
                table: "Agendamentos",
                column: "ProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_ServicoId",
                table: "Agendamentos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ProfileId",
                table: "AspNetUsers",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesAtendimento_ClienteId",
                table: "AvaliacoesAtendimento",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesAtendimento_ServicoId",
                table: "AvaliacoesAtendimento",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Caixa_UsuarioAberturaRefId",
                table: "Caixa",
                column: "UsuarioAberturaRefId");

            migrationBuilder.CreateIndex(
                name: "IX_Caixa_UsuarioFechamentoRefId",
                table: "Caixa",
                column: "UsuarioFechamentoRefId");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueEntradas_FornecedorId",
                table: "EstoqueEntradas",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueEntradas_ProdutoId",
                table: "EstoqueEntradas",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueEntradas_UsuarioId1",
                table: "EstoqueEntradas",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueMovimentacoes_FornecedorId",
                table: "EstoqueMovimentacoes",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueMovimentacoes_ProdutoId",
                table: "EstoqueMovimentacoes",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueMovimentacoes_UsuarioId1",
                table: "EstoqueMovimentacoes",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueSaidas_AgendamentoId",
                table: "EstoqueSaidas",
                column: "AgendamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueSaidas_ProdutoId",
                table: "EstoqueSaidas",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueSaidas_UsuarioId1",
                table: "EstoqueSaidas",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Faturacao_UsuarioId1",
                table: "Faturacao",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoProcedimentos_ClienteId",
                table: "HistoricoProcedimentos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoProcedimentos_ProfissionalId",
                table: "HistoricoProcedimentos",
                column: "ProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoProcedimentos_ServicoId",
                table: "HistoricoProcedimentos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Horarios_ProfissionalId",
                table: "Horarios",
                column: "ProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Lembretes_AgendamentoId",
                table: "Lembretes",
                column: "AgendamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagar_FormaPagamentoId",
                table: "Pagar",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagar_ProdutoId",
                table: "Pagar",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagar_UsuarioBaixadorId",
                table: "Pagar",
                column: "UsuarioBaixadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagar_UsuarioLancadorId",
                table: "Pagar",
                column: "UsuarioLancadorId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanoAgendamentos_AgendamentoId",
                table: "PlanoAgendamentos",
                column: "AgendamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanoAgendamentos_PlanoId",
                table: "PlanoAgendamentos",
                column: "PlanoId");

            migrationBuilder.CreateIndex(
                name: "IX_Planos_ClienteId",
                table: "Planos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_ProductCategoryId",
                table: "Produtos",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_SupplierId",
                table: "Produtos",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_UserId",
                table: "Produtos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Profissionais_UsuarioId1",
                table: "Profissionais",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProfissionalServicos_ProfissionalId",
                table: "ProfissionalServicos",
                column: "ProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfissionalServicos_ServicoId",
                table: "ProfissionalServicos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Receber_FormaPagamentoId",
                table: "Receber",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Receber_ProdutoId",
                table: "Receber",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Receber_UsuarioBaixadorId",
                table: "Receber",
                column: "UsuarioBaixadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Receber_UsuarioLancadorId",
                table: "Receber",
                column: "UsuarioLancadorId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroAcessos_UsuarioId1",
                table: "RegistroAcessos",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CategoriaServicoId",
                table: "Services",
                column: "CategoriaServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AvaliacoesAtendimento");

            migrationBuilder.DropTable(
                name: "Caixa");

            migrationBuilder.DropTable(
                name: "Configuracoes");

            migrationBuilder.DropTable(
                name: "Esteticas");

            migrationBuilder.DropTable(
                name: "EstoqueEntradas");

            migrationBuilder.DropTable(
                name: "EstoqueMovimentacoes");

            migrationBuilder.DropTable(
                name: "EstoqueSaidas");

            migrationBuilder.DropTable(
                name: "Faturacao");

            migrationBuilder.DropTable(
                name: "HistoricoProcedimentos");

            migrationBuilder.DropTable(
                name: "Horarios");

            migrationBuilder.DropTable(
                name: "Lembretes");

            migrationBuilder.DropTable(
                name: "Pagar");

            migrationBuilder.DropTable(
                name: "PlanoAgendamentos");

            migrationBuilder.DropTable(
                name: "ProfissionalServicos");

            migrationBuilder.DropTable(
                name: "Receber");

            migrationBuilder.DropTable(
                name: "RegistroAcessos");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Agendamentos");

            migrationBuilder.DropTable(
                name: "Planos");

            migrationBuilder.DropTable(
                name: "FormasPagamento");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Profissionais");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Fornecedores");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CategoriasServicos");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
