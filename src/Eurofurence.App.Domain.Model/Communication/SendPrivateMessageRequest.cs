namespace Eurofurence.App.Domain.Model.Communication
{
    public class SendPrivateMessageRequest
    {
        public string RecipientRegSysId { get; set; }
        public string RecipientIdentityId { get; set; }
        public string AuthorName { get; set; }
        public string ToastTitle { get; set; }
        public string ToastMessage { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}