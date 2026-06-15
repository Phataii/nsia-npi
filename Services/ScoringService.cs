using nsia.Models;

namespace nsia.Services
{
    public class ScoringService : IScoringService
    {

        public ScoreResult Calculate(Application app)
        {
            // ── PRE-SUBMISSION GATE ──────────────────────────────
            // If not registered in Nigeria OR not Nigerian origin → zero score
            bool isRegistered = app.IsRegisteredInNigeria == "Yes";
            bool isNigerian = IsNigerian(app.CountryOfOrigin);

            if (!isRegistered || !isNigerian)
            {
                return new ScoreResult
                {
                    TotalScore = 0,
                    MaxScore = GetMaxScore(),
                    Percentage = 0,
                    Band = "Disqualified",
                    Sections = new List<ScoreSection>(),
                    // Disqualified = true,
                    // DisqualifiedReason = !isRegistered
                    //     ? "Company is not legally registered in Nigeria."
                    //     : "Applicant is not Nigerian.",
                };
            }

            var sections = new List<ScoreSection>
            {
                ScorePreChecklist(app),
                ScoreCompanyInfo(app),
                ScoreTeam(app),
                ScoreProduct(app),
                ScoreCommercialPart1(app),
                ScoreCommercialPart2(app),
                ScoreSustainability(app),
                ScoreImpact(app),
            };

            var total = sections.Sum(s => s.Score);
            var max = sections.Sum(s => s.MaxScore);

            foreach (var s in sections)
                s.Percentage = s.MaxScore > 0
                    ? Math.Round(s.Score / (double)s.MaxScore * 100, 1) : 0;

            // In Calculate(), add temporary logging:
            // var isRegistered = app.IsRegisteredInNigeria == "Yes";
            // var isNigerian = IsNigerian(app.CountryOfOrigin);

            // Add this temporarily to see what's happening:
            // Console.WriteLine($"[SCORE DEBUG] IsRegisteredInNigeria='{app.IsRegisteredInNigeria}' → {isRegistered}");
            // Console.WriteLine($"[SCORE DEBUG] CountryOfOrigin='{app.CountryOfOrigin}' → {isNigerian}");

            return new ScoreResult
            {
                TotalScore = total,
                MaxScore = max,
                Percentage = max > 0 ? Math.Round(total / (double)max * 100, 1) : 0,
                Band = GetBand(total, max),
                Sections = sections,
            };
        }

        // ── PRE-SUBMISSION CHECKLIST ─────────────────────── Max: 3
        private ScoreSection ScorePreChecklist(Application app)
        {
            var criteria = new List<ScoreCriterion>
            {
                // Q1 — already gated above, but show as scored
                C("Registered in Nigeria",
                  app.IsRegisteredInNigeria, 1,
                  v => v == "Yes" ? (1, null) : (0, "Not registered")),

                // Q2 — sector (any valid sector = 1 point)
                C("Business sector",
                  app.BusinessSector, 1,
                  v => v is "Manufacturing" or "Healthcare" or "Climate & Food Security"
                       ? (1, null) : (0, "Invalid sector")),

                // Q3 — already gated, show as scored
                C("Country of origin",
                  app.CountryOfOrigin, 1,
                  v => IsNigerian(v) ? (1, null) : (0, "Non-Nigerian")),
            };

            return Build("Pre-Submission Checklist", criteria);
        }

        // ── COMPANY INFORMATION ──────────────────────────── Max: 6
        private ScoreSection ScoreCompanyInfo(Application app)
        {
            var criteria = new List<ScoreCriterion>
            {
                // Website
                C("Company website",
                  app.CompanyWebsite, 5,
                  v => string.IsNullOrWhiteSpace(v) ? (0, "No website provided") : (5, null)),

                // Geographic scope
                C("Geographic scope",
                  app.GeographicScope, 5,
                  v => v switch {
                      "State-wide"    => (1, null),
                      "Regional"      => (2, null),
                      "Nationwide"    => (3, null),
                      "International" => (5, null),
                      _               => (0, "Not specified")
                  }),
            };

            return Build("Company Information", criteria);
        }

