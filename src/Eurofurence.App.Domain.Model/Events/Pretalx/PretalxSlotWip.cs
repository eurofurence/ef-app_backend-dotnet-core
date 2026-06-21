using System.Text.Json.Serialization;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    /// <summary>
    /// Used for loading wip version of schedule without expansion of submissions as it is only used for
    /// retrieving blockers which do not have a submission.
    /// When not expanded, the submission field will be either <c>null</c> or the submissions <c>code</c>.
    /// </summary>
    /// <seealso cref="PretalxSlot" />
    public class PretalxSlotWip : PretalxSlot
    {
        [JsonPropertyName("submission")]
        public string SubmissionCode { get; init; }
        [JsonIgnore]
        public new PretalxSubmission Submission
        {
            get => SubmissionCode != null ? new PretalxSubmission() : null;
        }
    }
}