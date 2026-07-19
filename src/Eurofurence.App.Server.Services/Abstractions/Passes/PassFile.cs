namespace Eurofurence.App.Server.Services.Abstractions.Passes
{
    public class PassFile
    {
        public byte[] data { get; init; }
        public string name { get; init; }
        public string mimeType { get; init; }
    }
}