        // ── TEAM INFORMATION ─────────────────────────────── Max: 3
        private ScoreSection ScoreTeam(Application app)
        {
            var criteria = new List<ScoreCriterion>
            {
                // Number of founders
                C("Number of founders",
                  app.NumberOfFounders, 3,
                  v => v switch {
                      "1"  => (1, "Solo founder"),
                      "2"  => (3, "Optimal co-founder pair"),
                      "3"  => (2, null),
                      "4+" => (1, "Many founders — leadership clarity risk"),
                      _    => (0, "Not specified")
                  }),
            };

            return Build("Team Information", criteria);
        }

        // ── PRODUCT, GROWTH & TRACTION ───────────────────── Max: 40
        private ScoreSection ScoreProduct(Application app)
        {
            var criteria = new List<ScoreCriterion>
            {
                // Growth stage
                C("Growth stage",
                  app.GrowthStage, 10,
                  v => {
                      if (v.Contains("startup", StringComparison.OrdinalIgnoreCase) ||
                          v.Contains("startup stage", StringComparison.OrdinalIgnoreCase))
                          return (2, "Early validation");
                      if (v.Contains("growth", StringComparison.OrdinalIgnoreCase))
                          return (5, null);
                      if (v.Contains("established", StringComparison.OrdinalIgnoreCase))
                          return (8, null);
                      if (v.Contains("mature", StringComparison.OrdinalIgnoreCase))
                          return (10, "Strong market position");
                      return (0, "Not specified");
                  }),

                // Existing users
                C("Existing users/customers",
                  app.ExistingUsers, 10,
                  v => v switch {
                      var s when s.Contains("1-50")    => (2, "Early-stage traction"),
                      var s when s.Contains("51-200")  => (5, null),
                      var s when s.Contains("201-500") || s.Contains("201 - 500") => (8, null),
                      var s when s.Contains("500+") || s.Contains("500 +")        => (10, "Established customer base"),
                      _                                => (0, "Not specified")
                  }),

                // Total users reached
                C("Total users reached",
                  app.TotalUsersReached, 10,
                  v => v switch {
                      var s when s.Contains("1-50")    => (2, "Early-stage traction"),
                      var s when s.Contains("51-200")  => (5, null),
                      var s when s.Contains("201-500") || s.Contains("201 - 500") => (8, null),
                      var s when s.Contains("500+") || s.Contains("500 +")        => (10, "Established reach"),
                      _                                => (0, "Not specified")
                  }),
            };

            return Build("Product, Growth & Traction", criteria);
        }

