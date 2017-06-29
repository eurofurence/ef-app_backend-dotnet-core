using System;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Common.Validation;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Server.Services.Abstractions.Security;

namespace Eurofurence.App.Server.Services.Security
{
    public class RegSysAlternativePinAuthenticationProvider : IAuthenticationProvider, IRegSysAlternativePinAuthenticationProvider
    {
        private readonly IEntityRepository<RegSysAlternativePinRecord> _regSysAlternativePinRepository;

        public RegSysAlternativePinAuthenticationProvider(IEntityRepository<RegSysAlternativePinRecord> regSysAlternativePinRepository)
        {
            _regSysAlternativePinRepository = regSysAlternativePinRepository;
        }

        public async Task<RegSysAlternativePinResponse> RequestAlternativePinAsync(RegSysAlternativePinRequest request, string requesterUid)
        {
            int regNo = 0;

            if (!BadgeChecksum.TryParse(request.RegNoOnBadge, out regNo)) return null;

            var alternativePin =
                (await _regSysAlternativePinRepository.FindAllAsync(a => a.RegNo == regNo))
                .SingleOrDefault();

            bool existingRecord = true;

            if (alternativePin == null)
            {
                existingRecord = false;
                alternativePin = new RegSysAlternativePinRecord()
                {
                    RegNo = regNo,
                    Pin = new Random().Next(1000, 9999).ToString(),
                    IssuedByUid = requesterUid,
                    IssuedDateTimeUtc = DateTime.UtcNow
                };
                alternativePin.NewId();
            }

            alternativePin.NameOnBadge = request.NameOnBadge;
            alternativePin.RequestLog.Add(new RegSysAlternativePinRecord.RequestRecord()
            {
                RequestDateTimeUtc = DateTime.UtcNow,
                NameOnBadge = request.NameOnBadge,
                RequesterUid = requesterUid
            });

            alternativePin.Touch();

            if (existingRecord)
                await _regSysAlternativePinRepository.ReplaceOneAsync(alternativePin);
            else
                await _regSysAlternativePinRepository.InsertOneAsync(alternativePin);

            return new RegSysAlternativePinResponse()
            {
                NameOnBadge = alternativePin.NameOnBadge,
                Pin = alternativePin.Pin,
                RegNo = alternativePin.RegNo
            };
        }


        public async Task<AuthenticationResult> ValidateRegSysAuthenticationRequestAsync(RegSysAuthenticationRequest request)
        {
            var result = new AuthenticationResult
            {
                Source = GetType().Name,
                IsAuthenticated = false
            };

            var alternativePin =
                (await _regSysAlternativePinRepository.FindAllAsync(a => a.RegNo == request.RegNo))
                .SingleOrDefault();

            if (alternativePin != null && request.Password == alternativePin.Pin)
            {
                result.IsAuthenticated = true;
                result.RegNo = alternativePin.RegNo;
                result.Username = alternativePin.NameOnBadge;

                alternativePin.PinConsumptionDatesUtc.Add(DateTime.UtcNow);
                await _regSysAlternativePinRepository.ReplaceOneAsync(alternativePin);
            }

            return result;
        }
    }
}
