namespace Eurofurence.App.Domain.Model.Users
{
    public enum UserRegistrationStatus
    {
        Unknown = 0,
        New = 1,
        Approved = 2,
        PartiallyPaid = 3,
        Paid = 4,
        CheckedIn = 5,
        Cancelled = 6,
        Deleted = 7,
    }
}