        // ── COMMERCIAL PART 1 — REVENUE/FUNDING ─────────── Max: 68
        private ScoreSection ScoreCommercialPart1(Application app)
        {
            var criteria = new List<ScoreCriterion>
            {
                // Generating sales
                C("Generating sales",
                  app.HasStartedGeneratingSales, 10,
                  v => v.Contains("Yes", StringComparison.OrdinalIgnoreCase)
                       ? (10, "Market validation demonstrated")
                       : (0, "Pre-revenue, high risk")),

                // Year of first sale
                C("Year of first sale",
                  app.YearOfFirstSale, 10,
                  v => v switch {
                      var s when s.Contains("Before 2020") || s.Contains("2020") => (10, "5+ year sales history"),
                      var s when s.Contains("2021") || s.Contains("2022")        => (7, "3-4 year sales history"),
                      var s when s.Contains("2023") || s.Contains("2024")        => (5, "1-2 year sales history"),
                      var s when s.Contains("No sales") || s.Contains("No")      => (0, "No sales history"),
                      _                                                            => (0, "Not specified")
                  }),

                // Yearly sales revenue
                C("Yearly sales revenue",
                  app.YearlySalesRevenue, 10,
                  v => v switch {
                      var s when s.Contains("Above ₦500M") || s.Contains("500M") => (10, null),
                      var s when s.Contains("₦100M") || s.Contains("100M")       => (8, null),
                      var s when s.Contains("₦10M")  || s.Contains("10M")        => (5, null),
                      var s when s.Contains("Less than ₦10M")                    => (2, null),
                      var s when s.Contains("Zero") || s.Contains("0")           => (1, "No revenue"),
                      _                                                            => (0, "Not specified")
                  }),

                // Yearly profit
                C("Yearly profit",
                  app.YearlyProfit, 10,
                  v => v switch {
                      var s when s.Contains("Above ₦200M") || s.Contains("200M") => (10, null),
                      var s when s.Contains("₦50M")  || s.Contains("50M")        => (8, null),
                      var s when s.Contains("₦10M")  || s.Contains("10M")        => (5, null),
                      var s when s.Contains("Less than ₦10M")                    => (2, null),
                      var s when s.Contains("Zero") || s.Contains("0")           => (0, "No profit"),
                      _                                                            => (0, "Not specified")
                  }),

                // Proprietary funding
                C("Proprietary/founder's funding",
                  app.ProprietaryFunding, 10,
                  v => v switch {
                      var s when s.Contains("Above ₦50M") || s.Contains("50M")   => (10, null),
                      var s when s.Contains("₦10M") || s.Contains("10M")         => (8, null),
                      var s when s.Contains("₦1M")  || s.Contains("1M")          => (5, null),
                      var s when s.Contains("Less than ₦1M")                     => (2, null),
                      _                                                            => (0, "Not specified")
                  }),

                // External funding
                C("External funding",
                  app.ExternalFunding, 10,
                  v => v switch {
                      var s when s.Contains("Above ₦500M") || s.Contains("500M") => (10, null),
                      var s when s.Contains("₦100M") || s.Contains("100M")       => (8, null),
                      var s when s.Contains("₦10M")  || s.Contains("10M")        => (5, null),
                      var s when s.Contains("Less than ₦10M")                    => (2, null),
                      var s when s.Contains("Zero") || s.Contains("No external") => (0, "No external funding"),
                      _                                                            => (0, "Not specified")
                  }),
            };

            return Build("Commercial — Revenue & Funding", criteria);
        }

