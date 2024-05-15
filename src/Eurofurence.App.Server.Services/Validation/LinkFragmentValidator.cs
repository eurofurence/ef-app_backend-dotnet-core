using System;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Services.Abstractions.Validation;

namespace Eurofurence.App.Server.Services.Validation
{
    public class LinkFragmentValidator : ILinkFragmentValidator
    {
        private readonly IDealerService _dealerService;
        private readonly IEventConferenceRoomService _eventConferenceRoomService;
        private readonly IMapService _mapService;

        public LinkFragmentValidator(
            IDealerService dealerService, 
            IEventConferenceRoomService eventConferenceRoomService,
            IMapService mapService)
        {
            _dealerService = dealerService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _mapService = mapService;
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

                case LinkFragment.FragmentTypeEnum.EventConferenceRoom:

                    if (!Guid.TryParse(fragment.Target, out Guid eventConferenceRoomId))
                        return ValidationResult.Error("Target must be of typ Guid");

                    var eventConferenceRoom = await _eventConferenceRoomService.FindOneAsync(eventConferenceRoomId);
                    if (eventConferenceRoom == null)
                        return ValidationResult.Error($"No eventConferenceRoom found with id {eventConferenceRoomId}");

                    break;

                case LinkFragment.FragmentTypeEnum.MapEntry:

                    if (!Guid.TryParse(fragment.Target, out Guid mapEntryId))
                        return ValidationResult.Error("Target must be of typ Guid");

                    var mapEntry = _mapService.FindAll()
                        .SelectMany(a => a.Entries)
                        .SingleOrDefault(a => a.Id == mapEntryId);

                    if (mapEntry == null)
                        return ValidationResult.Error($"No mapEntry found with id {mapEntryId}");

                    break;

                case LinkFragment.FragmentTypeEnum.WebExternal:

                    if (!Uri.IsWellFormedUriString(fragment.Target, UriKind.Absolute))
                        return ValidationResult.Error("Target must be a well formated absolute Uri");

                    break;
            }

            return ValidationResult.Valid();
        }

        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }

            public static ValidationResult Valid()
            {
                return new ValidationResult {IsValid = true};
            }

            public static ValidationResult Error(string message)
            {
                return new ValidationResult {IsValid = false, ErrorMessage = message};
            }
        }
    }
}