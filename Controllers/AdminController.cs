using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nsia.Data;
using nsia.Models;
using nsia.Services;

namespace nsia.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AdminController> _logger;
        private readonly INinEncryptionService _ninService;
        private readonly IScoringService _scoring;
        private readonly IEmailService _email;
        public AdminController(ApplicationDbContext db, ILogger<AdminController> logger, INinEncryptionService ninService, IScoringService scoring, IEmailService email)
        {
            _db = db;
            _logger = logger;
            _ninService = ninService;
            _scoring = scoring;
            _email = email;
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
            HttpContext.Session.SetString("AdminEmail", admin.Email);
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
            if (!IsAdminLoggedIn())
                return Redirect("/admin/login");

            var apps = await _db.Applications.ToListAsync();

            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminRole = HttpContext.Session.GetString("AdminRole");
            ViewBag.AdminEmail = HttpContext.Session.GetString("AdminEmail");

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

            // ── Recent Applications
            ViewBag.RecentApps = apps
                .OrderByDescending(a => a.CreatedAt)
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
                    Score = _scoring.Calculate(a).TotalScore
                })
                .ToList();

            ViewBag.Success = TempData["Success"];

            return View();
        }

        // ─────────────────────────────────
        // APPLICATIONS LIST
        // ─────────────────────────────────

        // [HttpGet("/admin/applications")]
        // public async Task<IActionResult> Applications(
        //     string? search, string? status, string? sector,
        //     string? stage, int page = 1)
        // {
        //     if (!IsAdminLoggedIn()) return Redirect("/admin/login");

        //     const int pageSize = 20;

        //     var query = _db.Applications.AsQueryable();

        //     if (!string.IsNullOrWhiteSpace(search))
        //     {
        //         var s = search.ToLower();
        //         query = query.Where(a =>
        //             a.FullName.ToLower().Contains(s) ||
        //             a.Email.ToLower().Contains(s) ||
        //             (a.CompanyName != null && a.CompanyName.ToLower().Contains(s)) ||
        //             (a.ReferenceNumber != null && a.ReferenceNumber.ToLower().Contains(s)));
        //     }

        //     if (!string.IsNullOrEmpty(status))
        //         query = query.Where(a => a.Status == status);

        //     // if (!string.IsNullOrEmpty(sector))
        //     //     query = query.Where(a => a.Sector == sector);

        //     if (!string.IsNullOrEmpty(stage))
        //         query = query.Where(a => a.GrowthStage == stage);

        //     var total = await query.CountAsync();

        //     var applications = await query
        //         .OrderByDescending(a => a.CreatedAt)
        //         .Skip((page - 1) * pageSize)
        //         .Take(pageSize)
        //         .ToListAsync();

        //     ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
        //     ViewBag.Applications = applications;
        //     ViewBag.Total = total;
        //     ViewBag.Page = page;
        //     ViewBag.PageSize = pageSize;
        //     ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        //     ViewBag.Search = search;
        //     ViewBag.StatusFilter = status;
        //     ViewBag.SectorFilter = sector;
        //     ViewBag.StageFilter = stage;

        //     // ViewBag.Sectors = await _db.Applications
        //     //     .Where(a => a.Sector != null)
        //     //     .Select(a => a.Sector).Distinct().ToListAsync();

        //     ViewBag.Stages = await _db.Applications
        //         .Where(a => a.GrowthStage != null)
        //         .Select(a => a.GrowthStage).Distinct().ToListAsync();

        //     return View();
        // }

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

            ViewBag.Score = _scoring.Calculate(app);  // ← calculated fresh

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
        // SEND REMINDER TO PENDING APPLICATIONS
        // ─────────────────────────────────

        [Route("send-reminder")]
        public async Task<IActionResult> SendReminder(
        )
        {
            try
            {
                // Find existing application
                var applications = await _db.Applications
                    .Where(a => a.Status != "Submitted").ToListAsync();

                if (applications == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Applications not found"
                    });
                }

                foreach (var application in applications)
                {
                    await _email.SendApplicationReminderEmailAsync(application.Email, application.FullName, application.ReferenceNumber);

                }

                // Return success response
                TempData["Success"] = "Reminders sent successfully!";

                return Redirect("/admin/dashboard");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sendig emails");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while sending emails. Please try again."
                });
            }
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

        private string BuildCsv(List<Application> apps)
        {
            var sb = new StringBuilder();
            var baseUrl = "https://nsia-ip.com/npi/uploads";
            // var baseUrl = "http://localhost:5228/npi/uploads";

            string BuildDocumentUrl(string? path)
            {
                return string.IsNullOrWhiteSpace(path)
                    ? ""
                    : $"{baseUrl}/{path}";
            }
            // ── MAIN HEADERS ──
            sb.AppendLine(string.Join(",", new[]
            {
               // ── META
                "Score",
                "Reference Number",
                "Application Status",
                // "Application Step",
                "Started At",
                "Last Updated At",
                "Submitted At",

                // ── PRE-SUBMISSION CHECKLIST
                "Registered In Nigeria",
                "Business Sector",
                "Country Of Origin",

                // ── PERSONAL INFORMATION
                "Full Name",
                "Email Address",
                "Phone Number",
                "Gender",
                "Email Verified",
                "Location",
                "How Did You Hear About NPI",
                "Relationship To Business",

                // ── COMPANY INFORMATION
                "Company Name",
                "Company Website",
                "Business State",
                "Business LGA",
                "Company HQ Address",
                "Geographic Scope",
                "Company Registration Number",
                "Regulatory Compliance",
                "Tax Compliance",
                "Has Foreign Affiliates",
                "Is Nigerian Entity Primary",
                "Company Structure",
                "Parent Organization Name",
                "Other Competitions Participated In",

                // ── SOCIAL MEDIA
                "LinkedIn",
                "Twitter",
                "Instagram",
                "Facebook",

                // ── TEAM INFORMATION
                "Number Of Founders",
                "Founding Team Type",
                "Founder Industry Experience",
                "Management Team Experience",
                "Total Full-Time Employees",

                // ── FOUNDERS
                "Founder 1 Name",
                "Founder 1 Phone",
                "Founder 1 Role",
                "Founder 1 LinkedIn",
                "Founder 1 Nationality",

                "Founder 2 Name",
                "Founder 2 Phone",
                "Founder 2 Role",
                "Founder 2 LinkedIn",
                "Founder 2 Nationality",

                "Founder 3 Name",
                "Founder 3 Phone",
                "Founder 3 Role",
                "Founder 3 LinkedIn",
                "Founder 3 Nationality",

                // ── PRODUCT, GROWTH & TRACTION
                "Growth Stage",
                "Key Milestones",
                "Existing Users",
                "Total Users Reached",
                "Core Business Model",
                "Unique Selling Point",
                "Main Competitors",
                "Market Penetration Strategy",
                "Key Features",

                // ── COMMERCIAL PART 1 — REVENUE/FUNDING
                "Started Generating Sales",
                "Year Of First Sale",
                "Yearly Sales Revenue",
                "Yearly Profit",
                "Proprietary Funding",
                "External Funding",
                "Types Of Funding",
                "Currently Fundraising",
                "Projected Revenue",
                "Company Valuation",

                // ── COMMERCIAL PART 2 — STRATEGY/MARKET
                "Demand Evidence",
                "Revenue Streams",
                "Geographic Scalability",
                "Gross Margins",
                "Primary Competitive Advantage",
                "Operating Runway",
                "Active Partnerships",
                "Regulatory Approach",
                "Cross Industry Application",
                "Long-Term Growth Strategy",
                "Supply Chain Reliability",
                "IP Ownership",
                "Pricing Strategy",
                "Biggest Risks",
                "New Customers (Last 6 Months)",
                "Customer Growth Rate",
                "Average CAC",
                "Repeat Customer Revenue",

                // ── SUSTAINABILITY
                "SDG Alignment",
                "Business Replicability",
                "Sustainability Integration",
                "Energy & Waste Reduction",
                "Sustainability Technology",
                "Scaling With Sustainability",
                "Climate Change Approach",
                "Digital Accessibility",

                // ── IMPACT
                "Underserved Market Percentage",
                "Systemic Inequality Approach",
                "Beneficiary Involvement",
                "Impact Data Sharing",
                "Jobs Created",
                "Gender Gap Approach",
                "Access For Underserved",
                "Resource Optimization",
                "Data Protection",
                "Population Impacted",
                "Social Good Contribution",
                "Ethical Operations",
                "Diversity & Inclusion",
                "Equitable Opportunities",
                "Accessibility For Disadvantaged",

                // ── ADDITIONAL INFORMATION
                "Document Details",
                "Additional Information",

                // ── AGREEMENTS
                "Agreed To Terms Of Service",
                "Agreed To Privacy Policy",
                "Agreed To Submission Agreement",

                // ── DOCUMENTS
                "Document 1",
                "Document 2",
                "Document 3",
            }));

            foreach (var a in apps)
            {
                var founders = a.Founders.OrderBy(f => f.DisplayOrder).ToList();
                var docs = a.Documents.OrderBy(d => d.UploadedAt).ToList();

                string F(int i, Func<Founder, string?> selector) =>
                    EscapeCsv(i < founders.Count ? selector(founders[i]) : null);

                string D(int i, Func<ApplicationDocument, string?> selector) =>
                    EscapeCsv(i < docs.Count ? selector(docs[i]) : null);

                // Get score for each application
                var score = _scoring.Calculate(a).TotalScore;

                var row = new[]
                {
                    // ── META
                    score.ToString(),
                    EscapeCsv(a.ReferenceNumber),
                    EscapeCsv(a.Status),
                    // a.ApplicationStep.ToString(),
                    a.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    a.UpdatedAt.ToString("yyyy-MM-dd HH:mm"),
                    a.SubmittedAt?.AddHours(1).ToString("yyyy-MM-dd HH:mm") ?? "",

                    // ── PRE-SUBMISSION CHECKLIST
                    EscapeCsv(a.IsRegisteredInNigeria),
                    EscapeCsv(a.BusinessSector),
                    EscapeCsv(a.CountryOfOrigin),

                    // ── PERSONAL INFORMATION
                    EscapeCsv(a.FullName),
                    EscapeCsv(a.Email),
                    EscapeCsv(a.Phone),
                    EscapeCsv(a.Gender),
                    a.IsEmailVerified ? "Yes" : "No",
                    EscapeCsv(a.Location),
                    EscapeCsv(a.HowDidYouHear),
                    EscapeCsv(a.RelationshipToBusiness),

                    // ── COMPANY INFORMATION
                    EscapeCsv(a.CompanyName),
                    EscapeCsv(a.CompanyWebsite),
                    EscapeCsv(a.BusinessState),
                    EscapeCsv(a.BusinessLga),
                    EscapeCsv(a.CompanyHqAddress),
                    EscapeCsv(a.GeographicScope),
                    EscapeCsv(a.CompanyRegistrationNumber),
                    EscapeCsv(a.RegulatoryCompliance),
                    EscapeCsv(a.TaxCompliance),
                    EscapeCsv(a.HasForeignAffiliates),
                    EscapeCsv(a.IsNigerianEntityPrimary),
                    EscapeCsv(a.CompanyStructure),
                    EscapeCsv(a.ParentOrganizationName),
                    EscapeCsv(a.OtherCompetitions),

                    // ── SOCIAL MEDIA
                    EscapeCsv(a.SocialMedia?.LinkedIn),
                    EscapeCsv(a.SocialMedia?.Twitter),
                    EscapeCsv(a.SocialMedia?.Instagram),
                    EscapeCsv(a.SocialMedia?.Facebook),

                    // ── TEAM INFORMATION
                    EscapeCsv(a.NumberOfFounders),
                    EscapeCsv(a.FoundingTeamType),
                    EscapeCsv(a.FounderIndustryExperience),
                    EscapeCsv(a.ManagementTeamExperience),
                    EscapeCsv(a.TotalFullTimeEmployees),

                    // ── FOUNDERS
                    F(0, f => f.FullName),
                    F(0, f => f.PhoneNumber),
                    F(0, f => f.Role),
                    F(0, f => f.LinkedInUrl),
                    F(0, f => f.Nationality),

                    F(1, f => f.FullName),
                    F(1, f => f.PhoneNumber),
                    F(1, f => f.Role),
                    F(1, f => f.LinkedInUrl),
                    F(1, f => f.Nationality),

                    F(2, f => f.FullName),
                    F(2, f => f.PhoneNumber),
                    F(2, f => f.Role),
                    F(2, f => f.LinkedInUrl),
                    F(2, f => f.Nationality),

                    // ── PRODUCT, GROWTH & TRACTION
                    EscapeCsv(a.GrowthStage),
                    EscapeCsv(a.KeyMilestones),
                    EscapeCsv(a.ExistingUsers),
                    EscapeCsv(a.TotalUsersReached),
                    EscapeCsv(a.CoreBusinessModel),
                    EscapeCsv(a.UniqueSellingPoint),
                    EscapeCsv(a.MainCompetitors),
                    EscapeCsv(a.MarketPenetrationStrategy),
                    EscapeCsv(a.KeyFeatures),

                    // ── COMMERCIAL PART 1
                    EscapeCsv(a.HasStartedGeneratingSales),
                    EscapeCsv(a.YearOfFirstSale),
                    EscapeCsv(a.YearlySalesRevenue),
                    EscapeCsv(a.YearlyProfit),
                    EscapeCsv(a.ProprietaryFunding),
                    EscapeCsv(a.ExternalFunding),
                    EscapeCsv(a.TypesOfFunding),
                    EscapeCsv(a.IsCurrentlyFundraising),
                    EscapeCsv(a.ProjectedRevenue),
                    EscapeCsv(a.CompanyValuation),

                    // ── COMMERCIAL PART 2
                    EscapeCsv(a.DemandEvidence),
                    EscapeCsv(a.RevenueStreams),
                    EscapeCsv(a.GeographicScalability),
                    EscapeCsv(a.GrossMargins),
                    EscapeCsv(a.PrimaryCompetitiveAdvantage),
                    EscapeCsv(a.OperatingRunway),
                    EscapeCsv(a.ActivePartnerships),
                    EscapeCsv(a.RegulatoryApproach),
                    EscapeCsv(a.CrossIndustryApplication),
                    EscapeCsv(a.LongTermGrowthStrategy),
                    EscapeCsv(a.SupplyChainReliability),
                    EscapeCsv(a.IpOwnership),
                    EscapeCsv(a.PricingStrategy),
                    EscapeCsv(a.BiggestRisks),
                    EscapeCsv(a.NewCustomersSixMonths),
                    EscapeCsv(a.CustomerGrowthRate),
                    EscapeCsv(a.AverageCAC),
                    EscapeCsv(a.RepeatCustomerRevenue),

                    // ── SUSTAINABILITY
                    EscapeCsv(a.SdgAlignment),
                    EscapeCsv(a.BusinessReplicability),
                    EscapeCsv(a.SustainabilityIntegration),
                    EscapeCsv(a.EnergyWasteReduction),
                    EscapeCsv(a.SustainabilityTechnology),
                    EscapeCsv(a.ScalingWithSustainability),
                    EscapeCsv(a.ClimateChangeApproach),
                    EscapeCsv(a.DigitalAccessibility),

                    // ── IMPACT
                    EscapeCsv(a.UnderservedMarketPercentage),
                    EscapeCsv(a.SystemicInequalityApproach),
                    EscapeCsv(a.BeneficiaryInvolvement),
                    EscapeCsv(a.ImpactDataSharing),
                    EscapeCsv(a.JobsCreated),
                    EscapeCsv(a.GenderGapApproach),
                    EscapeCsv(a.AccessForUnderserved),
                    EscapeCsv(a.ResourceOptimization),
                    EscapeCsv(a.DataProtection),
                    EscapeCsv(a.PopulationImpacted),
                    EscapeCsv(a.SocialGoodContribution),
                    EscapeCsv(a.EthicalOperations),
                    EscapeCsv(a.DiversityInclusion),
                    EscapeCsv(a.EquitableOpportunities),
                    EscapeCsv(a.AccessibilityForDisadvantaged),

                    // ── ADDITIONAL
                    EscapeCsv(a.DocumentDetails),
                    EscapeCsv(a.AdditionalInformation),

                    // ── AGREEMENTS
                    a.AgreesToTermsOfService ? "Yes" : "No",
                    a.AgreesToPrivacyPolicy ? "Yes" : "No",
                    a.AgreeToSubmissionAgreement ? "Yes" : "No",

                    // ── DOCUMENTS
                    BuildDocumentUrl(D(0, d => d.StoredFilePath)),
                    BuildDocumentUrl(D(1, d => d.StoredFilePath)),
                    BuildDocumentUrl(D(2, d => d.StoredFilePath)),
                    BuildDocumentUrl(D(3, d => d.StoredFilePath)),
                    BuildDocumentUrl(D(4, d => d.StoredFilePath)),
                }
            ;

                sb.AppendLine(string.Join(",", row));
            }

            return sb.ToString();
        }

    }
}