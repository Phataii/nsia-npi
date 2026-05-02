using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nsia.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApplicationStep = table.Column<int>(type: "int", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "Draft")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gender = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEmailVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EmailVerificationOtp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OtpExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompanyName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompanyUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SocialMedia_LinkedIn = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SocialMedia_Twitter = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SocialMedia_Instagram = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SocialMedia_Facebook = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompanyDescription = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessAddress = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsLegallyRegisteredInNigeria = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    YearOfIncorporation = table.Column<int>(type: "int", nullable: true),
                    CacRegistrationNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasForeignAffiliates = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ForeignAffiliateDetails = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsNigerianEntityOperational = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Milestones = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SuccessMetrics = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LongTermVision = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumberOfFounders = table.Column<int>(type: "int", nullable: true),
                    TeamComposition = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalEmployees = table.Column<int>(type: "int", nullable: true),
                    GrowthStage = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MvpLink = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sector = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductDescription = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserCountRange = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessModel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UniqueSellingPoint = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MainCompetitors = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GoToMarketStrategy = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KeyFeatures = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasStartedGeneratingRevenue = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    FundingTypes = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCurrentlyFundraising = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CompanyValuation = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProjectedRevenueNextYear = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GrossMargins = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PricingStrategy = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RevenueStreams = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RepeatCustomerRevenuePercentage = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperatingRunway = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DemandEvidence = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstimatedMarketShare = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrimaryCompetitiveEdge = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeographicScalability = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CrossIndustryApplicability = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LongTermGrowthPlan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FeedbackUpdateFrequency = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomersAcquiredPastSixMonths = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerGrowthRatePastYear = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AverageCustomerAcquisitionCost = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SupplyChainReliability = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegulatoryCompliance = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BiggestRisks = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActivePartnerships = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpOwnership = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AlignsWithUnSdgs = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    SdgsAddressed = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReducesEnvironmentalHarm = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    EnvironmentalHarmReduction = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResourceOptimisation = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnderservedMarketPercentage = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SystemicInequalityReduction = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GenderGapApproach = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccessForUnderservedGroups = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JobsCreated = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PeopleImpacted = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserSelfRelianceLevel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SocialOutcomeTracking = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImpactMeasurementMethod = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImpactDataSharingLevel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeneficiaryInvolvementLevel = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LocalContextTailoring = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EthicalPracticesApproach = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataProtectionApproach = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrustBuildingApproach = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModelReplicability = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImpactDurability = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CrisisPerformance = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PolicyAdvocacy = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImpactDataAndStatistics = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MeasurableCommunityDifferences = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TopImpactExamplesDetails = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgreesToNsiaPrivacyPolicy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AgreesToCompetitionSubmissionAgreement = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AdditionalInformation = table.Column<string>(type: "text", maxLength: 3000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentDetails = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    SubmittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ApplicationDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApplicationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OriginalFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StoredFilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileExtension = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UploadedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationDocuments_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Founders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApplicationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkedInUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nationality = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Founders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Founders_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocuments_ApplicationId",
                table: "ApplicationDocuments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicationStep",
                table: "Applications",
                column: "ApplicationStep");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CreatedAt",
                table: "Applications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Email",
                table: "Applications",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ReferenceNumber",
                table: "Applications",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Status",
                table: "Applications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Founders_ApplicationId",
                table: "Founders",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Founders_ApplicationId_DisplayOrder",
                table: "Founders",
                columns: new[] { "ApplicationId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationDocuments");

            migrationBuilder.DropTable(
                name: "Founders");

            migrationBuilder.DropTable(
                name: "Applications");
        }
    }
}
