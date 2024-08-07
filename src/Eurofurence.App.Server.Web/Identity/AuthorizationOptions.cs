using System.Collections.Generic;

namespace Eurofurence.App.Server.Web.Identity;

public class AuthorizationOptions
{
    public HashSet<string> Attendee { get; set; } = new();

    public HashSet<string> Admin { get; set; } = new();

    public HashSet<string> KnowledgeBaseEditor { get; set; } = new();

    public HashSet<string> MapEditor { get; set; } = new();

    public HashSet<string> ArtShow { get; set; } = new();
    
    public HashSet<string> FursuitBadgeSystem { get; set; } = new();
    
    public HashSet<string> PrivateMessageSender { get; set; } = new();
}