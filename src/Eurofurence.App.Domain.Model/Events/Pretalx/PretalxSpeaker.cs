namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    /// <summary>
    /// Used for deserialization of the slot model from the pretalx API.
    /// 
    /// see https://docs.pretalx.org/api/resources/#tag/speakers/operation/speakers_retrieve
    /// </summary>
    public class PretalxSpeaker
    {
        public string Code { get; init; }
        public string Name { get; init; }
        public string Biography { get; init; }
        public string AvatarUrl { get; init; }
    }
}