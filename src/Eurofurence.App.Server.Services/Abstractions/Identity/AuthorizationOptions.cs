using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.Identity;

public class AuthorizationOptions
{
    public HashSet<string> Admin { get; init; } = [];

    public HashSet<string> AnnouncementManager { get; init; } = [];

    public HashSet<string> Attendee { get; init; } = [];

    public HashSet<string> AttendeeCheckedIn { get; init; } = [];

    public HashSet<string> EventArtworkManager { get; init; } = [];

    public HashSet<string> EventFeedbackManager { get; init; } = [];

    public HashSet<string> Staff { get; init; } = [];

    public HashSet<string> KnowledgeBaseEditor { get; init; } = [];

    public HashSet<string> MapEditor { get; init; } = [];

    public HashSet<string> ArtShow { get; init; } = [];

    /// <summary>
    /// Artist alley moderators may approve or reject table applications from attendees.
    /// </summary>
    public HashSet<string> ArtistAlleyModerator { get; init; } = [];

    public HashSet<string> ArtistAlleyAdmin { get; init; } = [];

    public HashSet<string> PrivateMessageSender { get; init; } = [];
}