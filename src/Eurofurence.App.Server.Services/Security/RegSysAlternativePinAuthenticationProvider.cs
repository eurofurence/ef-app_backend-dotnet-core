using System;
using System.Threading.Tasks;
using Eurofurence.App.Common.Validation;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Security
{
    public class RegSysAlternativePinAuthenticationProvider : IAuthenticationProvider, IRegSysAlternativePinAuthenticationProvider
    {
        private readonly AppDbContext _appDbContext;

        public RegSysAlternativePinAuthenticationProvider(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        private string GeneratePin()
        {
            var r = new Random();
            return $"{r.Next(100000, 999999)}";
        }

        public async Task<RegSysAlternativePinResponse> RequestAlternativePinAsync(RegSysAlternativePinRequest request, string requesterUid)
        {
            int regNo = 0;

            if (!BadgeChecksum.TryParse(request.RegNoOnBadge, out regNo)) return null;

            var alternativePin = await _appDbContext.RegSysAlternativePins.FirstOrDefaultAsync(a => a.RegNo == regNo);

            bool existingRecord = true;

            if (alternativePin == null)
            {
                existingRecord = false;
                alternativePin = new RegSysAlternativePinRecord()
                {
                    RegNo = regNo,
                };
                alternativePin.NewId();
            }

            alternativePin.IssuedByUid = requesterUid;
            alternativePin.IssuedDateTimeUtc = DateTime.UtcNow;
            alternativePin.Pin = GeneratePin();
            alternativePin.NameOnBadge = request.NameOnBadge;
            var issueRecord = _appDbContext.IssueRecords.Add(new IssueRecord()
            {
                RequestDateTimeUtc = DateTime.UtcNow,
                NameOnBadge = request.NameOnBadge,
                RequesterUid = requesterUid
            });
            alternativePin.IssueLog.Add(issueRecord.Entity);

            alternativePin.Touch();

            if (existingRecord)
                _appDbContext.RegSysAlternativePins.Update(alternativePin);
            else
                _appDbContext.RegSysAlternativePins.Add(alternativePin);

            await _appDbContext.SaveChangesAsync();

            return new RegSysAlternativePinResponse()
            {
                NameOnBadge = alternativePin.NameOnBadge,
                Pin = alternativePin.Pin,
                RegNo = alternativePin.RegNo
            };
        }

        public async Task<RegSysAlternativePinRecord> GetAlternativePinAsync(int regNo)
        {
            return await _appDbContext.RegSysAlternativePins.FirstOrDefaultAsync(a => a.RegNo == regNo);
        }

        public async Task<AuthenticationResult> ValidateRegSysAuthenticationRequestAsync(RegSysAuthenticationRequest request)
        {
            var result = new AuthenticationResult
            {
                Source = GetType().Name,
                IsAuthenticated = false
            };

            var alternativePin =
                await _appDbContext.RegSysAlternativePins.FirstOrDefaultAsync(a => a.RegNo == request.RegNo);

            if (alternativePin != null && request.Password == alternativePin.Pin)
            {
                result.IsAuthenticated = true;
                result.RegNo = alternativePin.RegNo;
                result.Username = alternativePin.NameOnBadge;

                alternativePin.PinConsumptionDatesUtc.Add(DateTime.UtcNow);
                _appDbContext.RegSysAlternativePins.Update(alternativePin);
                await _appDbContext.SaveChangesAsync();
            }

            return result;
        }
    }
}
