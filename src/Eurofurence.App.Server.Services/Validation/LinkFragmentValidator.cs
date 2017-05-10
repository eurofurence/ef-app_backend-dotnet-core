using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Server.Services.Abstractions;
using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Validation
{
    public class LinkFragmentValidator : ILinkFragmentValidator
    {
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }

            public static ValidationResult Valid()
            {
                return new ValidationResult() { IsValid = true };
            }

            public static ValidationResult Error(string message)
            {
                return new ValidationResult() { IsValid = false, ErrorMessage = message };
            }
        }

        readonly IDealerService _dealerService;

        public LinkFragmentValidator(IDealerService dealerService)
        {
            _dealerService = dealerService;
        }

        public async Task<ValidationResult> ValidateAsync(LinkFragment fragment)
        {
            switch (fragment.FragmentType)
            {
                case LinkFragment.FragmentTypeEnum.DealerDetail:

                    if (!Guid.TryParse(fragment.Target, out Guid dealerId))
                        return ValidationResult.Error("Target must be of typ Guid");

                    var dealer = await _dealerService.FindOneAsync(dealerId);
                    if (dealer == null)
                        return ValidationResult.Error($"No dealer found with id {dealerId}");

                    break;


                case LinkFragment.FragmentTypeEnum.WebExternal:

                    if (!Uri.IsWellFormedUriString(fragment.Target, UriKind.Absolute))
                        return ValidationResult.Error("Target must be a well formated absolute Uri");

                    break;
            }

            return ValidationResult.Valid();
        }
        
    }
}
