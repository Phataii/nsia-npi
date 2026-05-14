using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nsia.Data;
using nsia.Models;
using nsia.Services;
using nsia.ViewModels;

namespace Nsia.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _email;
        private readonly IFileService _fileService;
        private readonly ILogger<ApplicationController> _logger;
        private readonly INinEncryptionService _ninService;
        private readonly IConfiguration _config;

        public ApplicationController(
            ApplicationDbContext db,
            IEmailService email,
            IFileService fileService,
            ILogger<ApplicationController> logger,
            INinEncryptionService ninService,
            IConfiguration config)
        {
            _db = db;
            _email = email;
            _fileService = fileService;
            _logger = logger;
            _ninService = ninService;
            _config = config;
        }
        [HttpGet]
        public async Task<IActionResult> PreChecklist()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            PopulateStepViewBag(app);
            ViewBag.IsRegisteredInNigeria = app.IsRegisteredInNigeria;
            ViewBag.BusinessSector = app.BusinessSector;
            ViewBag.CountryOfOrigin = app.CountryOfOrigin;

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePreChecklist(
            string? IsRegisteredInNigeria,
            string? BusinessSector,
            string? CountryOfOrigin)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            app.IsRegisteredInNigeria = IsRegisteredInNigeria;
            app.BusinessSector = BusinessSector;
            app.CountryOfOrigin = CountryOfOrigin;
            app.ApplicationStep = Math.Max(app.ApplicationStep, 2);
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction("PersonalInfo");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePreChecklistDraft(
            string? IsRegisteredInNigeria,
            string? BusinessSector,
            string? CountryOfOrigin)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.IsRegisteredInNigeria = IsRegisteredInNigeria;
            app.BusinessSector = BusinessSector;
            app.CountryOfOrigin = CountryOfOrigin;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }


        [HttpGet]
        public async Task<IActionResult> PersonalInfo()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 2) return RedirectToAction("PreChecklist");

            PopulateStepViewBag(app);
            ViewBag.FullName = app.FullName;
            ViewBag.Email = app.Email;
            ViewBag.Phone = app.Phone;
            ViewBag.Gender = app.Gender;
            ViewBag.RelationshipToBusiness = app.RelationshipToBusiness;

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePersonalInfo(
            string? FullName,
            string? Phone,
            string? Gender,
            string? Nin,
            string? RelationshipToBusiness)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            app.FullName = FullName?.Trim();
            app.Phone = Phone?.Trim();
            app.Gender = Gender;
            app.RelationshipToBusiness = RelationshipToBusiness;
            app.UpdatedAt = DateTime.UtcNow;

            // Encrypt NIN only if provided
            if (!string.IsNullOrWhiteSpace(Nin))
                app.NinEncrypted = _ninService.Encrypt(Nin.Trim());

            app.ApplicationStep = Math.Max(app.ApplicationStep, 3);
            await _db.SaveChangesAsync();
            return RedirectToAction("CompanyInfo");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePersonalInfoDraft(
            string? FullName,
            string? Phone,
            string? Gender,
            string? Nin,
            string? RelationshipToBusiness)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.FullName = FullName?.Trim() ?? app.FullName;
            app.Phone = Phone?.Trim() ?? app.Phone;
            app.Gender = Gender ?? app.Gender;
            app.RelationshipToBusiness = RelationshipToBusiness ?? app.RelationshipToBusiness;
            app.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(Nin))
                app.NinEncrypted = _ninService.Encrypt(Nin.Trim());

            await _db.SaveChangesAsync();
            return Ok();
        }
        // ─────────────────────────────────
        // DASHBOARD
        // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            ViewBag.ApplicantName = app.FullName;
            ViewBag.CompletedStep = app.ApplicationStep - 1;
            return View();
        }

        // ─────────────────────────────────
        // STEP 1 – PROFILE
        // ─────────────────────────────────


        // ─────────────────────────────────
        // STEP 2 – COMPANY INFO
        // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CompanyInfo()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 3) return RedirectToAction("PersonalInfo");

            PopulateStepViewBag(app);
            ViewBag.CompanyName = app.CompanyName;
            ViewBag.CompanyWebsite = app.CompanyWebsite;
            ViewBag.Twitter = app.SocialMedia?.Twitter;
            ViewBag.Instagram = app.SocialMedia?.Instagram;
            ViewBag.LinkedIn = app.SocialMedia?.LinkedIn;
            ViewBag.Facebook = app.SocialMedia?.Facebook;
            ViewBag.BusinessState = app.BusinessState;
            ViewBag.BusinessLga = app.BusinessLga;
            ViewBag.CompanyHqAddress = app.CompanyHqAddress;
            ViewBag.GeographicScope = app.GeographicScope;
            ViewBag.CompanyRegistrationNumber = app.CompanyRegistrationNumber;
            ViewBag.RegulatoryCompliance = app.RegulatoryCompliance;
            ViewBag.TaxCompliance = app.TaxCompliance;
            ViewBag.HasForeignAffiliates = app.HasForeignAffiliates;
            ViewBag.IsNigerianEntityPrimary = app.IsNigerianEntityPrimary;
            ViewBag.CompanyStructure = app.CompanyStructure;
            ViewBag.ParentOrganizationName = app.ParentOrganizationName;
            ViewBag.OtherCompetitions = app.OtherCompetitions;

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCompanyInfo(
            string? CompanyName,
            string? CompanyWebsite,
            string? Twitter,
            string? Instagram,
            string? LinkedIn,
            string? Facebook,
            string? BusinessState,
            string? BusinessLga,
            string? CompanyHqAddress,
            string? GeographicScope,
            string? CompanyRegistrationNumber,
            string? RegulatoryCompliance,
            string? TaxCompliance,
            string? HasForeignAffiliates,
            string? IsNigerianEntityPrimary,
            string? CompanyStructure,
            string? ParentOrganizationName,
            string? OtherCompetitions)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            app.CompanyName = CompanyName?.Trim();
            app.CompanyWebsite = CompanyWebsite?.Trim();
            app.SocialMedia = new SocialMedia
            {
                Twitter = Twitter?.Trim(),
                Instagram = Instagram?.Trim(),
                LinkedIn = LinkedIn?.Trim(),
                Facebook = Facebook?.Trim(),
            };
            app.BusinessState = BusinessState;
            app.BusinessLga = BusinessLga;
            app.CompanyHqAddress = CompanyHqAddress?.Trim();
            app.GeographicScope = GeographicScope;
            app.CompanyRegistrationNumber = CompanyRegistrationNumber?.Trim();
            app.RegulatoryCompliance = RegulatoryCompliance;
            app.TaxCompliance = TaxCompliance;
            app.HasForeignAffiliates = HasForeignAffiliates;
            app.IsNigerianEntityPrimary = IsNigerianEntityPrimary;
            app.CompanyStructure = CompanyStructure;
            app.ParentOrganizationName = ParentOrganizationName?.Trim();
            app.OtherCompetitions = OtherCompetitions?.Trim();
            app.ApplicationStep = Math.Max(app.ApplicationStep, 4);
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction("TeamInfo");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCompanyInfoDraft(
            string? CompanyName, string? CompanyWebsite,
            string? Twitter, string? Instagram, string? LinkedIn, string? Facebook,
            string? BusinessState, string? BusinessLga, string? CompanyHqAddress,
            string? GeographicScope, string? CompanyRegistrationNumber,
            string? RegulatoryCompliance, string? TaxCompliance,
            string? HasForeignAffiliates, string? IsNigerianEntityPrimary,
            string? CompanyStructure, string? ParentOrganizationName,
            string? OtherCompetitions)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.CompanyName = CompanyName?.Trim() ?? app.CompanyName;
            app.CompanyWebsite = CompanyWebsite?.Trim() ?? app.CompanyWebsite;
            app.SocialMedia = new SocialMedia
            {
                Twitter = Twitter?.Trim() ?? app.SocialMedia?.Twitter,
                Instagram = Instagram?.Trim() ?? app.SocialMedia?.Instagram,
                LinkedIn = LinkedIn?.Trim() ?? app.SocialMedia?.LinkedIn,
                Facebook = Facebook?.Trim() ?? app.SocialMedia?.Facebook,
            };
            app.BusinessState = BusinessState ?? app.BusinessState;
            app.BusinessLga = BusinessLga ?? app.BusinessLga;
            app.CompanyHqAddress = CompanyHqAddress?.Trim() ?? app.CompanyHqAddress;
            app.GeographicScope = GeographicScope ?? app.GeographicScope;
            app.CompanyRegistrationNumber = CompanyRegistrationNumber?.Trim() ?? app.CompanyRegistrationNumber;
            app.RegulatoryCompliance = RegulatoryCompliance ?? app.RegulatoryCompliance;
            app.TaxCompliance = TaxCompliance ?? app.TaxCompliance;
            app.HasForeignAffiliates = HasForeignAffiliates ?? app.HasForeignAffiliates;
            app.IsNigerianEntityPrimary = IsNigerianEntityPrimary ?? app.IsNigerianEntityPrimary;
            app.CompanyStructure = CompanyStructure ?? app.CompanyStructure;
            app.ParentOrganizationName = ParentOrganizationName?.Trim() ?? app.ParentOrganizationName;
            app.OtherCompetitions = OtherCompetitions?.Trim() ?? app.OtherCompetitions;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ─────────────────────────────────
        // STEP 3 – TEAM INFO
        // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> TeamInfo()
        {
            var app = await GetCurrentApplicationAsync(includeFounders: true);
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 4) return RedirectToAction("CompanyInfo");

            PopulateStepViewBag(app);
            ViewBag.NumberOfFounders = app.NumberOfFounders;
            ViewBag.FoundingTeamType = app.FoundingTeamType;
            ViewBag.FounderIndustryExperience = app.FounderIndustryExperience;
            ViewBag.ManagementTeamExperience = app.ManagementTeamExperience;
            ViewBag.TotalFullTimeEmployees = app.TotalFullTimeEmployees;
            ViewBag.Founders = app.Founders
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new
                {
                    name = f.FullName ?? "",
                    phone = f.PhoneNumber ?? "",
                    role = f.Role ?? "",
                    linkedin = f.LinkedInUrl ?? "",
                    nationality = f.Nationality ?? "",
                }).ToList();

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTeamInfo(
            string? NumberOfFounders,
            string? FoundingTeamType,
            string? FounderIndustryExperience,
            string? ManagementTeamExperience,
            string? TotalFullTimeEmployees,
            List<string>? FounderName,
            List<string>? FounderPhone,
            List<string>? FounderRole,
            List<string>? FounderLinkedIn,
            List<string>? FounderNationality)
        {
            var app = await GetCurrentApplicationAsync(includeFounders: true);
            if (app == null) return RedirectToLogin();

            app.NumberOfFounders = NumberOfFounders;
            app.FoundingTeamType = FoundingTeamType;
            app.FounderIndustryExperience = FounderIndustryExperience;
            app.ManagementTeamExperience = ManagementTeamExperience;
            app.TotalFullTimeEmployees = TotalFullTimeEmployees;
            app.UpdatedAt = DateTime.UtcNow;

            // Upsert founders
            var existingFounders = await _db.Founders
                .Where(f => f.ApplicationId == app.Id)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            var submittedCount = FounderName?.Count(n => !string.IsNullOrWhiteSpace(n)) ?? 0;

            for (int i = 0; i < submittedCount; i++)
            {
                var name = FounderName![i].Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                if (i < existingFounders.Count)
                {
                    existingFounders[i].FullName = name;
                    existingFounders[i].PhoneNumber = FounderPhone?.ElementAtOrDefault(i)?.Trim();
                    existingFounders[i].Role = FounderRole?.ElementAtOrDefault(i)?.Trim();
                    existingFounders[i].LinkedInUrl = FounderLinkedIn?.ElementAtOrDefault(i)?.Trim();
                    existingFounders[i].Nationality = FounderNationality?.ElementAtOrDefault(i)?.Trim();
                    existingFounders[i].DisplayOrder = i + 1;
                }
                else
                {
                    _db.Founders.Add(new Founder
                    {
                        ApplicationId = app.Id,
                        FullName = name,
                        PhoneNumber = FounderPhone?.ElementAtOrDefault(i)?.Trim(),
                        Role = FounderRole?.ElementAtOrDefault(i)?.Trim(),
                        LinkedInUrl = FounderLinkedIn?.ElementAtOrDefault(i)?.Trim(),
                        Nationality = FounderNationality?.ElementAtOrDefault(i)?.Trim(),
                        DisplayOrder = i + 1,
                    });
                }
            }

            // Remove extras
            if (existingFounders.Count > submittedCount)
                _db.Founders.RemoveRange(existingFounders.Skip(submittedCount));

            app.ApplicationStep = Math.Max(app.ApplicationStep, 5);
            await _db.SaveChangesAsync();
            return RedirectToAction("ProductInfo");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTeamInfoDraft(
            string? NumberOfFounders,
            string? FoundingTeamType,
            string? FounderIndustryExperience,
            string? ManagementTeamExperience,
            string? TotalFullTimeEmployees)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.NumberOfFounders = NumberOfFounders ?? app.NumberOfFounders;
            app.FoundingTeamType = FoundingTeamType ?? app.FoundingTeamType;
            app.FounderIndustryExperience = FounderIndustryExperience ?? app.FounderIndustryExperience;
            app.ManagementTeamExperience = ManagementTeamExperience ?? app.ManagementTeamExperience;
            app.TotalFullTimeEmployees = TotalFullTimeEmployees ?? app.TotalFullTimeEmployees;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // // ─────────────────────────────────
        // // STEP 4 – PRODUCT INFO
        // // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ProductInfo()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 5) return RedirectToAction("TeamInfo");

            PopulateStepViewBag(app);
            ViewBag.GrowthStage = app.GrowthStage;
            ViewBag.KeyMilestones = app.KeyMilestones;
            ViewBag.ExistingUsers = app.ExistingUsers;
            ViewBag.TotalUsersReached = app.TotalUsersReached;
            ViewBag.CoreBusinessModel = app.CoreBusinessModel;
            ViewBag.UniqueSellingPoint = app.UniqueSellingPoint;
            ViewBag.MainCompetitors = app.MainCompetitors;
            ViewBag.MarketPenetrationStrategy = app.MarketPenetrationStrategy;
            ViewBag.KeyFeatures = app.KeyFeatures;

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProductInfo(
            string? GrowthStage,
            List<string>? KeyMilestones,
            string? ExistingUsers,
            string? TotalUsersReached,
            string? CoreBusinessModel,
            string? UniqueSellingPoint,
            string? MainCompetitors,
            string? MarketPenetrationStrategy,
            List<string>? KeyFeatures)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            app.GrowthStage = GrowthStage;
            app.KeyMilestones = KeyMilestones != null ? string.Join(",", KeyMilestones) : null;
            app.ExistingUsers = ExistingUsers;
            app.TotalUsersReached = TotalUsersReached;
            app.CoreBusinessModel = CoreBusinessModel;
            app.UniqueSellingPoint = UniqueSellingPoint;
            app.MainCompetitors = MainCompetitors;
            app.MarketPenetrationStrategy = MarketPenetrationStrategy;
            app.KeyFeatures = KeyFeatures != null ? string.Join(",", KeyFeatures) : null;
            app.ApplicationStep = Math.Max(app.ApplicationStep, 6);
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction("Commercial1");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProductInfoDraft(
            string? GrowthStage,
            List<string>? KeyMilestones,
            string? ExistingUsers,
            string? TotalUsersReached,
            string? CoreBusinessModel,
            string? UniqueSellingPoint,
            string? MainCompetitors,
            string? MarketPenetrationStrategy,
            List<string>? KeyFeatures)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.GrowthStage = GrowthStage ?? app.GrowthStage;
            app.KeyMilestones = KeyMilestones != null ? string.Join(",", KeyMilestones) : app.KeyMilestones;
            app.ExistingUsers = ExistingUsers ?? app.ExistingUsers;
            app.TotalUsersReached = TotalUsersReached ?? app.TotalUsersReached;
            app.CoreBusinessModel = CoreBusinessModel ?? app.CoreBusinessModel;
            app.UniqueSellingPoint = UniqueSellingPoint ?? app.UniqueSellingPoint;
            app.MainCompetitors = MainCompetitors ?? app.MainCompetitors;
            app.MarketPenetrationStrategy = MarketPenetrationStrategy ?? app.MarketPenetrationStrategy;
            app.KeyFeatures = KeyFeatures != null ? string.Join(",", KeyFeatures) : app.KeyFeatures;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // // ─────────────────────────────────
        // // STEP 5 – COMMERCIAL
        // // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Commercial1()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 6) return RedirectToAction("ProductInfo");

            PopulateStepViewBag(app);
            ViewBag.HasStartedGeneratingSales = app.HasStartedGeneratingSales;
            ViewBag.YearOfFirstSale = app.YearOfFirstSale;
            ViewBag.YearlySalesRevenue = app.YearlySalesRevenue;
            ViewBag.YearlyProfit = app.YearlyProfit;
            ViewBag.ProprietaryFunding = app.ProprietaryFunding;
            ViewBag.ExternalFunding = app.ExternalFunding;
            ViewBag.TypesOfFunding = app.TypesOfFunding;
            ViewBag.IsCurrentlyFundraising = app.IsCurrentlyFundraising;
            ViewBag.ProjectedRevenue = app.ProjectedRevenue;
            ViewBag.CompanyValuation = app.CompanyValuation;

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCommercial1(
            string? HasStartedGeneratingSales,
            string? YearOfFirstSale,
            string? YearlySalesRevenue,
            string? YearlyProfit,
            string? ProprietaryFunding,
            string? ExternalFunding,
            List<string>? TypesOfFunding,
            string? IsCurrentlyFundraising,
            string? ProjectedRevenue,
            string? CompanyValuation)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            app.HasStartedGeneratingSales = HasStartedGeneratingSales;
            app.YearOfFirstSale = YearOfFirstSale;
            app.YearlySalesRevenue = YearlySalesRevenue;
            app.YearlyProfit = YearlyProfit;
            app.ProprietaryFunding = ProprietaryFunding;
            app.ExternalFunding = ExternalFunding;
            app.TypesOfFunding = TypesOfFunding != null ? string.Join(",", TypesOfFunding) : null;
            app.IsCurrentlyFundraising = IsCurrentlyFundraising;
            app.ProjectedRevenue = ProjectedRevenue;
            app.CompanyValuation = CompanyValuation;
            app.ApplicationStep = Math.Max(app.ApplicationStep, 7);
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction("Commercial2");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCommercial1Draft(
            string? HasStartedGeneratingSales,
            string? YearOfFirstSale,
            string? YearlySalesRevenue,
            string? YearlyProfit,
            string? ProprietaryFunding,
            string? ExternalFunding,
            List<string>? TypesOfFunding,
            string? IsCurrentlyFundraising,
            string? ProjectedRevenue,
            string? CompanyValuation)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.HasStartedGeneratingSales = HasStartedGeneratingSales ?? app.HasStartedGeneratingSales;
            app.YearOfFirstSale = YearOfFirstSale ?? app.YearOfFirstSale;
            app.YearlySalesRevenue = YearlySalesRevenue ?? app.YearlySalesRevenue;
            app.YearlyProfit = YearlyProfit ?? app.YearlyProfit;
            app.ProprietaryFunding = ProprietaryFunding ?? app.ProprietaryFunding;
            app.ExternalFunding = ExternalFunding ?? app.ExternalFunding;
            app.TypesOfFunding = TypesOfFunding != null ? string.Join(",", TypesOfFunding) : app.TypesOfFunding;
            app.IsCurrentlyFundraising = IsCurrentlyFundraising ?? app.IsCurrentlyFundraising;
            app.ProjectedRevenue = ProjectedRevenue ?? app.ProjectedRevenue;
            app.CompanyValuation = CompanyValuation ?? app.CompanyValuation;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Commercial2()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 7) return RedirectToAction("Commercial1");

            PopulateStepViewBag(app);
            ViewBag.DemandEvidence = app.DemandEvidence;
            ViewBag.RevenueStreams = app.RevenueStreams;
            ViewBag.GeographicScalability = app.GeographicScalability;
            ViewBag.GrossMargins = app.GrossMargins;
            ViewBag.PrimaryCompetitiveAdvantage = app.PrimaryCompetitiveAdvantage;
            ViewBag.OperatingRunway = app.OperatingRunway;
            ViewBag.ActivePartnerships = app.ActivePartnerships;
            ViewBag.RegulatoryApproach = app.RegulatoryApproach;
            ViewBag.CrossIndustryApplication = app.CrossIndustryApplication;
            ViewBag.LongTermGrowthStrategy = app.LongTermGrowthStrategy;
            ViewBag.SupplyChainReliability = app.SupplyChainReliability;
            ViewBag.IpOwnership = app.IpOwnership;
            ViewBag.PricingStrategy = app.PricingStrategy;
            ViewBag.BiggestRisks = app.BiggestRisks;
            ViewBag.NewCustomersSixMonths = app.NewCustomersSixMonths;
            ViewBag.CustomerGrowthRate = app.CustomerGrowthRate;
            ViewBag.AverageCAC = app.AverageCAC;
            ViewBag.RepeatCustomerRevenue = app.RepeatCustomerRevenue;

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCommercial2(
            string? DemandEvidence,
            string? RevenueStreams,
            string? GeographicScalability,
            string? GrossMargins,
            string? PrimaryCompetitiveAdvantage,
            string? OperatingRunway,
            string? ActivePartnerships,
            string? RegulatoryApproach,
            string? CrossIndustryApplication,
            string? LongTermGrowthStrategy,
            string? SupplyChainReliability,
            string? IpOwnership,
            string? PricingStrategy,
            string? BiggestRisks,
            string? NewCustomersSixMonths,
            string? CustomerGrowthRate,
            string? AverageCAC,
            string? RepeatCustomerRevenue)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            app.DemandEvidence = DemandEvidence;
            app.RevenueStreams = RevenueStreams;
            app.GeographicScalability = GeographicScalability;
            app.GrossMargins = GrossMargins;
            app.PrimaryCompetitiveAdvantage = PrimaryCompetitiveAdvantage;
            app.OperatingRunway = OperatingRunway;
            app.ActivePartnerships = ActivePartnerships;
            app.RegulatoryApproach = RegulatoryApproach;
            app.CrossIndustryApplication = CrossIndustryApplication;
            app.LongTermGrowthStrategy = LongTermGrowthStrategy;
            app.SupplyChainReliability = SupplyChainReliability;
            app.IpOwnership = IpOwnership;
            app.PricingStrategy = PricingStrategy;
            app.BiggestRisks = BiggestRisks;
            app.NewCustomersSixMonths = NewCustomersSixMonths;
            app.CustomerGrowthRate = CustomerGrowthRate;
            app.AverageCAC = AverageCAC;
            app.RepeatCustomerRevenue = RepeatCustomerRevenue;
            app.ApplicationStep = Math.Max(app.ApplicationStep, 8);
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction("Sustainability");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCommercial2Draft(
            string? DemandEvidence, string? RevenueStreams,
            string? GeographicScalability, string? GrossMargins,
            string? PrimaryCompetitiveAdvantage, string? OperatingRunway,
            string? ActivePartnerships, string? RegulatoryApproach,
            string? CrossIndustryApplication, string? LongTermGrowthStrategy,
            string? SupplyChainReliability, string? IpOwnership,
            string? PricingStrategy, string? BiggestRisks,
            string? NewCustomersSixMonths, string? CustomerGrowthRate,
            string? AverageCAC, string? RepeatCustomerRevenue)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.DemandEvidence = DemandEvidence ?? app.DemandEvidence;
            app.RevenueStreams = RevenueStreams ?? app.RevenueStreams;
            app.GeographicScalability = GeographicScalability ?? app.GeographicScalability;
            app.GrossMargins = GrossMargins ?? app.GrossMargins;
            app.PrimaryCompetitiveAdvantage = PrimaryCompetitiveAdvantage ?? app.PrimaryCompetitiveAdvantage;
            app.OperatingRunway = OperatingRunway ?? app.OperatingRunway;
            app.ActivePartnerships = ActivePartnerships ?? app.ActivePartnerships;
            app.RegulatoryApproach = RegulatoryApproach ?? app.RegulatoryApproach;
            app.CrossIndustryApplication = CrossIndustryApplication ?? app.CrossIndustryApplication;
            app.LongTermGrowthStrategy = LongTermGrowthStrategy ?? app.LongTermGrowthStrategy;
            app.SupplyChainReliability = SupplyChainReliability ?? app.SupplyChainReliability;
            app.IpOwnership = IpOwnership ?? app.IpOwnership;
            app.PricingStrategy = PricingStrategy ?? app.PricingStrategy;
            app.BiggestRisks = BiggestRisks ?? app.BiggestRisks;
            app.NewCustomersSixMonths = NewCustomersSixMonths ?? app.NewCustomersSixMonths;
            app.CustomerGrowthRate = CustomerGrowthRate ?? app.CustomerGrowthRate;
            app.AverageCAC = AverageCAC ?? app.AverageCAC;
            app.RepeatCustomerRevenue = RepeatCustomerRevenue ?? app.RepeatCustomerRevenue;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }
        // // ─────────────────────────────────
        // // STEP 6 – IMPACT & SUSTAINABILITY
        // // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Sustainability()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 8) return RedirectToAction("Commercial2");

            PopulateStepViewBag(app);
            ViewBag.SdgAlignment = app.SdgAlignment;
            ViewBag.BusinessReplicability = app.BusinessReplicability;
            ViewBag.SustainabilityIntegration = app.SustainabilityIntegration;
            ViewBag.EnergyWasteReduction = app.EnergyWasteReduction;
            ViewBag.SustainabilityTechnology = app.SustainabilityTechnology;
            ViewBag.ScalingWithSustainability = app.ScalingWithSustainability;
            ViewBag.ClimateChangeApproach = app.ClimateChangeApproach;
            ViewBag.DigitalAccessibility = app.DigitalAccessibility;

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSustainability(
            List<string>? SdgAlignment,
            string? BusinessReplicability,
            string? SustainabilityIntegration,
            string? EnergyWasteReduction,
            string? SustainabilityTechnology,
            string? ScalingWithSustainability,
            string? ClimateChangeApproach,
            string? DigitalAccessibility)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            app.SdgAlignment = SdgAlignment != null ? string.Join(",", SdgAlignment) : null;
            app.BusinessReplicability = BusinessReplicability;
            app.SustainabilityIntegration = SustainabilityIntegration;
            app.EnergyWasteReduction = EnergyWasteReduction;
            app.SustainabilityTechnology = SustainabilityTechnology;
            app.ScalingWithSustainability = ScalingWithSustainability;
            app.ClimateChangeApproach = ClimateChangeApproach;
            app.DigitalAccessibility = DigitalAccessibility;
            app.ApplicationStep = Math.Max(app.ApplicationStep, 9);
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction("Impact");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSustainabilityDraft(
            List<string>? SdgAlignment,
            string? BusinessReplicability,
            string? SustainabilityIntegration,
            string? EnergyWasteReduction,
            string? SustainabilityTechnology,
            string? ScalingWithSustainability,
            string? ClimateChangeApproach,
            string? DigitalAccessibility)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.SdgAlignment = SdgAlignment != null ? string.Join(",", SdgAlignment) : app.SdgAlignment;
            app.BusinessReplicability = BusinessReplicability ?? app.BusinessReplicability;
            app.SustainabilityIntegration = SustainabilityIntegration ?? app.SustainabilityIntegration;
            app.EnergyWasteReduction = EnergyWasteReduction ?? app.EnergyWasteReduction;
            app.SustainabilityTechnology = SustainabilityTechnology ?? app.SustainabilityTechnology;
            app.ScalingWithSustainability = ScalingWithSustainability ?? app.ScalingWithSustainability;
            app.ClimateChangeApproach = ClimateChangeApproach ?? app.ClimateChangeApproach;
            app.DigitalAccessibility = DigitalAccessibility ?? app.DigitalAccessibility;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Impact()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 9) return RedirectToAction("Sustainability");

            PopulateStepViewBag(app);
            ViewBag.UnderservedMarketPercentage = app.UnderservedMarketPercentage;
            ViewBag.SystemicInequalityApproach = app.SystemicInequalityApproach;
            ViewBag.BeneficiaryInvolvement = app.BeneficiaryInvolvement;
            ViewBag.ImpactDataSharing = app.ImpactDataSharing;
            ViewBag.JobsCreated = app.JobsCreated;
            ViewBag.GenderGapApproach = app.GenderGapApproach;
            ViewBag.AccessForUnderserved = app.AccessForUnderserved;
            ViewBag.ResourceOptimization = app.ResourceOptimization;
            ViewBag.DataProtection = app.DataProtection;
            ViewBag.PopulationImpacted = app.PopulationImpacted;
            ViewBag.SocialGoodContribution = app.SocialGoodContribution;
            ViewBag.EthicalOperations = app.EthicalOperations;
            ViewBag.DiversityInclusion = app.DiversityInclusion;
            ViewBag.EquitableOpportunities = app.EquitableOpportunities;
            ViewBag.AccessibilityForDisadvantaged = app.AccessibilityForDisadvantaged;

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveImpact(
            string? UnderservedMarketPercentage,
            string? SystemicInequalityApproach,
            string? BeneficiaryInvolvement,
            string? ImpactDataSharing,
            string? JobsCreated,
            string? GenderGapApproach,
            string? AccessForUnderserved,
            string? ResourceOptimization,
            string? DataProtection,
            string? PopulationImpacted,
            string? SocialGoodContribution,
            List<string>? EthicalOperations,
            List<string>? DiversityInclusion,
            List<string>? EquitableOpportunities,
            List<string>? AccessibilityForDisadvantaged)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            app.UnderservedMarketPercentage = UnderservedMarketPercentage;
            app.SystemicInequalityApproach = SystemicInequalityApproach;
            app.BeneficiaryInvolvement = BeneficiaryInvolvement;
            app.ImpactDataSharing = ImpactDataSharing;
            app.JobsCreated = JobsCreated;
            app.GenderGapApproach = GenderGapApproach;
            app.AccessForUnderserved = AccessForUnderserved;
            app.ResourceOptimization = ResourceOptimization;
            app.DataProtection = DataProtection;
            app.PopulationImpacted = PopulationImpacted;
            app.SocialGoodContribution = SocialGoodContribution;
            app.EthicalOperations = EthicalOperations != null ? string.Join(",", EthicalOperations) : null;
            app.DiversityInclusion = DiversityInclusion != null ? string.Join(",", DiversityInclusion) : null;
            app.EquitableOpportunities = EquitableOpportunities != null ? string.Join(",", EquitableOpportunities) : null;
            app.AccessibilityForDisadvantaged = AccessibilityForDisadvantaged != null ? string.Join(",", AccessibilityForDisadvantaged) : null;
            app.ApplicationStep = Math.Max(app.ApplicationStep, 10);
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction("Additional");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveImpactDraft(
            string? UnderservedMarketPercentage,
            string? SystemicInequalityApproach,
            string? BeneficiaryInvolvement,
            string? ImpactDataSharing,
            string? JobsCreated,
            string? GenderGapApproach,
            string? AccessForUnderserved,
            string? ResourceOptimization,
            string? DataProtection,
            string? PopulationImpacted,
            string? SocialGoodContribution,
            List<string>? EthicalOperations,
            List<string>? DiversityInclusion,
            List<string>? EquitableOpportunities,
            List<string>? AccessibilityForDisadvantaged)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.UnderservedMarketPercentage = UnderservedMarketPercentage ?? app.UnderservedMarketPercentage;
            app.SystemicInequalityApproach = SystemicInequalityApproach ?? app.SystemicInequalityApproach;
            app.BeneficiaryInvolvement = BeneficiaryInvolvement ?? app.BeneficiaryInvolvement;
            app.ImpactDataSharing = ImpactDataSharing ?? app.ImpactDataSharing;
            app.JobsCreated = JobsCreated ?? app.JobsCreated;
            app.GenderGapApproach = GenderGapApproach ?? app.GenderGapApproach;
            app.AccessForUnderserved = AccessForUnderserved ?? app.AccessForUnderserved;
            app.ResourceOptimization = ResourceOptimization ?? app.ResourceOptimization;
            app.DataProtection = DataProtection ?? app.DataProtection;
            app.PopulationImpacted = PopulationImpacted ?? app.PopulationImpacted;
            app.SocialGoodContribution = SocialGoodContribution ?? app.SocialGoodContribution;
            app.EthicalOperations = EthicalOperations != null ? string.Join(",", EthicalOperations) : app.EthicalOperations;
            app.DiversityInclusion = DiversityInclusion != null ? string.Join(",", DiversityInclusion) : app.DiversityInclusion;
            app.EquitableOpportunities = EquitableOpportunities != null ? string.Join(",", EquitableOpportunities) : app.EquitableOpportunities;
            app.AccessibilityForDisadvantaged = AccessibilityForDisadvantaged != null ? string.Join(",", AccessibilityForDisadvantaged) : app.AccessibilityForDisadvantaged;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }
        // ─────────────────────────────────
        // STEP 7 – ADDITIONAL INFORMATION
        // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Additional()
        {
            var app = await GetCurrentApplicationAsync(includeDocuments: true);
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 10) return RedirectToAction("Impact");

            PopulateStepViewBag(app);
            ViewBag.AdditionalInformation = app.AdditionalInformation;
            ViewBag.Documents = app.Documents
                .OrderBy(d => d.UploadedAt)
                .Select(d => new
                {
                    id = d.Id,
                    name = d.OriginalFileName ?? "",
                    type = d.DocumentType ?? "",
                    size = d.FileSizeBytes,
                    url = d.OriginalFileName ?? "",
                }).ToList();

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAdditional(
            string? AdditionalInformation,
            List<IFormFile>? Documents,
            List<string>? DocumentTypes)
        {
            var app = await GetCurrentApplicationAsync(includeDocuments: true);
            if (app == null) return RedirectToLogin();

            app.AdditionalInformation = AdditionalInformation?.Trim();
            app.UpdatedAt = DateTime.UtcNow;

            // Upload new documents
            if (Documents != null)
            {
                for (int i = 0; i < Documents.Count; i++)
                {
                    var file = Documents[i];
                    if (file == null || file.Length == 0) continue;

                    // var (storedPath, publicUrl, fileName) =
                    //     await _fileService.SaveFileAsync(file, app.Id.ToString());

                    // app.Documents.Add(new ApplicationDocument
                    // {
                    //     ApplicationId = app.Id,
                    //     OriginalFileName = fileName,
                    //     StoredFilePath = storedPath,
                    //     // PublicUrl = publicUrl,
                    //     DocumentType = DocumentTypes?.ElementAtOrDefault(i) ?? "Supporting Document",
                    //     FileSizeBytes = file.Length,
                    //     FileExtension = Path.GetExtension(fileName).ToLowerInvariant(),
                    //     UploadedAt = DateTime.UtcNow,
                    // });
                }
            }

            // Mark as submitted
            app.Status = "Submitted";
            app.SubmittedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Send confirmation email (fire and forget)
            _ = _email.SendSubmissionConfirmationAsync(app.Email!, app.FullName!, app.ReferenceNumber!);

            return RedirectToAction("Submitted");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAdditionalDraft(
            string? AdditionalInformation)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.AdditionalInformation = AdditionalInformation?.Trim() ?? app.AdditionalInformation;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(Guid documentId)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            var doc = await _db.ApplicationDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.ApplicationId == app.Id);

            if (doc == null) return NotFound();

            _fileService.DeleteFile(doc.StoredFilePath);
            _db.ApplicationDocuments.Remove(doc);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("/npi/uploads/{**filePath}")]
        public IActionResult ServeFile(string filePath)
        {
            var rootPath = _config["UploadSettings:RootPath"]
                ?? throw new InvalidOperationException("UploadSettings:RootPath not configured.");

            // Resolve relative paths
            if (!Path.IsPathRooted(rootPath))
                rootPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), rootPath));

            // Sanitise filePath to prevent directory traversal attacks
            var safePath = filePath.Replace("..", "").TrimStart('/');
            var fullPath = Path.GetFullPath(Path.Combine(rootPath, safePath));
            // Make sure the resolved path is still inside the upload root
            if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid file path.");

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            var mimeType = ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };

            // inline = open in browser, attachment = force download
            var fileName = Path.GetFileName(fullPath);
            var contentDisposition = ext == ".pdf" || ext == ".png" || ext == ".jpg" || ext == ".jpeg"
                ? $"inline; filename=\"{fileName}\""
                : $"attachment; filename=\"{fileName}\"";

            Response.Headers.Add("Content-Disposition", contentDisposition);
            return PhysicalFile(fullPath, mimeType);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAdditionalDraft(AdditionalViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.DocumentDetails = model.DocumentDetails?.Trim() ?? app.DocumentDetails;
            app.AdditionalInformation = model.AdditionalInformation?.Trim() ?? app.AdditionalInformation;
            app.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ─────────────────────────────────
        // SUBMISSION CONFIRMATION
        // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Submitted()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            ViewBag.ApplicantName = app.FullName;
            ViewBag.ReferenceNumber = app.ReferenceNumber;
            ViewBag.SubmittedAt = app.SubmittedAt?.ToString("dd MMMM yyyy, HH:mm") + " UTC";
            return View();
        }

        // ─────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────

        private async Task<nsia.Models.Application?> GetCurrentApplicationAsync(
            bool includeFounders = false,
            bool includeDocuments = false)
        {
            var idStr = HttpContext.Session.GetString("ApplicationId");
            if (!Guid.TryParse(idStr, out var id)) return null;

            var query = _db.Applications.AsQueryable();
            if (includeFounders) query = query.Include(a => a.Founders);
            if (includeDocuments) query = query.Include(a => a.Documents);

            return await query.FirstOrDefaultAsync(a => a.Id == id);
        }

        private IActionResult RedirectToLogin() =>
            RedirectToAction("Login", "Account");

        private void PopulateStepViewBag(Application app)
        {
            ViewBag.ApplicantName = app.FullName;
            ViewBag.CompletedStep = app.ApplicationStep - 1;
        }

        private static List<string> ParseList(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? new List<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

        // private static Task ApplyCommercial(Application app, CommercialViewModel m)
        // {
        //     app.HasStartedGeneratingRevenue = m.GeneratingRevenue == "Yes" ? true : m.GeneratingRevenue == "No" ? false : null;
        //     app.FundingTypes = string.Join(",", m.FundingTypes);
        //     app.IsCurrentlyFundraising = m.CurrentlyFundraising == "Yes" ? true : m.CurrentlyFundraising == "No" ? false : null;
        //     app.CompanyValuation = m.Valuation;
        //     app.ProjectedRevenueNextYear = m.ProjectedRevenue;
        //     app.RevenueStreams = m.RevenueStreams;
        //     app.GrossMargins = m.GrossMargins;
        //     app.RepeatCustomerRevenuePercentage = m.RepeatRevenue;
        //     app.PricingStrategy = m.PricingStrategy;
        //     app.OperatingRunway = m.Runway;
        //     app.DemandEvidence = m.DemandEvidence;
        //     app.EstimatedMarketShare = m.MarketShare;
        //     app.PrimaryCompetitiveEdge = m.CompetitiveEdge;
        //     app.GeographicScalability = m.GeoScalability;
        //     app.CrossIndustryApplicability = m.CrossIndustry;
        //     app.LongTermGrowthPlan = m.GrowthPlan;
        //     app.FeedbackUpdateFrequency = m.UpdateFrequency;
        //     app.CustomersAcquiredPastSixMonths = m.NewCustomers;
        //     app.CustomerGrowthRatePastYear = m.GrowthRate;
        //     app.AverageCustomerAcquisitionCost = m.CAC;
        //     app.SupplyChainReliability = m.SupplyChain;
        //     app.RegulatoryCompliance = m.Compliance;
        //     app.BiggestRisks = string.Join(",", m.Risks);
        //     app.ActivePartnerships = m.Partnerships;
        //     app.IpOwnership = m.IPOwnership;

        //     return Task.CompletedTask;
        // }

        // private static Task ApplyImpact(Application app, ImpactViewModel m)
        // {
        //     app.AlignsWithUnSdgs = m.SDGAligned == "Yes" ? true : m.SDGAligned == "No" ? false : null;
        //     app.SdgsAddressed = m.SDGCount;
        //     app.ReducesEnvironmentalHarm = m.EnvImpact == "Yes" ? true : m.EnvImpact == "No" ? false : null;
        //     app.EnvironmentalHarmReduction = m.EnvReduction;
        //     app.ResourceOptimisation = m.ResourceUse;
        //     app.UnderservedMarketPercentage = m.UnderservedMarket;
        //     app.SystemicInequalityReduction = m.Inequities;
        //     app.GenderGapApproach = m.GenderGaps;
        //     app.AccessForUnderservedGroups = m.AccessUnderserved;
        //     app.JobsCreated = m.JobsCreated;
        //     app.PeopleImpacted = m.PeopleImpacted;
        //     app.UserSelfRelianceLevel = m.SelfReliance;
        //     app.SocialOutcomeTracking = m.OutcomeTracking;
        //     app.ImpactMeasurementMethod = m.ImpactMeasurement;
        //     app.ImpactDataSharingLevel = m.ImpactSharing;
        //     app.BeneficiaryInvolvementLevel = m.BeneficiaryInvolvement;
        //     app.LocalContextTailoring = m.LocalContext;
        //     app.EthicalPracticesApproach = m.Ethics;
        //     app.DataProtectionApproach = m.DataProtection;
        //     app.TrustBuildingApproach = m.TrustBuilding;
        //     app.ModelReplicability = m.Replicability;
        //     app.ImpactDurability = m.ImpactDurability;
        //     app.CrisisPerformance = m.CrisisPerformance;
        //     app.PolicyAdvocacy = m.PolicyAdvocacy;
        //     app.ImpactDataAndStatistics = m.ImpactData?.Trim();
        //     app.MeasurableCommunityDifferences = m.ImpactExamples?.Trim();
        //     app.TopImpactExamplesDetails = m.TopDetails?.Trim();
        //     app.AgreesToNsiaPrivacyPolicy = m.AgreePrivacy;
        //     app.AgreesToCompetitionSubmissionAgreement = m.AgreeCompetition;

        //     return Task.CompletedTask;
        // }

        // private static Task ApplyCompanyInfoDraft(Application app, CompanyInfoViewModel m)
        // {
        //     if (!string.IsNullOrEmpty(m.CompanyName)) app.CompanyName = m.CompanyName.Trim();
        //     if (!string.IsNullOrEmpty(m.CompanyUrl)) app.CompanyUrl = m.CompanyUrl.Trim();
        //     if (!string.IsNullOrEmpty(m.CompanyDescription)) app.CompanyDescription = m.CompanyDescription.Trim();
        //     if (!string.IsNullOrEmpty(m.BusinessAddress)) app.BusinessAddress = m.BusinessAddress.Trim();

        //     app.SocialMedia = new SocialMedia
        //     {
        //         Twitter = m.TwitterHandle?.Trim(),
        //         Instagram = m.InstagramHandle?.Trim(),
        //         LinkedIn = m.LinkedInHandle?.Trim(),
        //         Facebook = m.FacebookHandle?.Trim(),
        //     };

        //     return Task.CompletedTask;
        // }
    }
}