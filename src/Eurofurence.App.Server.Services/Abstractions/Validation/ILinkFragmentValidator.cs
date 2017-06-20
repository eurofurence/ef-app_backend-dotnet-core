using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Server.Services.Validation;

namespace Eurofurence.App.Server.Services.Abstractions.Validation
{
    public interface ILinkFragmentValidator
    {
        Task<LinkFragmentValidator.ValidationResult> ValidateAsync(LinkFragment fragment);
    }
}