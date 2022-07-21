using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Lassie
{
    public interface ILassieApiClient
    {
        public Task<LostAndFoundResponse[]> QueryLostAndFoundDbAsync(string command = "lostandfound");
    }
}
