using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace nsia.Models
{
    public class Application
    {
        // ======================
        // IDENTITY & STATUS
        // ======================

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int ApplicationStep { get; set; } = 1;

        [MaxLength(20)]
        public string? ReferenceNumber { get; set; }

        [Required, MaxLength(30)]
        public string Status { get; set; } = "Draft";

        // ======================
        // SECTION A – Applicant Profile
        // ======================

        [Required, MaxLength(100)]
        public string FullName { get; set; } = default!;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = default!;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = default!;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = default!;

        [MaxLength(20)]
        public string? Gender { get; set; }

        public bool IsEmailVerified { get; set; } = false;

        public string? EmailVerificationOtp { get; set; }

        public DateTime? OtpExpiresAt { get; set; }

        // ======================
        // SECTION B – Company Info
        // ======================

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(500)]
        public string? CompanyUrl { get; set; }

        public SocialMedia SocialMedia { get; set; } = new();

        [Column(TypeName = "text")]
        public string? CompanyDescription { get; set; }

        [MaxLength(300)]
        public string? BusinessAddress { get; set; }

        public bool IsLegallyRegisteredInNigeria { get; set; }

        public int? YearOfIncorporation { get; set; }

        [MaxLength(30)]
        public string? CacRegistrationNumber { get; set; }

        public bool HasForeignAffiliates { get; set; }

        [Column(TypeName = "text")]
        public string? ForeignAffiliateDetails { get; set; }

        public bool IsNigerianEntityOperational { get; set; }

        /// <summary>
        /// Comma-separated milestone option values e.g. "A,C"
        /// </summary>
        [MaxLength(20)]
        public string? Milestones { get; set; }

        /// <summary>
        /// Comma-separated success metric option values e.g. "B,D"
        /// </summary>
        [MaxLength(20)]
        public string? SuccessMetrics { get; set; }

        /// <summary>
        /// Comma-separated vision option values e.g. "A,B"
        /// </summary>
        [MaxLength(20)]
        public string? LongTermVision { get; set; }

        // ======================
        // SECTION C – Team Info
        // ======================

        public int? NumberOfFounders { get; set; }

        /// <summary>
        /// Comma-separated team composition option values e.g. "A,D"
        /// </summary>
        [MaxLength(20)]
        public string? TeamComposition { get; set; }

        public int? TotalEmployees { get; set; }

        /// <summary>
        /// Navigation property — founder records stored in separate table
        /// </summary>
        public List<Founder> Founders { get; set; } = new();

        // ======================
        // SECTION D – Product Info
        // ======================

        [MaxLength(30)]
        public string? GrowthStage { get; set; }

        [MaxLength(500)]
        public string? MvpLink { get; set; }

        [MaxLength(50)]
        public string? Sector { get; set; }

        [Column(TypeName = "text")]
        public string? ProductDescription { get; set; }

        [MaxLength(30)]
        public string? UserCountRange { get; set; }

        /// <summary>
        /// Comma-separated business model option values e.g. "A,C"
        /// </summary>
        [MaxLength(20)]
        public string? BusinessModel { get; set; }

        /// <summary>
        /// Comma-separated USP option values
        /// </summary>
        [MaxLength(20)]
        public string? UniqueSellingPoint { get; set; }

        /// <summary>
        /// Comma-separated competitor option values
        /// </summary>
        [MaxLength(20)]
        public string? MainCompetitors { get; set; }

        /// <summary>
        /// Comma-separated go-to-market option values
        /// </summary>
        [MaxLength(20)]
        public string? GoToMarketStrategy { get; set; }

        /// <summary>
        /// Comma-separated key feature option values
        /// </summary>
        [MaxLength(20)]
        public string? KeyFeatures { get; set; }

        // ======================
        // SECTION E – Commercial
        // ======================

        public bool? HasStartedGeneratingRevenue { get; set; }

        /// <summary>
        /// Comma-separated funding type option values e.g. "A,B"
        /// </summary>
        [MaxLength(50)]
        public string? FundingTypes { get; set; }

        public bool? IsCurrentlyFundraising { get; set; }

        [MaxLength(50)]
        public string? CompanyValuation { get; set; }

        [MaxLength(50)]
        public string? ProjectedRevenueNextYear { get; set; }

        [MaxLength(20)]
        public string? GrossMargins { get; set; }

        [MaxLength(50)]
        public string? PricingStrategy { get; set; }

        [MaxLength(20)]
        public string? RevenueStreams { get; set; }

        [MaxLength(20)]
        public string? RepeatCustomerRevenuePercentage { get; set; }

        [MaxLength(20)]
        public string? OperatingRunway { get; set; }

        [MaxLength(50)]
        public string? DemandEvidence { get; set; }

        [MaxLength(20)]
        public string? EstimatedMarketShare { get; set; }

        [MaxLength(50)]
        public string? PrimaryCompetitiveEdge { get; set; }

        [MaxLength(50)]
        public string? GeographicScalability { get; set; }

        [MaxLength(50)]
        public string? CrossIndustryApplicability { get; set; }

        [MaxLength(50)]
        public string? LongTermGrowthPlan { get; set; }

        [MaxLength(30)]
        public string? FeedbackUpdateFrequency { get; set; }

        [MaxLength(20)]
        public string? CustomersAcquiredPastSixMonths { get; set; }

        [MaxLength(20)]
        public string? CustomerGrowthRatePastYear { get; set; }

        [MaxLength(30)]
        public string? AverageCustomerAcquisitionCost { get; set; }

        [MaxLength(50)]
        public string? SupplyChainReliability { get; set; }

        [MaxLength(50)]
        public string? RegulatoryCompliance { get; set; }

        /// <summary>
        /// Comma-separated risk option values e.g. "A,C"
        /// </summary>
        [MaxLength(50)]
        public string? BiggestRisks { get; set; }

        [MaxLength(50)]
        public string? ActivePartnerships { get; set; }

        [MaxLength(50)]
        public string? IpOwnership { get; set; }

        // ======================
        // SECTION F – Impact & Sustainability
        // ======================

        // SDG
        public bool? AlignsWithUnSdgs { get; set; }

        [MaxLength(10)]
        public string? SdgsAddressed { get; set; }

        // Environmental
        public bool? ReducesEnvironmentalHarm { get; set; }

        [MaxLength(60)]
        public string? EnvironmentalHarmReduction { get; set; }

        [MaxLength(50)]
        public string? ResourceOptimisation { get; set; }

        // Social & Inclusion
        [MaxLength(20)]
        public string? UnderservedMarketPercentage { get; set; }

        [MaxLength(50)]
        public string? SystemicInequalityReduction { get; set; }

        [MaxLength(60)]
        public string? GenderGapApproach { get; set; }

        [MaxLength(60)]
        public string? AccessForUnderservedGroups { get; set; }

        [MaxLength(30)]
        public string? JobsCreated { get; set; }

        [MaxLength(30)]
        public string? PeopleImpacted { get; set; }

        [MaxLength(50)]
        public string? UserSelfRelianceLevel { get; set; }

        // Measurement & Governance
        [MaxLength(50)]
        public string? SocialOutcomeTracking { get; set; }

        [MaxLength(60)]
        public string? ImpactMeasurementMethod { get; set; }

        [MaxLength(50)]
        public string? ImpactDataSharingLevel { get; set; }

        [MaxLength(60)]
        public string? BeneficiaryInvolvementLevel { get; set; }

        [MaxLength(50)]
        public string? LocalContextTailoring { get; set; }

        [MaxLength(60)]
        public string? EthicalPracticesApproach { get; set; }

        [MaxLength(50)]
        public string? DataProtectionApproach { get; set; }

        [MaxLength(60)]
        public string? TrustBuildingApproach { get; set; }

        // Scalability & Longevity
        [MaxLength(50)]
        public string? ModelReplicability { get; set; }

        [MaxLength(60)]
        public string? ImpactDurability { get; set; }

        [MaxLength(50)]
        public string? CrisisPerformance { get; set; }

        [MaxLength(50)]
        public string? PolicyAdvocacy { get; set; }

        // Open-text evidence
        [Column(TypeName = "text")]
        public string? ImpactDataAndStatistics { get; set; }

        [Column(TypeName = "text")]
        public string? MeasurableCommunityDifferences { get; set; }
        [Column(TypeName = "text")]
        public string? TopImpactExamplesDetails { get; set; }

        // Declarations
        public bool AgreesToNsiaPrivacyPolicy { get; set; }

        public bool AgreesToCompetitionSubmissionAgreement { get; set; }

        // ======================
        // SECTION G – Additional Information
        // (populated in Step 7)
        // ======================

        [MaxLength(3000)]
        public string? AdditionalInformation { get; set; }
        [Column(TypeName = "text")]
        public string? DocumentDetails { get; set; }
        public List<ApplicationDocument> Documents { get; set; } = new();

        // ======================
        // AUDITING
        // ======================

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }
    }

    // ======================
    // FOUNDER — separate table
    // ======================

    public class Founder
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Application))]
        public Guid ApplicationId { get; set; }

        public Application Application { get; set; } = default!;

        [Required, MaxLength(100)]
        public string FullName { get; set; } = default!;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Role { get; set; }

        [MaxLength(500)]
        public string? LinkedInUrl { get; set; }

        [MaxLength(60)]
        public string? Nationality { get; set; }

        public int DisplayOrder { get; set; }
    }
    public class ApplicationDocument
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Application))]
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; } = default!;

        [MaxLength(255)]
        public string OriginalFileName { get; set; } = default!;

        [MaxLength(500)]
        public string StoredFilePath { get; set; } = default!;

        [MaxLength(30)]
        public string DocumentType { get; set; } = "Other";

        public long FileSizeBytes { get; set; }

        [MaxLength(20)]
        public string FileExtension { get; set; } = default!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
    // ======================
    // SOCIAL MEDIA — owned type
    // ======================

    [Owned]
    public class SocialMedia
    {
        [MaxLength(200)]
        public string? LinkedIn { get; set; }

        [MaxLength(100)]
        public string? Twitter { get; set; }

        [MaxLength(100)]
        public string? Instagram { get; set; }

        [MaxLength(200)]
        public string? Facebook { get; set; }
    }
}