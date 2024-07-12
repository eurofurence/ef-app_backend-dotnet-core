using System.Collections.Generic;

namespace Eurofurence.App.Server.Web.Identity;

public class AuthorizationOptions
{
    public HashSet<string> System { get; set; } = new();

    public HashSet<string> Developer { get; set; } = new();

    public HashSet<string> KnowledgeBaseMaintainer { get; set; } = new();
    
    public HashSet<string> ArtShow { get; set; } = new();
}