namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxSubmission
    {
        public string Code { get; init; }
        public string Title { get; init; }
        public PretalxSpeaker[] Speakers { get; init; }
        public PretalxSubmissionType SubmissionType { get; init; }
        public PretalxTrack Track { get; init; }
        /*
        Expand `slots.submission.tags` is currently resulting in an Internal Server Error (HTTP 500) status
        code on the `/schedules/` endpoint of Pretalx.
        */
        //public PretalxTag[] Tags { get; init; }
        public int[] Tags { get; init; }
        public string State { get; init; }
        public string Abstract { get; init; }
        public string Description { get; init; }
    }
}