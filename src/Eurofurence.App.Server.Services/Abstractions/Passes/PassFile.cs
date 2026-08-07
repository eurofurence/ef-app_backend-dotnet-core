namespace Eurofurence.App.Server.Services.Abstractions.Passes
{
    public class PassFile
    {
        public byte[] Data { get; init; }
        public string Name { get; init; }
        public string MimeType { get; init; }
    }
}