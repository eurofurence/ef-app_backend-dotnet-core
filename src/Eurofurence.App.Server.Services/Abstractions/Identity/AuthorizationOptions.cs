using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.Identity;

public class AuthorizationOptions
{
    public HashSet<string> Admin { get; init; } = new();

    public HashSet<string> Attendee { get; init; } = new();

    public HashSet<string> AttendeeCheckedIn { get; init; } = new();

    public HashSet<string> Staff { get; init; } = new();

    public HashSet<string> KnowledgeBaseEditor { get; init; } = new();

    public HashSet<string> MapEditor { get; init; } = new();

    public HashSet<string> ArtShow { get; init; } = new();

    /// <summary>
    /// Artist alley moderators may approve or reject table applications from attendees.
    /// </summary>
    public HashSet<string> ArtistAlleyModerator { get; init; } = new();

    public HashSet<string> ArtistAlleyAdmin { get; init; } = new();

    public HashSet<string> PrivateMessageSender { get; init; } = new();
}