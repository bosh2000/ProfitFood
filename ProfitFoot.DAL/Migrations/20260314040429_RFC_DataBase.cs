using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfitFood.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RFC_DataBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_BaseUnitsStorage_BaseUnitStorageId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_BaseUnits_BaseUnitId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductGroups_GroupId",
                table: "Products");

            //  migrationBuilder.DropTable(
            //    name: "BaseUnits");

            //migrationBuilder.DropTable(
            //    name: "BaseUnitsStorage");

            //migrationBuilder.DropTable(
            //    name: "ProductGroups");

            migrationBuilder.DropIndex(
                name: "IX_Products_BaseUnitStorageId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_GroupId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BaseUnitStorageId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "Products",
                newName: "ProductCategoryId");

            migrationBuilder.AddColumn<string>(
                name: "Article",
                table: "Products",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPerishable",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MinStock",
                table: "Products",
                type: "TEXT",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "StorageUnitId",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AgeGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AgeFromMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    AgeToMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgeGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OldValueJson = table.Column<string>(type: "TEXT", nullable: false),
                    NewValueJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DatabaseFilePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    AutoMigrateOnStartup = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreateBackupBeforeMigration = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastBackupAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DishCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MealTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MonthFrom = table.Column<int>(type: "INTEGER", nullable: false),
                    MonthTo = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.CheckConstraint("CK_Seasons_MonthFrom", "MonthFrom >= 1 AND MonthFrom <= 12");
                    table.CheckConstraint("CK_Seasons_MonthTo", "MonthTo >= 1 AND MonthTo <= 12");
                });

            migrationBuilder.CreateTable(
                name: "Signers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Position = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StorageLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UnitType = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseFactor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    IsBase = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChildGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AgeGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlannedChildrenCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildGroups_AgeGroups_AgeGroupId",
                        column: x => x.AgeGroupId,
                        principalTable: "AgeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    DishCategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MealTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dishes_DishCategories_DishCategoryId",
                        column: x => x.DishCategoryId,
                        principalTable: "DishCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dishes_MealTypes_MealTypeId",
                        column: x => x.MealTypeId,
                        principalTable: "MealTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CycleMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    SeasonId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AgeGroupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DaysCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DateFrom = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    DateTo = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CycleMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CycleMenus_AgeGroups_AgeGroupId",
                        column: x => x.AgeGroupId,
                        principalTable: "AgeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CycleMenus_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InstitutionName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    InstitutionShortName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DefaultStorageLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Form299Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    DirectorName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSettings_StorageLocations_DefaultStorageLocationId",
                        column: x => x.DefaultStorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    UnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockBalances_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockBalances_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockBalances_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecipeCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DishId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CardNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeCards_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CycleMenuDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CycleMenuId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DayNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CycleMenuDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CycleMenuDays_CycleMenus_CycleMenuId",
                        column: x => x.CycleMenuId,
                        principalTable: "CycleMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MenuDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    SeasonId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CycleMenuId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyMenus_CycleMenus_CycleMenuId",
                        column: x => x.CycleMenuId,
                        principalTable: "CycleMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyMenus_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecipeCardVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeCardId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DateFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DateTo = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    OutputQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    OutputUnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeCardVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeCardVersions_RecipeCards_RecipeCardId",
                        column: x => x.RecipeCardId,
                        principalTable: "RecipeCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeCardVersions_Units_OutputUnitId",
                        column: x => x.OutputUnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyMenuAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DailyMenuId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChildGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlannedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ActualCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyMenuAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyMenuAttendances_ChildGroups_ChildGroupId",
                        column: x => x.ChildGroupId,
                        principalTable: "ChildGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyMenuAttendances_DailyMenus_DailyMenuId",
                        column: x => x.DailyMenuId,
                        principalTable: "DailyMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RequirementDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DailyMenuId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalChildrenCount = table.Column<int>(type: "INTEGER", nullable: false),
                    HeadSignerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StorekeeperSignerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MedWorkerSignerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PrintedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuRequirements_DailyMenus_DailyMenuId",
                        column: x => x.DailyMenuId,
                        principalTable: "DailyMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuRequirements_Signers_HeadSignerId",
                        column: x => x.HeadSignerId,
                        principalTable: "Signers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuRequirements_Signers_MedWorkerSignerId",
                        column: x => x.MedWorkerSignerId,
                        principalTable: "Signers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuRequirements_Signers_StorekeeperSignerId",
                        column: x => x.StorekeeperSignerId,
                        principalTable: "Signers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuRequirements_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CycleMenuItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CycleMenuDayId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MealTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DishId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeCardVersionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CycleMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CycleMenuItems_CycleMenuDays_CycleMenuDayId",
                        column: x => x.CycleMenuDayId,
                        principalTable: "CycleMenuDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CycleMenuItems_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CycleMenuItems_MealTypes_MealTypeId",
                        column: x => x.MealTypeId,
                        principalTable: "MealTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CycleMenuItems_RecipeCardVersions_RecipeCardVersionId",
                        column: x => x.RecipeCardVersionId,
                        principalTable: "RecipeCardVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyMenuItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DailyMenuId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MealTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DishId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeCardVersionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyMenuItems_DailyMenus_DailyMenuId",
                        column: x => x.DailyMenuId,
                        principalTable: "DailyMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyMenuItems_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyMenuItems_MealTypes_MealTypeId",
                        column: x => x.MealTypeId,
                        principalTable: "MealTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyMenuItems_RecipeCardVersions_RecipeCardVersionId",
                        column: x => x.RecipeCardVersionId,
                        principalTable: "RecipeCardVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DishOutputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeCardVersionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgeGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OutputQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    UnitId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishOutputs_AgeGroups_AgeGroupId",
                        column: x => x.AgeGroupId,
                        principalTable: "AgeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DishOutputs_RecipeCardVersions_RecipeCardVersionId",
                        column: x => x.RecipeCardVersionId,
                        principalTable: "RecipeCardVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DishOutputs_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeCardVersionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrossQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    NetQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    UnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LossPercent = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_RecipeCardVersions_RecipeCardVersionId",
                        column: x => x.RecipeCardVersionId,
                        principalTable: "RecipeCardVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MenuRequirementItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MenuRequirementId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuRequirementItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuRequirementItems_MenuRequirements_MenuRequirementId",
                        column: x => x.MenuRequirementId,
                        principalTable: "MenuRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuRequirementItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuRequirementItems_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentType = table.Column<int>(type: "INTEGER", nullable: false),
                    DocumentNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DocumentDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelatedMenuRequirementId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockDocuments_MenuRequirements_RelatedMenuRequirementId",
                        column: x => x.RelatedMenuRequirementId,
                        principalTable: "MenuRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockDocuments_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockDocumentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StockDocumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    UnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockDocumentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockDocumentItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockDocumentItems_StockDocuments_StockDocumentId",
                        column: x => x.StockDocumentId,
                        principalTable: "StockDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockDocumentItems_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCategoryId_Name",
                table: "Products",
                columns: new[] { "ProductCategoryId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_StorageUnitId",
                table: "Products",
                column: "StorageUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AgeGroups_Name",
                table: "AgeGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_DefaultStorageLocationId",
                table: "AppSettings",
                column: "DefaultStorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId_ChangedAt",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChildGroups_AgeGroupId",
                table: "ChildGroups",
                column: "AgeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildGroups_Name",
                table: "ChildGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CycleMenuDays_CycleMenuId_DayNumber",
                table: "CycleMenuDays",
                columns: new[] { "CycleMenuId", "DayNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CycleMenuItems_CycleMenuDayId_MealTypeId_SortOrder",
                table: "CycleMenuItems",
                columns: new[] { "CycleMenuDayId", "MealTypeId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CycleMenuItems_DishId",
                table: "CycleMenuItems",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_CycleMenuItems_MealTypeId",
                table: "CycleMenuItems",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CycleMenuItems_RecipeCardVersionId",
                table: "CycleMenuItems",
                column: "RecipeCardVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CycleMenus_AgeGroupId",
                table: "CycleMenus",
                column: "AgeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CycleMenus_SeasonId_AgeGroupId_Name",
                table: "CycleMenus",
                columns: new[] { "SeasonId", "AgeGroupId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyMenuAttendances_ChildGroupId",
                table: "DailyMenuAttendances",
                column: "ChildGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMenuAttendances_DailyMenuId_ChildGroupId",
                table: "DailyMenuAttendances",
                columns: new[] { "DailyMenuId", "ChildGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyMenuItems_DailyMenuId_MealTypeId_SortOrder",
                table: "DailyMenuItems",
                columns: new[] { "DailyMenuId", "MealTypeId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyMenuItems_DishId",
                table: "DailyMenuItems",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMenuItems_MealTypeId",
                table: "DailyMenuItems",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMenuItems_RecipeCardVersionId",
                table: "DailyMenuItems",
                column: "RecipeCardVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMenus_CycleMenuId",
                table: "DailyMenus",
                column: "CycleMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMenus_MenuDate",
                table: "DailyMenus",
                column: "MenuDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyMenus_SeasonId",
                table: "DailyMenus",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_DishCategories_Name",
                table: "DishCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_Code",
                table: "Dishes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_DishCategoryId",
                table: "Dishes",
                column: "DishCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_MealTypeId",
                table: "Dishes",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_Name",
                table: "Dishes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DishOutputs_AgeGroupId",
                table: "DishOutputs",
                column: "AgeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DishOutputs_RecipeCardVersionId_AgeGroupId",
                table: "DishOutputs",
                columns: new[] { "RecipeCardVersionId", "AgeGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DishOutputs_UnitId",
                table: "DishOutputs",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MealTypes_Code",
                table: "MealTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuRequirementItems_MenuRequirementId_SortOrder",
                table: "MenuRequirementItems",
                columns: new[] { "MenuRequirementId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuRequirementItems_ProductId",
                table: "MenuRequirementItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRequirementItems_UnitId",
                table: "MenuRequirementItems",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRequirements_DailyMenuId",
                table: "MenuRequirements",
                column: "DailyMenuId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuRequirements_HeadSignerId",
                table: "MenuRequirements",
                column: "HeadSignerId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRequirements_MedWorkerSignerId",
                table: "MenuRequirements",
                column: "MedWorkerSignerId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRequirements_Number_RequirementDate",
                table: "MenuRequirements",
                columns: new[] { "Number", "RequirementDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuRequirements_StorageLocationId",
                table: "MenuRequirements",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRequirements_StorekeeperSignerId",
                table: "MenuRequirements",
                column: "StorekeeperSignerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Name",
                table: "ProductCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentCategoryId",
                table: "ProductCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeCards_CardNumber",
                table: "RecipeCards",
                column: "CardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeCards_DishId",
                table: "RecipeCards",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeCardVersions_OutputUnitId",
                table: "RecipeCardVersions",
                column: "OutputUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeCardVersions_RecipeCardId_VersionNumber",
                table: "RecipeCardVersions",
                columns: new[] { "RecipeCardId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_ProductId",
                table: "RecipeIngredients",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_RecipeCardVersionId_SortOrder",
                table: "RecipeIngredients",
                columns: new[] { "RecipeCardVersionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_UnitId",
                table: "RecipeIngredients",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_Name",
                table: "Seasons",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Signers_FullName_Position",
                table: "Signers",
                columns: new[] { "FullName", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_ProductId",
                table: "StockBalances",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_StorageLocationId_ProductId",
                table: "StockBalances",
                columns: new[] { "StorageLocationId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_UnitId",
                table: "StockBalances",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocumentItems_ProductId",
                table: "StockDocumentItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocumentItems_StockDocumentId",
                table: "StockDocumentItems",
                column: "StockDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocumentItems_UnitId",
                table: "StockDocumentItems",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_DocumentNumber_DocumentDate_StorageLocationId",
                table: "StockDocuments",
                columns: new[] { "DocumentNumber", "DocumentDate", "StorageLocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_RelatedMenuRequirementId",
                table: "StockDocuments",
                column: "RelatedMenuRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_StorageLocationId",
                table: "StockDocuments",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_Code",
                table: "StorageLocations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_Name",
                table: "Units",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_ShortName",
                table: "Units",
                column: "ShortName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_BaseUnitId",
                table: "Products",
                column: "BaseUnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_StorageUnitId",
                table: "Products",
                column: "StorageUnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_BaseUnitId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_StorageUnitId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CycleMenuItems");

            migrationBuilder.DropTable(
                name: "DailyMenuAttendances");

            migrationBuilder.DropTable(
                name: "DailyMenuItems");

            migrationBuilder.DropTable(
                name: "DatabaseSettings");

            migrationBuilder.DropTable(
                name: "DishOutputs");

            migrationBuilder.DropTable(
                name: "MenuRequirementItems");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "StockBalances");

            migrationBuilder.DropTable(
                name: "StockDocumentItems");

            migrationBuilder.DropTable(
                name: "CycleMenuDays");

            migrationBuilder.DropTable(
                name: "ChildGroups");

            migrationBuilder.DropTable(
                name: "RecipeCardVersions");

            migrationBuilder.DropTable(
                name: "StockDocuments");

            migrationBuilder.DropTable(
                name: "RecipeCards");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "MenuRequirements");

            migrationBuilder.DropTable(
                name: "Dishes");

            migrationBuilder.DropTable(
                name: "DailyMenus");

            migrationBuilder.DropTable(
                name: "Signers");

            migrationBuilder.DropTable(
                name: "StorageLocations");

            migrationBuilder.DropTable(
                name: "DishCategories");

            migrationBuilder.DropTable(
                name: "MealTypes");

            migrationBuilder.DropTable(
                name: "CycleMenus");

            migrationBuilder.DropTable(
                name: "AgeGroups");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_Products_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCategoryId_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_StorageUnitId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Article",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsPerishable",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MinStock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StorageUnitId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "ProductCategoryId",
                table: "Products",
                newName: "GroupId");

            migrationBuilder.AddColumn<Guid>(
                name: "BaseUnitStorageId",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BaseUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseUnitsStorage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseUnitsStorage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BaseUnitStorageId",
                table: "Products",
                column: "BaseUnitStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_GroupId",
                table: "Products",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_BaseUnitsStorage_BaseUnitStorageId",
                table: "Products",
                column: "BaseUnitStorageId",
                principalTable: "BaseUnitsStorage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_BaseUnits_BaseUnitId",
                table: "Products",
                column: "BaseUnitId",
                principalTable: "BaseUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductGroups_GroupId",
                table: "Products",
                column: "GroupId",
                principalTable: "ProductGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}