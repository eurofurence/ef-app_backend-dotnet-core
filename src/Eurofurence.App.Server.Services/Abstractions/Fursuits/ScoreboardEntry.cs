namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public abstract class ScoreboardEntry
    {
        public int CollectionCount { get; set; }
        public string Name { get; set; }
        public int Rank { get; set; }
    }
}