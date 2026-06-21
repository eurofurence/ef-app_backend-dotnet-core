namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxPage<PageType>
    {
        public int Count { get; init; }
        public string Next { get; init; }
        public string Previous { get; init; }
        public PageType[] Results { get; init; }
    }
}