        // ── COMMERCIAL PART 2 — STRATEGY/MARKET ─────────── Max: 65
        private ScoreSection ScoreCommercialPart2(Application app)
        {
            var criteria = new List<ScoreCriterion>
            {
                // Demand evidence
                C("Demand evidence",
                  app.DemandEvidence, 5,
                  v => v switch {
                      var s when s.Contains("Paid pilots") || s.Contains("500+") => (5, null),
                      var s when s.Contains("LOI") || s.Contains("MOU")          => (4, null),
                      var s when s.Contains("Pre-order") || s.Contains("waitlist", StringComparison.OrdinalIgnoreCase) => (3, null),
                      var s when s.Contains("No formal") || s.Contains("None")   => (0, "No formal validation"),
                      _                                                            => (0, "Not specified")
                  }),

                // Geographic scalability
                C("Geographic scalability",
                  app.GeographicScalability, 5,
                  v => v switch {
                      var s when s.Contains("5+") || s.Contains("Already scaled") => (5, null),
                      var s when s.Contains("minimal") || s.Contains("Requires minimal") => (4, null),
                      var s when s.Contains("moderate") || s.Contains("Moderate") => (3, null),
                      var s when s.Contains("Not scalable") || s.Contains("No")   => (0, "Not scalable"),
                      _                                                             => (0, "Not specified")
                  }),

                // Gross margins
                C("Gross margins",
                  app.GrossMargins, 5,
                  v => v switch {
                      var s when s.Contains(">50") || s.Contains("50%+") || s.Contains("Above 50") => (5, "High profitability"),
                      var s when s.Contains("30") && s.Contains("50")  => (4, null),
                      var s when s.Contains("10") && s.Contains("30")  => (3, null),
                      var s when s.Contains("<10") || s.Contains("Under 10") => (0, "Low-margin business"),
                      _                                                  => (0, "Not specified")
                  }),

                // Primary competitive advantage
                C("Primary competitive advantage",
                  app.PrimaryCompetitiveAdvantage, 5,
                  v => {
                      if (string.IsNullOrWhiteSpace(v)) return (0, "Not specified");
                      int matches = 0;
                      if (v.Contains("Proprietary") || v.Contains("IP"))         matches++;
                      if (v.Contains("cost reduction") || v.Contains("25%"))     matches++;
                      if (v.Contains("Exclusive") || v.Contains("partnership"))  matches++;
                      if (v.Contains("No clear") || v.Contains("None"))          return (0, "No clear advantage");
                      return matches >= 3 ? (5, null) :
                             matches >= 2 ? (3, null) : (1, null);
                  }),

                // Operating runway
                C("Operating runway",
                  app.OperatingRunway, 5,
                  v => v switch {
                      var s when s.Contains("18+") || s.Contains("18 +") || s.Contains("Above 18") => (5, "Strong financial health"),
                      var s when s.Contains("12") && s.Contains("18") => (4, null),
                      var s when s.Contains("6")  && s.Contains("12") => (3, null),
                      var s when s.Contains("<6") || s.Contains("Under 6") || s.Contains("Less than 6") => (0, "Financial instability"),
                      _                                                 => (0, "Not specified")
                  }),

                // Active partnerships
                C("Active partnerships",
                  app.ActivePartnerships, 5,
                  v => v switch {
                      var s when s.Contains("10+") || s.Contains("10 +") => (5, null),
                      var s when s.Contains("5") && s.Contains("9")      => (4, null),
                      var s when s.Contains("1") && s.Contains("4")      => (2, null),
                      var s when s.Contains("None") || s.Contains("No")  => (0, "No partnerships"),
                      _                                                    => (0, "Not specified")
                  }),

                // Long-term growth strategy
                C("Long-term growth strategy",
                  app.LongTermGrowthStrategy, 5,
                  v => v switch {
                      var s when s.Contains("Acquisition") || s.Contains("IPO") => (5, "Clear exit strategy"),
                      var s when s.Contains("Sustainable profitability") || s.Contains("profitability") => (3, null),
                      var s when s.Contains("No defined") || s.Contains("None") => (0, "No defined plan"),
                      _                                                           => (0, "Not specified")
                  }),

                // IP ownership
                C("Intellectual property",
                  app.IpOwnership, 10,
                  v => v switch {
                      var s when s.Contains("Multiple patents") => (10, null),
                      var s when s.Contains("1 patent") || s.Contains("pending") => (7, null),
                      var s when s.Contains("Trade secrets") || s.Contains("Trade secret") => (4, null),
                      var s when s.Contains("No IP") || s.Contains("None") => (0, "No IP"),
                      _                                                      => (0, "Not specified")
                  }),

                // New customers (6 months)
                C("New customers (last 6 months)",
                  app.NewCustomersSixMonths, 5,
                  v => v switch {
                      var s when s.Contains("Over 1,000") || s.Contains("1000+") || s.Contains("Over 1000") => (5, null),
                      var s when s.Contains("500") && s.Contains("1,000") => (4, null),
                      var s when s.Contains("100") && s.Contains("500")   => (3, null),
                      var s when s.Contains("Less than 100") || s.Contains("Under 100") => (0, null),
                      _                                                     => (0, "Not specified")
                  }),

                // Customer growth rate
                C("Customer growth rate (past year)",
                  app.CustomerGrowthRate, 5,
                  v => v switch {
                      var s when s.Contains("Over 50") || s.Contains("50%+") => (5, null),
                      var s when s.Contains("25") && s.Contains("50")        => (4, null),
                      var s when s.Contains("10") && s.Contains("25")        => (3, null),
                      var s when s.Contains("Less than 10") || s.Contains("Under 10") => (0, null),
                      _                                                        => (0, "Not specified")
                  }),

                // Repeat customer revenue
                C("Revenue from repeat customers",
                  app.RepeatCustomerRevenue, 5,
                  v => v switch {
                      var s when s.Contains("Over 50") || s.Contains("50%+") => (5, null),
                      var s when s.Contains("30") && s.Contains("50")        => (4, null),
                      var s when s.Contains("10") && s.Contains("30")        => (3, null),
                      var s when s.Contains("Less than 10") || s.Contains("Under 10") => (0, null),
                      _                                                        => (0, "Not specified")
                  }),
            };

            return Build("Commercial — Strategy & Market", criteria);
        }

