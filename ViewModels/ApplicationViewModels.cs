using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace nsia.ViewModels
{
    public class ProfileViewModel
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = default!;

        [EmailAddress]
        public string Email { get; set; } = default!; // readonly — display only

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        public string Gender { get; set; } = default!;
        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(100)]
        public string? HowDidYouHear { get; set; }
    }

    public class CompanyInfoViewModel
    {
        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = default!;

        [MaxLength(500)]
        public string? CompanyUrl { get; set; }

        public string? TwitterHandle { get; set; }
        public string? InstagramHandle { get; set; }
        public string? LinkedInHandle { get; set; }
        public string? FacebookHandle { get; set; }

        [Required]
        public string CompanyDescription { get; set; } = default!;

        [Required, MaxLength(300)]
        public string BusinessAddress { get; set; } = default!;

        [Required]
        public string IsRegisteredInNigeria { get; set; } = default!;

        public string? RegistrationNumber { get; set; }

        [Required]
        public string IsOperationalEntity { get; set; } = default!;

        [Required]
        public string HasForeignAffiliates { get; set; } = default!;

        public string? AffiliateDetails { get; set; }

        public int? YearOfIncorporation { get; set; }

        public List<string> Milestones { get; set; } = new();
        public List<string> SuccessMetrics { get; set; } = new();
        public List<string> LongTermVision { get; set; } = new();
    }

    public class TeamInfoViewModel
    {
        [Required]
        public int? NumberOfFounders { get; set; }

        public List<string> TeamComposition { get; set; } = new();

        [Required]
        public int? TotalEmployees { get; set; }

        public List<FounderViewModel> Founders { get; set; } = new();
    }

    public class FounderViewModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = default!;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Role { get; set; }

        [MaxLength(500)]
        public string? LinkedInUrl { get; set; }

        [MaxLength(60)]
        public string? Nationality { get; set; }
    }

    public class ProductInfoViewModel
    {
        [Required]
        public string GrowthStage { get; set; } = default!;

        public string? MvpLink { get; set; }

        [Required]
        public string Sector { get; set; } = default!;

        [Required]
        public string ProductDescription { get; set; } = default!;

        [Required]
        public string UserCount { get; set; } = default!;

        public List<string> BusinessModel { get; set; } = new();
        public List<string> USP { get; set; } = new();
        public List<string> Competitors { get; set; } = new();
        public List<string> GoToMarket { get; set; } = new();
        public List<string> KeyFeatures { get; set; } = new();
    }

    public class CommercialViewModel
    {
        public string? GeneratingRevenue { get; set; }
        public List<string> FundingTypes { get; set; } = new();
        public string? CurrentlyFundraising { get; set; }
        public string? Valuation { get; set; }
        public string? ProjectedRevenue { get; set; }
        public string? RevenueStreams { get; set; }
        public string? GrossMargins { get; set; }
        public string? RepeatRevenue { get; set; }
        public string? PricingStrategy { get; set; }
        public string? Runway { get; set; }
        public string? DemandEvidence { get; set; }
        public string? MarketShare { get; set; }
        public string? CompetitiveEdge { get; set; }
        public string? GeoScalability { get; set; }
        public string? CrossIndustry { get; set; }
        public string? GrowthPlan { get; set; }
        public string? UpdateFrequency { get; set; }
        public string? NewCustomers { get; set; }
        public string? GrowthRate { get; set; }
        public string? CAC { get; set; }
        public string? SupplyChain { get; set; }
        public string? Compliance { get; set; }
        public List<string> Risks { get; set; } = new();
        public string? Partnerships { get; set; }
        public string? IPOwnership { get; set; }
    }

    public class ImpactViewModel
    {
        public string? SDGAligned { get; set; }
        public string? SDGCount { get; set; }
        public string? EnvImpact { get; set; }
        public string? EnvReduction { get; set; }
        public string? ResourceUse { get; set; }
        public string? UnderservedMarket { get; set; }
        public string? Inequities { get; set; }
        public string? GenderGaps { get; set; }
        public string? AccessUnderserved { get; set; }
        public string? JobsCreated { get; set; }
        public string? PeopleImpacted { get; set; }
        public string? SelfReliance { get; set; }
        public string? OutcomeTracking { get; set; }
        public string? ImpactMeasurement { get; set; }
        public string? ImpactSharing { get; set; }
        public string? BeneficiaryInvolvement { get; set; }
        public string? LocalContext { get; set; }
        public string? Ethics { get; set; }
        public string? DataProtection { get; set; }
        public string? TrustBuilding { get; set; }
        public string? Replicability { get; set; }
        public string? ImpactDurability { get; set; }
        public string? CrisisPerformance { get; set; }
        public string? PolicyAdvocacy { get; set; }
        public string? ImpactData { get; set; }
        public string? ImpactExamples { get; set; }
        public string? TopDetails { get; set; }
        public bool AgreePrivacy { get; set; }
        public bool AgreeCompetition { get; set; }
    }

    public class AdditionalViewModel
    {
        public List<IFormFile>? Documents { get; set; }
        public string? DocumentDetails { get; set; }
        public string? AdditionalInformation { get; set; }
    }
}