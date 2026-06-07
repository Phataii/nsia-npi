namespace nsia.Models
{
    public class ScoreResult
    {
        public int TotalScore { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public string Band { get; set; } = "";
        public List<ScoreSection> Sections { get; set; } = new();
    }

    public class ScoreSection
    {
        public string Name { get; set; } = "";
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public List<ScoreCriterion> Criteria { get; set; } = new();
    }

    public class ScoreCriterion
    {
        public string Field { get; set; } = "";
        public string Label { get; set; } = "";
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public string? Value { get; set; }
        public string? Reason { get; set; }
    }
}