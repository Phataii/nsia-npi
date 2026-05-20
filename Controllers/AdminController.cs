using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nsia.Data;
using nsia.Models;
using nsia.Services;

namespace Nsia.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AdminController> _logger;
        private readonly INinEncryptionService _ninService;

        public AdminController(ApplicationDbContext db, ILogger<AdminController> logger, INinEncryptionService ninService)
        {
            _db = db;
            _logger = logger;
            _ninService = ninService;
        }

        // ─────────────────────────────────
        // AUTH
        // ─────────────────────────────────

        [HttpGet("/admin/login")]
        public IActionResult Login() =>
            IsAdminLoggedIn() ? RedirectToAction("Dashboard") : View();

        [HttpPost("/admin/login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var admin = await _db.AdminUsers
                .FirstOrDefaultAsync(a => a.Email == email.ToLowerInvariant() && a.IsActive);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            admin.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            HttpContext.Session.SetString("AdminId", admin.Id.ToString());
            HttpContext.Session.SetString("AdminName", admin.FullName);
            HttpContext.Session.SetString("AdminRole", admin.Role);

            return RedirectToAction("Dashboard");
        }

        [HttpGet("/admin/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminId");
            HttpContext.Session.Remove("AdminName");
            HttpContext.Session.Remove("AdminRole");
            return Redirect("/admin/login");
        }

        // ─────────────────────────────────
        // DASHBOARD
        // ─────────────────────────────────

        [HttpGet("/admin")]
        [HttpGet("/admin/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdminLoggedIn()) return Redirect("/admin/login");

            var apps = await _db.Applications.ToListAsync();

            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminRole = HttpContext.Session.GetString("AdminRole");

            // ── Stats
            ViewBag.Total = apps.Count;
            ViewBag.Submitted = apps.Count(a => a.Status == "Submitted");
            ViewBag.Drafts = apps.Count(a => a.Status == "Draft");
            ViewBag.Today = apps.Count(a => a.CreatedAt.Date == DateTime.UtcNow.Date);

            // ── By Status (for donut chart)
            ViewBag.ByStatus = apps
                .GroupBy(a => a.Status ?? "Draft")
                .ToDictionary(g => g.Key, g => g.Count());

            // ── By Sector (for bar chart)
            ViewBag.BySector = apps
                .Where(a => !string.IsNullOrEmpty(a.BusinessSector))
                .GroupBy(a => a.BusinessSector!)
                .ToDictionary(g => g.Key, g => g.Count());

            // ── Recent 10
            ViewBag.RecentApps = apps
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new
                {
                    a.Id,
                    a.ReferenceNumber,
                    a.FullName,
                    a.Email,
                    a.CompanyName,
                    a.BusinessSector,
                    a.Status,
                    a.CreatedAt,
                })
                .ToList();

            return View();
        }

        // ─────────────────────────────────
        // APPLICATIONS LIST
        // ─────────────────────────────────

        [HttpGet("/admin/applications")]
        public async Task<IActionResult> Applications(
            string? search, string? status, string? sector,
            string? stage, int page = 1)
        {
            if (!IsAdminLoggedIn()) return Redirect("/admin/login");

            const int pageSize = 20;

            var query = _db.Applications.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(a =>
                    a.FullName.ToLower().Contains(s) ||
                    a.Email.ToLower().Contains(s) ||
                    (a.CompanyName != null && a.CompanyName.ToLower().Contains(s)) ||
                    (a.ReferenceNumber != null && a.ReferenceNumber.ToLower().Contains(s)));
            }

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            // if (!string.IsNullOrEmpty(sector))
            //     query = query.Where(a => a.Sector == sector);

            if (!string.IsNullOrEmpty(stage))
                query = query.Where(a => a.GrowthStage == stage);

            var total = await query.CountAsync();

            var applications = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.Applications = applications;
            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Search = search;
            ViewBag.StatusFilter = status;
            ViewBag.SectorFilter = sector;
            ViewBag.StageFilter = stage;

            // ViewBag.Sectors = await _db.Applications
            //     .Where(a => a.Sector != null)
            //     .Select(a => a.Sector).Distinct().ToListAsync();

            ViewBag.Stages = await _db.Applications
                .Where(a => a.GrowthStage != null)
                .Select(a => a.GrowthStage).Distinct().ToListAsync();

            return View();
        }

        // ─────────────────────────────────
        // APPLICATION DETAIL
        // ─────────────────────────────────

        [HttpGet("/admin/applications/{id}")]
        public async Task<IActionResult> ApplicationDetail(Guid id)
        {

            if (!IsAdminLoggedIn()) return Redirect("/admin/login");

            var app = await _db.Applications
                .Include(a => a.Founders)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound();

            // In ApplicationDetail action, after fetching the app:
            if (!string.IsNullOrEmpty(app.NinEncrypted))
                ViewBag.DecryptedNin = _ninService.Decrypt(app.NinEncrypted);

            // Also update Dashboard to pass required ViewBag data:
            ViewBag.Total = await _db.Applications.CountAsync();
            ViewBag.Submitted = await _db.Applications.CountAsync(a => a.Status == "Submitted");
            ViewBag.Drafts = await _db.Applications.CountAsync(a => a.Status == "Draft");
            ViewBag.Today = await _db.Applications.CountAsync(a => a.CreatedAt.Date == DateTime.UtcNow.Date);
            ViewBag.ByStatus = await _db.Applications.GroupBy(a => a.Status).ToDictionaryAsync(g => g.Key, g => g.Count());
            ViewBag.BySector = await _db.Applications.Where(a => a.BusinessSector != null).GroupBy(a => a.BusinessSector!).ToDictionaryAsync(g => g.Key, g => g.Count());
            ViewBag.RecentApps = await _db.Applications.OrderByDescending(a => a.CreatedAt).Take(10)
                .Select(a => new { a.Id, a.ReferenceNumber, a.FullName, a.Email, a.CompanyName, a.BusinessSector, a.Status, a.CreatedAt })
                .ToListAsync();

            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.Application = app;
            return View();
        }

        // ─────────────────────────────────
        // UPDATE STATUS
        // ─────────────────────────────────

        [HttpPost("/admin/applications/{id}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            if (!IsAdminLoggedIn()) return Redirect("/admin/login");

            var app = await _db.Applications.FindAsync(id);
            if (app == null) return NotFound();

            var allowed = new[] { "Draft", "Submitted", "Under Review", "Shortlisted", "Rejected", "Winner" };
            if (!allowed.Contains(status)) return BadRequest();

            app.Status = status;
            app.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Application status updated to {status}.";
            return RedirectToAction("ApplicationDetail", new { id });
        }

        // ─────────────────────────────────
        // EXPORT CSV
        // ─────────────────────────────────

        [HttpGet("/admin/export")]
        public async Task<IActionResult> ExportCsv(
            string? status, string? sector, string? stage)
        {
            if (!IsAdminLoggedIn()) return Redirect("/admin/login");

            var query = _db.Applications
                .Include(a => a.Founders)
                .Include(a => a.Documents)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);
            // if (!string.IsNullOrEmpty(sector)) query = query.Where(a => a.Sector == sector);
            if (!string.IsNullOrEmpty(stage)) query = query.Where(a => a.GrowthStage == stage);

            var apps = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();

            var csv = BuildCsv(apps);
            var bytes = Encoding.UTF8.GetBytes(csv);
            var fileName = $"NPI_Applications_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // ─────────────────────────────────
        // ADMIN USERS (SuperAdmin only)
        // ─────────────────────────────────

        [HttpGet("/admin/users")]
        public async Task<IActionResult> AdminUsers()
        {
            if (!IsAdminLoggedIn()) return Redirect("/admin/login");
            if (HttpContext.Session.GetString("AdminRole") != "SuperAdmin")
                return Forbid();

            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminUsers = await _db.AdminUsers.OrderByDescending(a => a.CreatedAt).ToListAsync();
            return View();
        }

        [HttpPost("/admin/users/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdminUser(
            string fullName, string email, string password, string role)
        {
            if (!IsAdminLoggedIn()) return Redirect("/admin/login");
            if (HttpContext.Session.GetString("AdminRole") != "SuperAdmin")
                return Forbid();

            var exists = await _db.AdminUsers.AnyAsync(a => a.Email == email.ToLowerInvariant());
            if (exists)
            {
                TempData["Error"] = "An admin with this email already exists.";
                return RedirectToAction("AdminUsers");
            }

            _db.AdminUsers.Add(new AdminUser
            {
                FullName = fullName.Trim(),
                Email = email.ToLowerInvariant().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role == "SuperAdmin" ? "SuperAdmin" : "Admin",
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Admin user created successfully.";
            return RedirectToAction("AdminUsers");
        }

        [HttpPost("/admin/users/{id}/toggle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminUser(Guid id)
        {
            if (!IsAdminLoggedIn()) return Redirect("/admin/login");
            if (HttpContext.Session.GetString("AdminRole") != "SuperAdmin")
                return Forbid();

            var admin = await _db.AdminUsers.FindAsync(id);
            if (admin == null) return NotFound();

            admin.IsActive = !admin.IsActive;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Admin user {(admin.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction("AdminUsers");
        }

        // ─────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────

        private bool IsAdminLoggedIn() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId"));

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            value = value.Replace("\r", " ").Replace("\n", " ");
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        private static string BuildCsv(List<Application> apps)
        {
            var sb = new StringBuilder();

            // ── MAIN HEADERS ──
            sb.AppendLine(string.Join(",", new[]
            {
                // Identity
                "Reference Number","Status","Application Step","Submitted At","Created At",
                // Profile
                "Full Name","Email","Phone","Gender","Email Verified", "Location", "How Did You Hear",
                // Company
                "Company Name","Company URL","Business Address","Year Incorporated",
                "Legally Registered","CAC Reg Number","Is Operational Entity",
                "Has Foreign Affiliates","Foreign Affiliate Details",
                "Twitter","Instagram","LinkedIn","Facebook",
                "Company Description","Milestones","Success Metrics","Long Term Vision",
                // Team
                "Number of Founders","Team Composition","Total Employees",
                // Founders (up to 5)
                "Founder 1 Name","Founder 1 Phone","Founder 1 Role","Founder 1 LinkedIn","Founder 1 Nationality",
                "Founder 2 Name","Founder 2 Phone","Founder 2 Role","Founder 2 LinkedIn","Founder 2 Nationality",
                // "Founder 3 Name","Founder 3 Phone","Founder 3 Role","Founder 3 LinkedIn","Founder 3 Nationality",
                // "Founder 4 Name","Founder 4 Phone","Founder 4 Role","Founder 4 LinkedIn","Founder 4 Nationality",
                // "Founder 5 Name","Founder 5 Phone","Founder 5 Role","Founder 5 LinkedIn","Founder 5 Nationality",
                // Product
                "Growth Stage","Sector","MVP Link","Product Description","User Count Range",
                "Business Model","USP","Competitors","Go To Market","Key Features",
                // Commercial
                "Generating Revenue","Funding Types","Currently Fundraising",
                "Company Valuation","Projected Revenue","Revenue Streams",
                "Gross Margins","Repeat Revenue %","Pricing Strategy","Operating Runway",
                "Demand Evidence","Market Share","Competitive Edge",
                "Geographic Scalability","Cross Industry","Growth Plan",
                "Update Frequency","New Customers 6m","Customer Growth Rate",
                "Avg CAC","Supply Chain","Regulatory Compliance",
                "Biggest Risks","Active Partnerships","IP Ownership",
                // Impact
                "Aligns With SDGs","SDGs Addressed","Reduces Env Harm",
                "Env Harm Reduction","Resource Optimisation",
                "Underserved Market %","Systemic Inequality Reduction",
                "Gender Gap Approach","Access For Underserved",
                "Jobs Created","People Impacted","User Self Reliance",
                "Outcome Tracking","Impact Measurement","Impact Sharing",
                "Beneficiary Involvement","Local Context","Ethical Practices",
                "Data Protection","Trust Building",
                "Model Replicability","Impact Durability","Crisis Performance",
                "Policy Advocacy","Impact Data","Community Differences","Top Impact Examples",
                "Agrees Privacy Policy","Agrees Competition Agreement",
                // Additional
                "Document Details","Additional Information",
                // Documents (up to 5)
                "Doc 1 Name",
                "Doc 2 Name",
                "Doc 3 Name",
                "Doc 4 Name",
                "Doc 5 Name",
            }));

            foreach (var a in apps)
            {
                var founders = a.Founders.OrderBy(f => f.DisplayOrder).ToList();
                var docs = a.Documents.OrderBy(d => d.UploadedAt).ToList();

                string F(int i, Func<Founder, string?> selector) =>
                    EscapeCsv(i < founders.Count ? selector(founders[i]) : null);

                string D(int i, Func<ApplicationDocument, string?> selector) =>
                    EscapeCsv(i < docs.Count ? selector(docs[i]) : null);

                var row = new[]
                {
                    // Identity
                    EscapeCsv(a.ReferenceNumber),
                    EscapeCsv(a.Status),
                    a.ApplicationStep.ToString(),
                    a.SubmittedAt?.ToString("yyyy-MM-dd HH:mm") ?? "",
                    a.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    // Profile
                    EscapeCsv(a.FullName),
                    EscapeCsv(a.Email),
                    EscapeCsv(a.Phone),
                    EscapeCsv(a.Gender),
                    a.IsEmailVerified ? "Yes" : "No",
                    // EscapeCsv(a.Location),
                    // EscapeCsv(a.HowDidYouHear),
                    // // Company
                    // EscapeCsv(a.CompanyName),
                    // EscapeCsv(a.CompanyUrl),
                    // EscapeCsv(a.BusinessAddress),
                    // a.YearOfIncorporation?.ToString() ?? "",
                    // a.IsLegallyRegisteredInNigeria ? "Yes" : "No",
                    // EscapeCsv(a.CacRegistrationNumber),
                    // a.IsNigerianEntityOperational ? "Yes" : "No",
                    // a.HasForeignAffiliates ? "Yes" : "No",
                    // EscapeCsv(a.ForeignAffiliateDetails),
                    // EscapeCsv(a.SocialMedia?.Twitter),
                    // EscapeCsv(a.SocialMedia?.Instagram),
                    // EscapeCsv(a.SocialMedia?.LinkedIn),
                    // EscapeCsv(a.SocialMedia?.Facebook),
                    // EscapeCsv(a.CompanyDescription),
                    // EscapeCsv(a.Milestones),
                    // EscapeCsv(a.SuccessMetrics),
                    // EscapeCsv(a.LongTermVision),
                    // // Team
                    // a.NumberOfFounders?.ToString() ?? "",
                    // EscapeCsv(a.TeamComposition),
                    // a.TotalEmployees?.ToString() ?? "",
                    // Founders
                    F(0,f=>f.FullName), F(0,f=>f.PhoneNumber), F(0,f=>f.Role), F(0,f=>f.LinkedInUrl), F(0,f=>f.Nationality),
                    F(1,f=>f.FullName), F(1,f=>f.PhoneNumber), F(1,f=>f.Role), F(1,f=>f.LinkedInUrl), F(1,f=>f.Nationality),
                    // F(2,f=>f.FullName), F(2,f=>f.PhoneNumber), F(2,f=>f.Role), F(2,f=>f.LinkedInUrl), F(2,f=>f.Nationality),
                    // F(3,f=>f.FullName), F(3,f=>f.PhoneNumber), F(3,f=>f.Role), F(3,f=>f.LinkedInUrl), F(3,f=>f.Nationality),
                    // F(4,f=>f.FullName), F(4,f=>f.PhoneNumber), F(4,f=>f.Role), F(4,f=>f.LinkedInUrl), F(4,f=>f.Nationality),
                    // Product
                    EscapeCsv(a.GrowthStage),
                    // EscapeCsv(a.Sector),
                    // EscapeCsv(a.MvpLink),
                    // EscapeCsv(a.ProductDescription),
                    // EscapeCsv(a.UserCountRange),
                    // EscapeCsv(a.BusinessModel),
                    // EscapeCsv(a.UniqueSellingPoint),
                    // EscapeCsv(a.MainCompetitors),
                    // EscapeCsv(a.GoToMarketStrategy),
                    EscapeCsv(a.KeyFeatures),
                    // Commercial
                    // a.HasStartedGeneratingRevenue == true ? "Yes" : a.HasStartedGeneratingRevenue == false ? "No" : "",
                    // EscapeCsv(a.FundingTypes),
                    // a.IsCurrentlyFundraising == true ? "Yes" : a.IsCurrentlyFundraising == false ? "No" : "",
                    // EscapeCsv(a.CompanyValuation),
                    // EscapeCsv(a.ProjectedRevenueNextYear),
                    // EscapeCsv(a.RevenueStreams),
                    // EscapeCsv(a.GrossMargins),
                    // EscapeCsv(a.RepeatCustomerRevenuePercentage),
                    // EscapeCsv(a.PricingStrategy),
                    // EscapeCsv(a.OperatingRunway),
                    // EscapeCsv(a.DemandEvidence),
                    // EscapeCsv(a.EstimatedMarketShare),
                    // EscapeCsv(a.PrimaryCompetitiveEdge),
                    // EscapeCsv(a.GeographicScalability),
                    // EscapeCsv(a.CrossIndustryApplicability),
                    // EscapeCsv(a.LongTermGrowthPlan),
                    // EscapeCsv(a.FeedbackUpdateFrequency),
                    // EscapeCsv(a.CustomersAcquiredPastSixMonths),
                    // EscapeCsv(a.CustomerGrowthRatePastYear),
                    // EscapeCsv(a.AverageCustomerAcquisitionCost),
                    // EscapeCsv(a.SupplyChainReliability),
                    // EscapeCsv(a.RegulatoryCompliance),
                    // EscapeCsv(a.BiggestRisks),
                    // EscapeCsv(a.ActivePartnerships),
                    // EscapeCsv(a.IpOwnership),
                    // Impact
                    // a.AlignsWithUnSdgs == true ? "Yes" : a.AlignsWithUnSdgs == false ? "No" : "",
                    // EscapeCsv(a.SdgsAddressed),
                    // a.ReducesEnvironmentalHarm == true ? "Yes" : a.ReducesEnvironmentalHarm == false ? "No" : "",
                    // EscapeCsv(a.EnvironmentalHarmReduction),
                    // EscapeCsv(a.ResourceOptimisation),
                    // EscapeCsv(a.UnderservedMarketPercentage),
                    // EscapeCsv(a.SystemicInequalityReduction),
                    // EscapeCsv(a.GenderGapApproach),
                    // EscapeCsv(a.AccessForUnderservedGroups),
                    // EscapeCsv(a.JobsCreated),
                    // EscapeCsv(a.PeopleImpacted),
                    // EscapeCsv(a.UserSelfRelianceLevel),
                    // EscapeCsv(a.SocialOutcomeTracking),
                    // EscapeCsv(a.ImpactMeasurementMethod),
                    // EscapeCsv(a.ImpactDataSharingLevel),
                    // EscapeCsv(a.BeneficiaryInvolvementLevel),
                    // EscapeCsv(a.LocalContextTailoring),
                    // EscapeCsv(a.EthicalPracticesApproach),
                    // EscapeCsv(a.DataProtectionApproach),
                    // EscapeCsv(a.TrustBuildingApproach),
                    // EscapeCsv(a.ModelReplicability),
                    // EscapeCsv(a.ImpactDurability),
                    // EscapeCsv(a.CrisisPerformance),
                    // EscapeCsv(a.PolicyAdvocacy),
                    // EscapeCsv(a.ImpactDataAndStatistics),
                    // EscapeCsv(a.MeasurableCommunityDifferences),
                    // EscapeCsv(a.TopImpactExamplesDetails),
                    // a.AgreesToNsiaPrivacyPolicy ? "Yes" : "No",
                    // a.AgreesToCompetitionSubmissionAgreement ? "Yes" : "No",
                    // Additional
                    EscapeCsv(a.DocumentDetails),
                    EscapeCsv(a.AdditionalInformation),
                    // Documents
                    D(0,d=>d.StoredFilePath),
                    D(1,d=>d.StoredFilePath),
                    D(2,d=>d.StoredFilePath),
                    D(3,d=>d.StoredFilePath),
                    D(4,d=>d.StoredFilePath),
                };

                sb.AppendLine(string.Join(",", row));
            }

            return sb.ToString();
        }

    }
}