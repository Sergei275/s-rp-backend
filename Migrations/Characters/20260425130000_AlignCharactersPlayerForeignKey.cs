using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SRp.Data;

#nullable disable

namespace s_rp_backend.Migrations.Characters
{
    [DbContext(typeof(CharactersContext))]
    [Migration("20260425130000_AlignCharactersPlayerForeignKey")]
    public partial class AlignCharactersPlayerForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Player_PlayerId",
                table: "Characters");

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[Player]', N'U') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'[dbo].[Players]', N'U') IS NULL
                    BEGIN
                        EXEC sp_rename N'[dbo].[Player]', N'Players';
                    END
                    ELSE
                    BEGIN
                        SET IDENTITY_INSERT [dbo].[Players] ON;

                        INSERT INTO [dbo].[Players] ([Id], [SteamId64], [LastSeenAt], [CreatedAt])
                        SELECT [Id], [SteamId64], [LastSeenAt], [CreatedAt]
                        FROM [dbo].[Player] AS sourcePlayer
                        WHERE NOT EXISTS (
                            SELECT 1
                            FROM [dbo].[Players] AS targetPlayer
                            WHERE targetPlayer.[Id] = sourcePlayer.[Id]
                        );

                        SET IDENTITY_INSERT [dbo].[Players] OFF;

                        DROP TABLE [dbo].[Player];
                    END
                END
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Players_PlayerId",
                table: "Characters",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Players_PlayerId",
                table: "Characters");

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false),
                    LastSeenAt = table.Column<long>(type: "bigint", nullable: false),
                    SteamId64 = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[Players]', N'U') IS NOT NULL
                BEGIN
                    SET IDENTITY_INSERT [dbo].[Player] ON;

                    INSERT INTO [dbo].[Player] ([Id], [SteamId64], [LastSeenAt], [CreatedAt])
                    SELECT [Id], [SteamId64], [LastSeenAt], [CreatedAt]
                    FROM [dbo].[Players];

                    SET IDENTITY_INSERT [dbo].[Player] OFF;
                END
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Player_PlayerId",
                table: "Characters",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SRp.Models.Character", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Age")
                        .HasColumnType("int");

                    b.Property<long>("CharacterId")
                        .HasColumnType("bigint");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId", "CharacterId")
                        .IsUnique();

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("SRp.Models.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long>("CreatedAt")
                        .HasColumnType("bigint");

                    b.Property<long>("LastSeenAt")
                        .HasColumnType("bigint");

                    b.Property<long>("SteamId64")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("SRp.Models.Character", b =>
                {
                    b.HasOne("SRp.Models.Player", "CharacterOwner")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CharacterOwner");
                });
#pragma warning restore 612, 618
        }
    }
}