        // ── SUSTAINABILITY ───────────────────────────────── Max: 8
        private ScoreSection ScoreSustainability(Application app)
        {
            var criteria = new List<ScoreCriterion>
            {
                // SDG alignment
                C("SDG alignment",
                  app.SdgAlignment, 5,
                  v => {
                      if (string.IsNullOrWhiteSpace(v)) return (0, "No SDGs selected");
                      var count = v.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
                      return count > 2 ? (5, "More than two SDGs") :
                             count == 2 ? (4, "Two SDGs") :
                             count == 1 ? (2, "One SDG") :
                                          (0, "None");
                  }),

                // Business replicability
                C("Business replicability",
                  app.BusinessReplicability, 3,
                  v => v switch {
                      var s when s.Contains("5+") || s.Contains("Already scaled") => (3, null),
                      var s when s.Contains("Designed for") || s.Contains("easy replication") => (2, null),
                      var s when s.Contains("Needs adaptation") || s.Contains("adaptation")   => (1, null),
                      var s when s.Contains("Not replicable") || s.Contains("None")           => (0, "Not replicable"),
                      _                                                                         => (0, "Not specified")
                  }),
            };

            return Build("Sustainability", criteria);
        }

        // ── IMPACT ───────────────────────────────────────── Max: 87
        private ScoreSection ScoreImpact(Application app)
        {
            var criteria = new List<ScoreCriterion>
            {
                // Underserved market %
                C("Underserved market percentage",
                  app.UnderservedMarketPercentage, 3,
                  v => v switch {
                      var s when s.Contains(">80") || s.Contains("80%") || s.Contains("Above 80") => (3, null),
                      var s when s.Contains("60") && s.Contains("80") => (2, null),
                      var s when s.Contains("40") && s.Contains("60") => (1, null),
                      var s when s.Contains("<40") || s.Contains("Under 40") || s.Contains("Less than 40") => (0, null),
                      _                                                => (0, "Not specified")
                  }),

                // Systemic inequity approach
                C("Systemic inequality approach",
                  app.SystemicInequalityApproach, 3,
                  v => v switch {
                      var s when s.Contains("2+") || s.Contains("Targets 2") => (3, null),
                      var s when s.Contains("1 inequity") || s.Contains("Addresses 1") => (2, null),
                      var s when s.Contains("Indirect") => (1, null),
                      var s when s.Contains("No focus") || s.Contains("None") => (0, "No focus"),
                      _                                 => (0, "Not specified")
                  }),

                // Beneficiary involvement
                C("Beneficiary involvement",
                  app.BeneficiaryInvolvement, 3,
                  v => v switch {
                      var s when s.Contains("Co-created") || s.Contains("community input") => (3, null),
                      var s when s.Contains("Piloted") || s.Contains("user feedback")      => (2, null),
                      var s when s.Contains("Minimal")                                     => (1, null),
                      var s when s.Contains("No involvement") || s.Contains("None")        => (0, "No beneficiary involvement"),
                      _                                                                      => (0, "Not specified")
                  }),

                // Impact data sharing
                C("Impact data sharing",
                  app.ImpactDataSharing, 3,
                  v => v switch {
                      var s when s.Contains("Public") && s.Contains("audit") => (3, null),
                      var s when s.Contains("stakeholder") || s.Contains("Regular")        => (2, null),
                      var s when s.Contains("Internal")                                     => (1, null),
                      var s when s.Contains("No sharing") || s.Contains("None")            => (0, "No data sharing"),
                      _                                                                      => (0, "Not specified")
                  }),

                // Jobs created
                C("Jobs created",
                  app.JobsCreated, 3,
                  v => v switch {
                      var s when s.Contains("200+") || s.Contains("200 +") => (3, null),
                      var s when s.Contains("50")  && s.Contains("200")    => (2, null),
                      var s when s.Contains("10")  && s.Contains("50")     => (1, null),
                      var s when s.Contains("<10") || s.Contains("Under 10") || s.Contains("Less than 10") => (0, "Fewer than 10 jobs"),
                      _                                                      => (0, "Not specified")
                  }),

                // Gender gap approach
                C("Gender gap approach",
                  app.GenderGapApproach, 3,
                  v => v switch {
                      var s when s.Contains("50%+") || s.Contains("50% +") || s.Contains("female leadership") => (3, null),
                      var s when s.Contains("Targeted programs") || s.Contains("women") => (2, null),
                      var s when s.Contains("Neutral") || s.Contains("No focus")        => (1, null),
                      var s when s.Contains("Worsens")                                  => (0, "Worsens gaps"),
                      _                                                                   => (0, "Not specified")
                  }),

                // Access for underserved
                C("Access for underserved groups",
                  app.AccessForUnderserved, 3,
                  v => v switch {
                      var s when s.Contains("10k+") || s.Contains("10,000") || s.Contains("Free/low-cost") => (3, null),
                      var s when s.Contains("Affordable") && s.Contains("outcomes") => (2, null),
                      var s when s.Contains("Limited")  => (1, null),
                      var s when s.Contains("No focus") || s.Contains("None") => (0, "No focus"),
                      _                                  => (0, "Not specified")
                  }),

                // Resource optimization
                C("Resource optimization",
                  app.ResourceOptimization, 3,
                  v => v switch {
                      var s when s.Contains("70%+") || s.Contains("70% +") || s.Contains("Reduces waste by 70") => (3, null),
                      var s when s.Contains("30") && s.Contains("70")    => (2, null),
                      var s when s.Contains("Minimal")                   => (1, null),
                      var s when s.Contains("No focus") || s.Contains("None") => (0, "No focus"),
                      _                                                    => (0, "Not specified")
                  }),

                // Data protection
                C("Data protection",
                  app.DataProtection, 3,
                  v => v switch {
                      var s when s.Contains("NDPR") || s.Contains("ISO") => (3, null),
                      var s when s.Contains("Internal") || s.Contains("encryption") => (2, null),
                      var s when s.Contains("Basic")   => (1, null),
                      var s when s.Contains("No protection") || s.Contains("None") => (0, "No protection"),
                      _                                  => (0, "Not specified")
                  }),

                // Population impacted
                C("Population impacted",
                  app.PopulationImpacted, 10,
                  v => v switch {
                      var s when s.Contains(">80") || s.Contains("80%+") || s.Contains("Above 80") => (10, null),
                      var s when s.Contains("60") && s.Contains("80") => (7, null),
                      var s when s.Contains("40") && s.Contains("60") => (5, null),
                      _                                                => (0, "Not specified")
                  }),

                // Social good contribution
                C("Social good contribution",
                  app.SocialGoodContribution, 10,
                  v => v switch {
                      var s when s.Contains("Expanding access") || s.Contains("vital services")   => (10, null),
                      var s when s.Contains("Driving sustainable") || s.Contains("community-based") => (8, null),
                      var s when s.Contains("Empowering underserved") || s.Contains("underserved") => (6, null),
                      var s when s.Contains("Innovating") || s.Contains("quality of life")        => (4, null),
                      _                                                                             => (0, "Not specified")
                  }),

                // Ethical operations
                C("Ethical & socially responsible operations",
                  app.EthicalOperations, 10,
                  v => v switch {
                      var s when s.Contains("transparent supply") || s.Contains("supply chain") => (10, null),
                      var s when s.Contains("regulatory standards") || s.Contains("Adhering")   => (8, null),
                      var s when s.Contains("ethical sourcing") || s.Contains("labor")          => (7, null),
                      var s when s.Contains("local stakeholders") || s.Contains("accountability") => (6, null),
                      var s when s.Contains("sustainability") && s.Contains("audit")            => (5, null),
                      _                                                                           => (0, "Not specified")
                  }),

                // Diversity & inclusion
                C("Diversity & inclusion initiatives",
                  app.DiversityInclusion, 10,
                  v => v switch {
                      var s when s.Contains("Targeted recruitment") || s.Contains("underrepresented groups") => (10, null),
                      var s when s.Contains("Mandatory diversity") || s.Contains("diversity training")       => (8, null),
                      var s when s.Contains("resource groups") || s.Contains("mentorship")                  => (7, null),
                      var s when s.Contains("Policy updates") || s.Contains("accountability measures")      => (6, null),
                      _                                                                                       => (0, "Not specified")
                  }),

                // Equitable opportunities
                C("Equitable opportunities",
                  app.EquitableOpportunities, 10,
                  v => v switch {
                      var s when s.Contains("universal design") || s.Contains("accessible use") => (10, null),
                      var s when s.Contains("built-in training") || s.Contains("upskill")       => (8, null),
                      var s when s.Contains("cultural") || s.Contains("regional needs")         => (7, null),
                      var s when s.Contains("adaptive technology") || s.Contains("bias")        => (6, null),
                      var s when s.Contains("stakeholders") && s.Contains("refinement")         => (5, null),
                      _                                                                           => (0, "Not specified")
                  }),

                // Accessibility for disadvantaged
                C("Accessibility for disadvantaged groups",
                  app.AccessibilityForDisadvantaged, 10,
                  v => v switch {
                      var s when s.Contains("Affordable pricing") || s.Contains("flexible payment") => (10, null),
                      var s when s.Contains("User-friendly") || s.Contains("localized language")    => (8, null),
                      var s when s.Contains("community groups") || s.Contains("outreach")           => (7, null),
                      var s when s.Contains("diverse literacy") || s.Contains("digital access")     => (6, null),
                      var s when s.Contains("Inclusive") && s.Contains("testing")                   => (5, null),
                      _                                                                               => (0, "Not specified")
                  }),
            };

            return Build("Impact", criteria);
        }

