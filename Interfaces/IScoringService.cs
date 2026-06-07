using nsia.Models;

namespace nsia.Services
{
    public interface IScoringService
    {
        ScoreResult Calculate(Application app);
    }
}