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
        private readonly IConfiguration _config;

        public ApplicationController(
            ApplicationDbContext db,
            IEmailService email,
            IFileService fileService,
            ILogger<ApplicationController> logger,
            IConfiguration config)
        {
            _db = db;
            _email = email;
            _fileService = fileService;
            _logger = logger;
            _config = config;
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

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            ViewBag.ApplicantName = app.FullName;
            ViewBag.CompletedStep = app.ApplicationStep - 1;
            ViewBag.FullName = app.FullName;
            ViewBag.Email = app.Email;
            ViewBag.PhoneNumber = app.Phone;
            ViewBag.Gender = app.Gender;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(ProfileViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            if (!ModelState.IsValid)
            {
                PopulateStepViewBag(app);
                return View("Profile", model);
            }

            app.FullName = model.FullName.Trim();
            app.Phone = model.PhoneNumber.Trim();
            app.Gender = model.Gender;
            app.ApplicationStep = Math.Max(app.ApplicationStep, 2);
            app.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction("CompanyInfo");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfileDraft(ProfileViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.FullName = model.FullName?.Trim() ?? app.FullName;
            app.Phone = model.PhoneNumber?.Trim() ?? app.Phone;
            app.Gender = model.Gender ?? app.Gender;
            app.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ─────────────────────────────────
        // STEP 2 – COMPANY INFO
        // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CompanyInfo()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 2) return RedirectToAction("Profile");

            PopulateStepViewBag(app);
            ViewBag.CompanyName = app.CompanyName;
            ViewBag.CompanyUrl = app.CompanyUrl;
            ViewBag.TwitterHandle = app.SocialMedia?.Twitter;
            ViewBag.InstagramHandle = app.SocialMedia?.Instagram;
            ViewBag.LinkedInHandle = app.SocialMedia?.LinkedIn;
            ViewBag.FacebookHandle = app.SocialMedia?.Facebook;
            ViewBag.CompanyDescription = app.CompanyDescription;
            ViewBag.BusinessAddress = app.BusinessAddress;
            ViewBag.IsRegisteredInNigeria = app.IsLegallyRegisteredInNigeria ? "Yes" : "No";
            ViewBag.RegistrationNumber = app.CacRegistrationNumber;
            ViewBag.IsOperationalEntity = app.IsNigerianEntityOperational ? "Yes" : "No";
            ViewBag.HasForeignAffiliates = app.HasForeignAffiliates ? "Yes" : "No";
            ViewBag.AffiliateDetails = app.ForeignAffiliateDetails;
            ViewBag.YearOfIncorporation = app.YearOfIncorporation;
            ViewBag.Milestones = ParseList(app.Milestones);
            ViewBag.SuccessMetrics = ParseList(app.SuccessMetrics);
            ViewBag.LongTermVision = ParseList(app.LongTermVision);
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCompanyInfo(CompanyInfoViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            if (!ModelState.IsValid)
            {
                PopulateStepViewBag(app);
                return View("CompanyInfo", model);
            }

            app.CompanyName = model.CompanyName.Trim();
            app.CompanyUrl = model.CompanyUrl?.Trim();
            app.SocialMedia = new SocialMedia
            {
                Twitter = model.TwitterHandle?.Trim(),
                Instagram = model.InstagramHandle?.Trim(),
                LinkedIn = model.LinkedInHandle?.Trim(),
                Facebook = model.FacebookHandle?.Trim(),
            };
            app.CompanyDescription = model.CompanyDescription?.Trim();
            app.BusinessAddress = model.BusinessAddress.Trim();
            app.IsLegallyRegisteredInNigeria = model.IsRegisteredInNigeria == "Yes";
            app.CacRegistrationNumber = model.RegistrationNumber?.Trim();
            app.IsNigerianEntityOperational = model.IsOperationalEntity == "Yes";
            app.HasForeignAffiliates = model.HasForeignAffiliates == "Yes";
            app.ForeignAffiliateDetails = model.AffiliateDetails?.Trim();
            app.YearOfIncorporation = model.YearOfIncorporation;
            app.Milestones = string.Join(",", model.Milestones);
            app.SuccessMetrics = string.Join(",", model.SuccessMetrics);
            app.LongTermVision = string.Join(",", model.LongTermVision);
            app.ApplicationStep = Math.Max(app.ApplicationStep, 3);
            app.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction("TeamInfo");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCompanyInfoDraft(CompanyInfoViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();
            await ApplyCompanyInfoDraft(app, model);
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
            if (app.ApplicationStep < 3) return RedirectToAction("CompanyInfo");

            PopulateStepViewBag(app);
            ViewBag.NumberOfFounders = app.NumberOfFounders;
            ViewBag.TotalEmployees = app.TotalEmployees;
            ViewBag.TeamComposition = ParseList(app.TeamComposition);
            ViewBag.Founders = app.Founders
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new
                {
                    name = f.FullName,
                    phone = f.PhoneNumber,
                    role = f.Role,
                    linkedin = f.LinkedInUrl,
                    nationality = f.Nationality,
                }).ToList();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTeamInfo(TeamInfoViewModel model)
        {
            var app = await GetCurrentApplicationAsync(includeFounders: true);
            if (app == null) return RedirectToLogin();

            if (!ModelState.IsValid)
            {
                PopulateStepViewBag(app);
                return View("TeamInfo", model);
            }

            app.NumberOfFounders = model.NumberOfFounders;
            app.TotalEmployees = model.TotalEmployees;
            app.TeamComposition = string.Join(",", model.TeamComposition);
            app.ApplicationStep = Math.Max(app.ApplicationStep, 4);
            app.UpdatedAt = DateTime.Now;

            // Get existing founders from DB
            var existingFounders = await _db.Founders
                .Where(f => f.ApplicationId == app.Id)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            var submittedFounders = model.Founders
                .Where(f => !string.IsNullOrWhiteSpace(f.Name))
                .ToList();

            for (int i = 0; i < submittedFounders.Count; i++)
            {
                var submitted = submittedFounders[i];

                if (i < existingFounders.Count)
                {
                    // Update existing record in place — no delete/insert
                    var existing = existingFounders[i];
                    existing.FullName = submitted.Name.Trim();
                    existing.PhoneNumber = submitted.PhoneNumber?.Trim();
                    existing.Role = submitted.Role?.Trim();
                    existing.LinkedInUrl = submitted.LinkedInUrl?.Trim();
                    existing.Nationality = submitted.Nationality?.Trim();
                    existing.DisplayOrder = i + 1;
                }
                else
                {
                    // More founders submitted than existed — add the new ones
                    _db.Founders.Add(new Founder
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = app.Id,
                        FullName = submitted.Name.Trim(),
                        PhoneNumber = submitted.PhoneNumber?.Trim(),
                        Role = submitted.Role?.Trim(),
                        LinkedInUrl = submitted.LinkedInUrl?.Trim(),
                        Nationality = submitted.Nationality?.Trim(),
                        DisplayOrder = i + 1,
                    });
                }
            }

            // Fewer founders submitted than existed — remove the extras
            if (existingFounders.Count > submittedFounders.Count)
            {
                var toRemove = existingFounders.Skip(submittedFounders.Count).ToList();
                _db.Founders.RemoveRange(toRemove);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("ProductInfo");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTeamInfoDraft(TeamInfoViewModel model)
        {
            var app = await GetCurrentApplicationAsync(includeFounders: true);
            if (app == null) return Unauthorized();

            app.NumberOfFounders = model.NumberOfFounders ?? app.NumberOfFounders;
            app.TotalEmployees = model.TotalEmployees ?? app.TotalEmployees;
            app.TeamComposition = string.Join(",", model.TeamComposition);
            app.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ─────────────────────────────────
        // STEP 4 – PRODUCT INFO
        // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ProductInfo()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 4) return RedirectToAction("TeamInfo");

            PopulateStepViewBag(app);
            ViewBag.GrowthStage = app.GrowthStage;
            ViewBag.Sector = app.Sector;
            ViewBag.MvpLink = app.MvpLink;
            ViewBag.ProductDescription = app.ProductDescription;
            ViewBag.UserCount = app.UserCountRange;
            ViewBag.BusinessModel = ParseList(app.BusinessModel);
            ViewBag.USP = ParseList(app.UniqueSellingPoint);
            ViewBag.Competitors = ParseList(app.MainCompetitors);
            ViewBag.GoToMarket = ParseList(app.GoToMarketStrategy);
            ViewBag.KeyFeatures = ParseList(app.KeyFeatures);
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProductInfo(ProductInfoViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            if (!ModelState.IsValid)
            {
                PopulateStepViewBag(app);
                return View("ProductInfo", model);
            }

            app.GrowthStage = model.GrowthStage;
            app.Sector = model.Sector;
            app.MvpLink = model.MvpLink?.Trim();
            app.ProductDescription = model.ProductDescription?.Trim();
            app.UserCountRange = model.UserCount;
            app.BusinessModel = string.Join(",", model.BusinessModel);
            app.UniqueSellingPoint = string.Join(",", model.USP);
            app.MainCompetitors = string.Join(",", model.Competitors);
            app.GoToMarketStrategy = string.Join(",", model.GoToMarket);
            app.KeyFeatures = string.Join(",", model.KeyFeatures);
            app.ApplicationStep = Math.Max(app.ApplicationStep, 5);
            app.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction("Commercial");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProductInfoDraft(ProductInfoViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();

            app.GrowthStage = model.GrowthStage ?? app.GrowthStage;
            app.Sector = model.Sector ?? app.Sector;
            app.MvpLink = model.MvpLink?.Trim() ?? app.MvpLink;
            app.ProductDescription = model.ProductDescription?.Trim() ?? app.ProductDescription;
            app.UserCountRange = model.UserCount ?? app.UserCountRange;
            app.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // ─────────────────────────────────
        // STEP 5 – COMMERCIAL
        // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Commercial()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 5) return RedirectToAction("ProductInfo");

            PopulateStepViewBag(app);
            ViewBag.GeneratingRevenue = app.HasStartedGeneratingRevenue == true ? "Yes" : app.HasStartedGeneratingRevenue == false ? "No" : null;
            ViewBag.FundingTypes = ParseList(app.FundingTypes);
            ViewBag.CurrentlyFundraising = app.IsCurrentlyFundraising == true ? "Yes" : app.IsCurrentlyFundraising == false ? "No" : null;
            ViewBag.Valuation = app.CompanyValuation;
            ViewBag.ProjectedRevenue = app.ProjectedRevenueNextYear;
            ViewBag.RevenueStreams = app.RevenueStreams;
            ViewBag.GrossMargins = app.GrossMargins;
            ViewBag.RepeatRevenue = app.RepeatCustomerRevenuePercentage;
            ViewBag.PricingStrategy = app.PricingStrategy;
            ViewBag.Runway = app.OperatingRunway;
            ViewBag.DemandEvidence = app.DemandEvidence;
            ViewBag.MarketShare = app.EstimatedMarketShare;
            ViewBag.CompetitiveEdge = app.PrimaryCompetitiveEdge;
            ViewBag.GeoScalability = app.GeographicScalability;
            ViewBag.CrossIndustry = app.CrossIndustryApplicability;
            ViewBag.GrowthPlan = app.LongTermGrowthPlan;
            ViewBag.UpdateFrequency = app.FeedbackUpdateFrequency;
            ViewBag.NewCustomers = app.CustomersAcquiredPastSixMonths;
            ViewBag.GrowthRate = app.CustomerGrowthRatePastYear;
            ViewBag.CAC = app.AverageCustomerAcquisitionCost;
            ViewBag.SupplyChain = app.SupplyChainReliability;
            ViewBag.Compliance = app.RegulatoryCompliance;
            ViewBag.Risks = ParseList(app.BiggestRisks);
            ViewBag.Partnerships = app.ActivePartnerships;
            ViewBag.IPOwnership = app.IpOwnership;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCommercial(CommercialViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            await ApplyCommercial(app, model);
            app.ApplicationStep = Math.Max(app.ApplicationStep, 6);
            app.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction("Impact");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCommercialDraft(CommercialViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();
            ApplyCommercial(app, model);
            app.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // ─────────────────────────────────
        // STEP 6 – IMPACT & SUSTAINABILITY
        // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Impact()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 6) return RedirectToAction("Commercial");

            PopulateStepViewBag(app);
            ViewBag.SDGAligned = app.AlignsWithUnSdgs == true ? "Yes" : app.AlignsWithUnSdgs == false ? "No" : null;
            ViewBag.SDGCount = app.SdgsAddressed;
            ViewBag.EnvImpact = app.ReducesEnvironmentalHarm == true ? "Yes" : app.ReducesEnvironmentalHarm == false ? "No" : null;
            ViewBag.EnvReduction = app.EnvironmentalHarmReduction;
            ViewBag.ResourceUse = app.ResourceOptimisation;
            ViewBag.UnderservedMarket = app.UnderservedMarketPercentage;
            ViewBag.Inequities = app.SystemicInequalityReduction;
            ViewBag.GenderGaps = app.GenderGapApproach;
            ViewBag.AccessUnderserved = app.AccessForUnderservedGroups;
            ViewBag.JobsCreated = app.JobsCreated;
            ViewBag.PeopleImpacted = app.PeopleImpacted;
            ViewBag.SelfReliance = app.UserSelfRelianceLevel;
            ViewBag.OutcomeTracking = app.SocialOutcomeTracking;
            ViewBag.ImpactMeasurement = app.ImpactMeasurementMethod;
            ViewBag.ImpactSharing = app.ImpactDataSharingLevel;
            ViewBag.BeneficiaryInvolvement = app.BeneficiaryInvolvementLevel;
            ViewBag.LocalContext = app.LocalContextTailoring;
            ViewBag.Ethics = app.EthicalPracticesApproach;
            ViewBag.DataProtection = app.DataProtectionApproach;
            ViewBag.TrustBuilding = app.TrustBuildingApproach;
            ViewBag.Replicability = app.ModelReplicability;
            ViewBag.ImpactDurability = app.ImpactDurability;
            ViewBag.CrisisPerformance = app.CrisisPerformance;
            ViewBag.PolicyAdvocacy = app.PolicyAdvocacy;
            ViewBag.ImpactData = app.ImpactDataAndStatistics;
            ViewBag.ImpactExamples = app.MeasurableCommunityDifferences;
            ViewBag.TopDetails = app.TopImpactExamplesDetails;
            ViewBag.AgreePrivacy = app.AgreesToNsiaPrivacyPolicy;
            ViewBag.AgreeCompetition = app.AgreesToCompetitionSubmissionAgreement;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveImpact(ImpactViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            ApplyImpact(app, model);
            app.ApplicationStep = Math.Max(app.ApplicationStep, 7);
            app.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction("Additional");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveImpactDraft(ImpactViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return Unauthorized();
            await ApplyImpact(app, model);
            app.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // ─────────────────────────────────
        // STEP 7 – ADDITIONAL INFORMATION
        // ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Additional()
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();
            if (app.ApplicationStep < 7) return RedirectToAction("Impact");

            PopulateStepViewBag(app);
            ViewBag.DocumentDetails = app.DocumentDetails;
            ViewBag.AdditionalInformation = app.AdditionalInformation;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAdditional(AdditionalViewModel model)
        {
            var app = await GetCurrentApplicationAsync();
            if (app == null) return RedirectToLogin();

            // Save text fields
            app.DocumentDetails = model.DocumentDetails?.Trim();
            app.AdditionalInformation = model.AdditionalInformation?.Trim();
            app.CreatedAt = DateTime.Now;
            app.UpdatedAt = DateTime.Now;

            // Save uploaded files
            if (model.Documents != null && model.Documents.Count > 0)
            {
                var existingCount = await _db.ApplicationDocuments
                    .CountAsync(d => d.ApplicationId == app.Id);

                foreach (var file in model.Documents.Take(5 - existingCount))
                {
                    try
                    {
                        var (storedPath, originalName) =
                            await _fileService.SaveDocumentAsync(file, app.Id);

                        _db.ApplicationDocuments.Add(new ApplicationDocument
                        {
                            ApplicationId = app.Id,
                            OriginalFileName = originalName,
                            StoredFilePath = storedPath,
                            FileExtension = Path.GetExtension(originalName).ToLowerInvariant(),
                            FileSizeBytes = file.Length,
                            DocumentType = "Other",
                        });
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning("File rejected: {Message}", ex.Message);
                    }
                }
            }

            await _db.SaveChangesAsync();

            // Final submit
            app.Status = "Submitted";
            app.ApplicationStep = Math.Max(app.ApplicationStep, 8);
            app.SubmittedAt = DateTime.Now;
            app.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            await _email.SendSubmissionConfirmationAsync(
                app.Email, app.FullName, app.ReferenceNumber!);

            return RedirectToAction("Submitted");
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

        private async Task<Application?> GetCurrentApplicationAsync(
            bool includeFounders = false)
        {
            var idStr = HttpContext.Session.GetString("ApplicationId");
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var id))
                return null;

            var query = _db.Applications.AsTracking();

            if (includeFounders)
                query = query.Include(a => a.Founders);

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

        private static Task ApplyCommercial(Application app, CommercialViewModel m)
        {
            app.HasStartedGeneratingRevenue = m.GeneratingRevenue == "Yes" ? true : m.GeneratingRevenue == "No" ? false : null;
            app.FundingTypes = string.Join(",", m.FundingTypes);
            app.IsCurrentlyFundraising = m.CurrentlyFundraising == "Yes" ? true : m.CurrentlyFundraising == "No" ? false : null;
            app.CompanyValuation = m.Valuation;
            app.ProjectedRevenueNextYear = m.ProjectedRevenue;
            app.RevenueStreams = m.RevenueStreams;
            app.GrossMargins = m.GrossMargins;
            app.RepeatCustomerRevenuePercentage = m.RepeatRevenue;
            app.PricingStrategy = m.PricingStrategy;
            app.OperatingRunway = m.Runway;
            app.DemandEvidence = m.DemandEvidence;
            app.EstimatedMarketShare = m.MarketShare;
            app.PrimaryCompetitiveEdge = m.CompetitiveEdge;
            app.GeographicScalability = m.GeoScalability;
            app.CrossIndustryApplicability = m.CrossIndustry;
            app.LongTermGrowthPlan = m.GrowthPlan;
            app.FeedbackUpdateFrequency = m.UpdateFrequency;
            app.CustomersAcquiredPastSixMonths = m.NewCustomers;
            app.CustomerGrowthRatePastYear = m.GrowthRate;
            app.AverageCustomerAcquisitionCost = m.CAC;
            app.SupplyChainReliability = m.SupplyChain;
            app.RegulatoryCompliance = m.Compliance;
            app.BiggestRisks = string.Join(",", m.Risks);
            app.ActivePartnerships = m.Partnerships;
            app.IpOwnership = m.IPOwnership;

            return Task.CompletedTask;
        }

        private static Task ApplyImpact(Application app, ImpactViewModel m)
        {
            app.AlignsWithUnSdgs = m.SDGAligned == "Yes" ? true : m.SDGAligned == "No" ? false : null;
            app.SdgsAddressed = m.SDGCount;
            app.ReducesEnvironmentalHarm = m.EnvImpact == "Yes" ? true : m.EnvImpact == "No" ? false : null;
            app.EnvironmentalHarmReduction = m.EnvReduction;
            app.ResourceOptimisation = m.ResourceUse;
            app.UnderservedMarketPercentage = m.UnderservedMarket;
            app.SystemicInequalityReduction = m.Inequities;
            app.GenderGapApproach = m.GenderGaps;
            app.AccessForUnderservedGroups = m.AccessUnderserved;
            app.JobsCreated = m.JobsCreated;
            app.PeopleImpacted = m.PeopleImpacted;
            app.UserSelfRelianceLevel = m.SelfReliance;
            app.SocialOutcomeTracking = m.OutcomeTracking;
            app.ImpactMeasurementMethod = m.ImpactMeasurement;
            app.ImpactDataSharingLevel = m.ImpactSharing;
            app.BeneficiaryInvolvementLevel = m.BeneficiaryInvolvement;
            app.LocalContextTailoring = m.LocalContext;
            app.EthicalPracticesApproach = m.Ethics;
            app.DataProtectionApproach = m.DataProtection;
            app.TrustBuildingApproach = m.TrustBuilding;
            app.ModelReplicability = m.Replicability;
            app.ImpactDurability = m.ImpactDurability;
            app.CrisisPerformance = m.CrisisPerformance;
            app.PolicyAdvocacy = m.PolicyAdvocacy;
            app.ImpactDataAndStatistics = m.ImpactData?.Trim();
            app.MeasurableCommunityDifferences = m.ImpactExamples?.Trim();
            app.TopImpactExamplesDetails = m.TopDetails?.Trim();
            app.AgreesToNsiaPrivacyPolicy = m.AgreePrivacy;
            app.AgreesToCompetitionSubmissionAgreement = m.AgreeCompetition;

            return Task.CompletedTask;
        }

        private static Task ApplyCompanyInfoDraft(Application app, CompanyInfoViewModel m)
        {
            if (!string.IsNullOrEmpty(m.CompanyName)) app.CompanyName = m.CompanyName.Trim();
            if (!string.IsNullOrEmpty(m.CompanyUrl)) app.CompanyUrl = m.CompanyUrl.Trim();
            if (!string.IsNullOrEmpty(m.CompanyDescription)) app.CompanyDescription = m.CompanyDescription.Trim();
            if (!string.IsNullOrEmpty(m.BusinessAddress)) app.BusinessAddress = m.BusinessAddress.Trim();

            app.SocialMedia = new SocialMedia
            {
                Twitter = m.TwitterHandle?.Trim(),
                Instagram = m.InstagramHandle?.Trim(),
                LinkedIn = m.LinkedInHandle?.Trim(),
                Facebook = m.FacebookHandle?.Trim(),
            };

            return Task.CompletedTask;
        }
    }
}