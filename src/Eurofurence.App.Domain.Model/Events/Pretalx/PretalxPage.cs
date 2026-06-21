namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    /// <summary>
    /// Used for deserialization of pagination from the pretalx API.
    /// 
    /// see https://docs.pretalx.org/api/fundamentals/#pagination
    /// </summary>
    public class PretalxPage<PageType>
    {
        public int Count { get; init; }
        public string Next { get; init; }
        public string Previous { get; init; }
        public PageType[] Results { get; init; }
    }
}