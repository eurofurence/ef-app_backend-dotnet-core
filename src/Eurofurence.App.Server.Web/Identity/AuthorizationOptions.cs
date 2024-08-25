using System.Collections.Generic;

namespace Eurofurence.App.Server.Web.Identity;

public class AuthorizationOptions
{
    public HashSet<string> Admin { get; set; } = new();

    public HashSet<string> Attendee { get; set; } = new();
    public HashSet<string> AttendeeCheckedIn { get; set; } = new();

    public HashSet<string> KnowledgeBaseEditor { get; set; } = new();

    public HashSet<string> MapEditor { get; set; } = new();

    public HashSet<string> ArtShow { get; set; } = new();

    public HashSet<string> FursuitBadgeSystem { get; set; } = new();

    /// <summary>
    /// Artist alley moderators may approve or reject table applications from attendees.
    /// </summary>
    public HashSet<string> ArtistAlleyModerator { get; set; } = new();

    public HashSet<string> ArtistAlleyAdmin { get; set; } = new();

    public HashSet<string> PrivateMessageSender { get; set; } = new();
}