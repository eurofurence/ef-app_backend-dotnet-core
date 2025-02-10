namespace Eurofurence.App.Server.Services.Abstractions
{
    public class JobsOptions
    {
        public JobOption FlushPrivateMessageNotifications { get; set; }
        public JobOption UpdateAnnouncements { get; set; }
        public JobOption UpdateDealers { get; set; }
        public JobOption UpdateEvents { get; set; }
        public JobOption UpdateLostAndFound { get; set; }
    }
}