        // ── HELPERS ────────────────────────────────────────────────

        private static ScoreCriterion C(
            string label,
            string? value,
            int maxScore,
            Func<string, (int pts, string? reason)> scorer)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new ScoreCriterion { Label = label, Value = value, Score = 0, MaxScore = maxScore, Reason = "Not provided" };

            var (pts, reason) = scorer(value.Trim());
            return new ScoreCriterion
            {
                Label = label,
                Value = value,
                Score = Math.Clamp(pts, 0, maxScore),
                MaxScore = maxScore,
                Reason = reason,
            };
        }

        private static ScoreSection Build(string name, List<ScoreCriterion> criteria)
        {
            var earned = criteria.Sum(c => c.Score);
            var max = criteria.Sum(c => c.MaxScore);
            return new ScoreSection
            {
                Name = name,
                Score = earned,
                MaxScore = max,
                Percentage = max > 0 ? Math.Round(earned / (double)max * 100, 1) : 0,
                Criteria = criteria,
            };
        }

        private static bool IsNigerian(string? country)
        {
            if (string.IsNullOrWhiteSpace(country)) return false;
            return country.Contains("Nigeria", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetMaxScore()
        {
            // Pre-check(3) + Company(10) + Team(3) + Product(30) +
            // Comm1(60) + Comm2(65) + Sustain(8) + Impact(87) = 266
            return 266;
        }

        private static string GetBand(int score, int max)
        {
            if (max == 0) return "N/A";
            var pct = score / (double)max * 100;
            return pct switch
            {
                >= 80 => "Excellent",
                >= 60 => "Strong",
                >= 40 => "Moderate",
                >= 20 => "Weak",
                _ => "Poor"
            };
        }
    }
}