using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace nsia.Models
{
    public class Application
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // ── Meta
        public int ApplicationStep { get; set; } = 1;


        public string Status { get; set; } = "Draft";


        public string? ReferenceNumber { get; set; }

        public bool IsEmailVerified { get; set; } = false;


        public string? EmailVerificationOtp { get; set; }

        public DateTime? OtpExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }

        // ── PRE-SUBMISSION CHECKLIST (Step 1)

        public string? IsRegisteredInNigeria { get; set; } // Yes / No


        public string? BusinessSector { get; set; } // Manufacturing / Healthcare / Climate & Food Security


        public string? CountryOfOrigin { get; set; }

        // ── PERSONAL INFORMATION (Step 2)

        public string? FullName { get; set; }
        public string? Email { get; set; }

        public string? PasswordHash { get; set; }


        public string? Phone { get; set; }


        public string? Gender { get; set; }

        // NIN stored encrypted

        public string? NinEncrypted { get; set; }


        public string? RelationshipToBusiness { get; set; } // Founder / Partner / Team Member / Agent

        // ── COMPANY INFORMATION (Step 3)

        public string? CompanyName { get; set; }


        public string? CompanyWebsite { get; set; }

        // Social media stored as owned type
        public SocialMedia? SocialMedia { get; set; }


        public string? BusinessState { get; set; }


        public string? BusinessLga { get; set; }


        public string? CompanyHqAddress { get; set; }


        public string? GeographicScope { get; set; } // State-wide / Regional / Nationwide / International


        public string? CompanyRegistrationNumber { get; set; }


        public string? RegulatoryCompliance { get; set; }


        public string? TaxCompliance { get; set; }


        public string? HasForeignAffiliates { get; set; } // Yes / No


        public string? IsNigerianEntityPrimary { get; set; } // Yes / No


        public string? CompanyStructure { get; set; } // Parent Company / Subsidiary


        public string? ParentOrganizationName { get; set; }


        public string? OtherCompetitions { get; set; } // free text or "No"

        // ── TEAM INFORMATION (Step 4)

        public string? NumberOfFounders { get; set; } // 1 / 2 / 3 / 4+


        public string? FoundingTeamType { get; set; } // comma-separated full text values


        public string? FounderIndustryExperience { get; set; }


        public string? ManagementTeamExperience { get; set; }


        public string? TotalFullTimeEmployees { get; set; }

        public List<Founder> Founders { get; set; } = new();

        // ── PRODUCT, GROWTH & TRACTION (Step 5)

        public string? GrowthStage { get; set; }


        public string? KeyMilestones { get; set; } // comma-separated full text values


        public string? ExistingUsers { get; set; }


        public string? TotalUsersReached { get; set; }


        public string? CoreBusinessModel { get; set; }


        public string? UniqueSellingPoint { get; set; }


        public string? MainCompetitors { get; set; }


        public string? MarketPenetrationStrategy { get; set; }


        public string? KeyFeatures { get; set; } // comma-separated full text values

        // ── COMMERCIAL PART 1 — REVENUE/FUNDING (Step 6)

        public string? HasStartedGeneratingSales { get; set; }


        public string? YearOfFirstSale { get; set; }


        public string? YearlySalesRevenue { get; set; }


        public string? YearlyProfit { get; set; }


        public string? ProprietaryFunding { get; set; }


        public string? ExternalFunding { get; set; }


        public string? TypesOfFunding { get; set; } // comma-separated full text values


        public string? IsCurrentlyFundraising { get; set; }


        public string? ProjectedRevenue { get; set; }


        public string? CompanyValuation { get; set; }

        // ── COMMERCIAL PART 2 — STRATEGY/MARKET (Step 7)

        public string? DemandEvidence { get; set; }


        public string? RevenueStreams { get; set; }


        public string? GeographicScalability { get; set; }


        public string? GrossMargins { get; set; }


        public string? PrimaryCompetitiveAdvantage { get; set; }


        public string? OperatingRunway { get; set; }


        public string? ActivePartnerships { get; set; }


        public string? RegulatoryApproach { get; set; }


        public string? CrossIndustryApplication { get; set; }


        public string? LongTermGrowthStrategy { get; set; }


        public string? SupplyChainReliability { get; set; }


        public string? IpOwnership { get; set; }


        public string? PricingStrategy { get; set; }


        public string? BiggestRisks { get; set; }


        public string? NewCustomersSixMonths { get; set; }


        public string? CustomerGrowthRate { get; set; }


        public string? AverageCAC { get; set; }


        public string? RepeatCustomerRevenue { get; set; }

        // ── SUSTAINABILITY (Step 8)

        public string? SdgAlignment { get; set; } // comma-separated SDG names


        public string? BusinessReplicability { get; set; }


        public string? SustainabilityIntegration { get; set; }


        public string? EnergyWasteReduction { get; set; }


        public string? SustainabilityTechnology { get; set; }


        public string? ScalingWithSustainability { get; set; }


        public string? ClimateChangeApproach { get; set; }


        public string? DigitalAccessibility { get; set; }

        // ── IMPACT (Step 9)

        public string? UnderservedMarketPercentage { get; set; }


        public string? SystemicInequalityApproach { get; set; }


        public string? BeneficiaryInvolvement { get; set; }


        public string? ImpactDataSharing { get; set; }


        public string? JobsCreated { get; set; }


        public string? GenderGapApproach { get; set; }

        public string? AccessForUnderserved { get; set; }
        public string? ResourceOptimization { get; set; }

        public string? DataProtection { get; set; }


        public string? PopulationImpacted { get; set; }


        public string? SocialGoodContribution { get; set; }


        public string? EthicalOperations { get; set; } // comma-separated


        public string? DiversityInclusion { get; set; } // comma-separated


        public string? EquitableOpportunities { get; set; } // comma-separated


        public string? AccessibilityForDisadvantaged { get; set; } // comma-separated

        // ── ADDITIONAL / DOCUMENTS (Step 10)
        [Column(TypeName = "text")]
        public string? AdditionalInformation { get; set; }

        [Column(TypeName = "text")]
        public string? DocumentDetails { get; set; }

        public List<ApplicationDocument> Documents { get; set; } = new();

        // ── AGREEMENTS
        public bool AgreesToTermsOfService { get; set; } = false;
        public bool AgreesToPrivacyPolicy { get; set; } = false;
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
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }


        public string? Role { get; set; }


        public string? LinkedInUrl { get; set; }


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

        public string OriginalFileName { get; set; } = default!;


        public string StoredFilePath { get; set; } = default!;


        public string DocumentType { get; set; } = "Other";

        public long FileSizeBytes { get; set; }
        public string FileExtension { get; set; } = default!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
    // ======================
    // SOCIAL MEDIA — owned type
    // ======================

    [Owned]
    public class SocialMedia
    {

        public string? LinkedIn { get; set; }


        public string? Twitter { get; set; }


        public string? Instagram { get; set; }


        public string? Facebook { get; set; }
    }
}