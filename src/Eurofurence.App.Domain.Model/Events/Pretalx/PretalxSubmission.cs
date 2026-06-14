namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxSubmission
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public PretalxSpeaker[] Speakers { get; set; }
        public PretalxSubmissionType SubmissionType { get; set; }
        public PretalxTrack Track { get; set; }
        public PretalxTag[] Tags { get; set; }
        public PretalxSubmissionState State { get; set; }
        public string Abstract { get; set; }
        public string Description { get; set; }
    }
}