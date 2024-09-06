using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using dotAPNS;
using dotAPNS.AspNetCore;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class PushNotificationChannelManager : IPushNotificationChannelManager
    {
        private readonly IDeviceIdentityService _deviceService;
        private readonly IRegistrationIdentityService _registrationService;
        private readonly AppDbContext _appDbContext;
        private readonly ConventionSettings _conventionSettings;
        private readonly FirebaseConfiguration _firebaseConfiguration;
        private readonly FirebaseMessaging _firebaseMessaging;
        private readonly ApnsConfiguration _apnsConfiguration;
        private readonly IApnsService _apnsService;
        private readonly ApnsJwtOptions _apnsJwtOptions;
        private readonly ILogger _logger;

        public PushNotificationChannelManager(
            IDeviceIdentityService deviceService,
            IRegistrationIdentityService registrationService,
            AppDbContext appDbContext,
            FirebaseConfiguration configuration,
            ConventionSettings conventionSettings,
            ApnsConfiguration apnsConfiguration,
            IApnsService apnsService,
            ILoggerFactory loggerFactory)
        {
            _deviceService = deviceService;
            _registrationService = registrationService;
            _appDbContext = appDbContext;
            _firebaseConfiguration = configuration;
            _conventionSettings = conventionSettings;
            _apnsConfiguration = apnsConfiguration;
            _apnsService = apnsService;
            _logger = loggerFactory.CreateLogger(GetType());

            if (_firebaseConfiguration.IsConfigured)
            {
                _logger.LogInformation("Configuring Firebase Cloud Messaging (FCM)…");
                var googleCredential = GoogleCredential.FromFile(_firebaseConfiguration.GoogleServiceCredentialKeyFile);
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions { Credential = googleCredential });
                }

                _firebaseMessaging = FirebaseMessaging.GetMessaging(FirebaseApp.DefaultInstance);
            }

            if (_apnsConfiguration.IsConfigured)
            {
                _logger.LogInformation("Configuring Apple Push Notification service (APNs)…");
                _apnsJwtOptions = new ApnsJwtOptions()
                {
                    BundleId = apnsConfiguration.BundleId,
                    //CertFilePath = apnsConfiguration.CertFilePath,
                    CertContent = apnsConfiguration.CertContent,
                    KeyId = apnsConfiguration.KeyId,
                    TeamId = apnsConfiguration.TeamId,
                };
            }
        }

        public async Task PushAnnouncementNotificationAsync(
            AnnouncementRecord announcement,
            CancellationToken cancellationToken = default)
        {
            if (_firebaseConfiguration.IsConfigured)
            {
                var androidMessage = new Message()
                {
                    Topic = $"{_conventionSettings.ConventionIdentifier}-android",
                    Android = new AndroidConfig()
                    {
                        Notification = new AndroidNotification()
                        {
                            Title = announcement.Title.RemoveMarkdown(),
                            Body = announcement.Content.RemoveMarkdown(),
                            Icon = "notification_icon",
                            Color = "#006459"
                        }
                    },
                    Data = new Dictionary<string, string>()
                {
                    // For Legacy Native Android App
                    { "Event", "Announcement" },
                    { "Title", announcement.Title.RemoveMarkdown() },
                    { "Text", announcement.Content.RemoveMarkdown() },
                    { "RelatedId", announcement.Id.ToString() },
                    { "CID", _conventionSettings.ConventionIdentifier },

                    // For Expo / React Native
                    { "experienceId", _firebaseConfiguration.ExpoExperienceId },
                    { "scopeKey", _firebaseConfiguration.ExpoScopeKey },
                }
                };

                await _firebaseMessaging.SendAsync(androidMessage, cancellationToken);
            }

            if (_apnsConfiguration.IsConfigured)
            {
                var apnsDeviceIdentities = await _deviceService.FindAll(d => d.DeviceType == DeviceType.Ios).ToArrayAsync();

                _logger.LogInformation($"Pushing announcement {announcement.Id} to {apnsDeviceIdentities.Count()} devices on APNs…");

                var apnsTasks = apnsDeviceIdentities.Select(apnsDeviceIdentity => pushAlertApnsAsync(
                    apnsDeviceIdentity,
                    announcement.Title.RemoveMarkdown(),
                    announcement.Content.RemoveMarkdown(),
                    announcement.Id,
                    "announcement",
                    "Announcement"
                ));
                var apnsResults = await Task.WhenAll(apnsTasks);
                await pruneInvalidApnsDevicesAsync(apnsResults);
            }
        }

        public async Task PushPrivateMessageNotificationToIdentityIdAsync(
            string identityId,
            string toastTitle,
            string toastMessage,
            Guid relatedId,
            CancellationToken cancellationToken = default)
        {
            if (!_firebaseConfiguration.IsConfigured && !_apnsConfiguration.IsConfigured) return;

            var devices = await _deviceService.FindByIdentityId(identityId, cancellationToken);

            await PushPrivateMessageNotificationAsync(devices,
                toastTitle,
                toastMessage,
                relatedId,
                cancellationToken
            );
        }

        public async Task PushPrivateMessageNotificationToRegSysIdAsync(
            string regSysId,
            string toastTitle,
            string toastMessage,
            Guid relatedId,
            CancellationToken cancellationToken = default)
        {
            if (!_firebaseConfiguration.IsConfigured && !_apnsConfiguration.IsConfigured) return;

            var devices = await _deviceService.FindByRegSysId(regSysId, cancellationToken);

            await PushPrivateMessageNotificationAsync(devices,
                toastTitle,
                toastMessage,
                relatedId,
                cancellationToken
            );
        }

        public async Task PushSyncRequestAsync(CancellationToken cancellationToken = default)
        {
            if (_firebaseConfiguration.IsConfigured)
            {
                var androidMessage = new Message()
                {
                    Topic = $"{_conventionSettings.ConventionIdentifier}-android",
                    Data = new Dictionary<string, string>()
                {
                    // For Legacy Native Android App
                    { "Event", "Sync" },
                    { "CID", _conventionSettings.ConventionIdentifier },

                    // For Expo / React Native
                    { "experienceId", _firebaseConfiguration.ExpoExperienceId },
                    { "scopeKey", _firebaseConfiguration.ExpoScopeKey },
                    {
                        "body", JsonSerializer.Serialize(new
                            {
                                @event = "Sync",
                                cid = _conventionSettings.ConventionIdentifier
                            }
                        )
                    }
                }
                };

                await _firebaseMessaging.SendAsync(androidMessage, cancellationToken);
            }

            if (_apnsConfiguration.IsConfigured)
            {
                var apnsDeviceIdentities = await _deviceService.FindAll(d => d.DeviceType == DeviceType.Ios).ToArrayAsync();

                _logger.LogInformation($"Pushing sync request to {apnsDeviceIdentities.Count()} devices on APNs…");

                var apnsTasks = apnsDeviceIdentities.Select(pushSyncApnsAsync);
                var apnsResults = await Task.WhenAll(apnsTasks);
                await pruneInvalidApnsDevicesAsync(apnsResults);
            }
        }


        public async Task RegisterDeviceAsync(
            string deviceToken,
            string identityId,
            string[] regSysIds,
            DeviceType type,
            CancellationToken cancellationToken = default)
        {
            var existing = await _appDbContext.DeviceIdentities
                .Where(x => x.DeviceToken == deviceToken)
                .ToListAsync(cancellationToken);

            if (existing.Count > 0)
            {
                foreach (var identity in existing)
                {
                    identity.IdentityId = identityId;
                    identity.Touch();
                }

                await _appDbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                switch (type)
                {
                    case DeviceType.Android:
                        await _firebaseMessaging.SubscribeToTopicAsync([deviceToken], $"{_conventionSettings.ConventionIdentifier}-android");
                        break;
                    case DeviceType.Ios:
                        // Using APNS directly instead of via FCM
                        break;
                }
                await _deviceService.InsertOneAsync(new DeviceIdentityRecord
                {
                    IdentityId = identityId,
                    DeviceToken = deviceToken,
                    DeviceType = type
                }, cancellationToken);
            }

            if (regSysIds.Length == 0)
            {
                return;
            }

            var set = new HashSet<string>(regSysIds);

            foreach (var id in await _registrationService
                         .FindAll(x => set.Contains(x.RegSysId))
                         .Select(x => x.RegSysId)
                         .ToListAsync(cancellationToken))
            {
                set.Remove(id);
            }

            await _registrationService.InsertMultipleAsync(set.Select(x => new RegistrationIdentityRecord
            {
                RegSysId = x,
                IdentityId = identityId
            }).ToList(), cancellationToken);
        }

        private struct ApnsResult
        {
            public DeviceIdentityRecord DeviceIdentity { get; set; }
            public ApnsResponse ApnsResponse { get; set; }
        }

        private async Task pruneInvalidApnsDevicesAsync(IEnumerable<ApnsResult> apnsResults)
        {
            var invalidDeviceIdentityIds = new List<Guid>();
            foreach (var apnsResult in apnsResults)
            {
                if (apnsResult.ApnsResponse.IsSuccessful) continue;

                var responseReason = apnsResult.ApnsResponse.Reason;
                if (responseReason == ApnsResponseReason.BadDeviceToken
                    || responseReason == ApnsResponseReason.DeviceTokenNotForTopic
                    || responseReason == ApnsResponseReason.ExpiredToken
                    || responseReason == ApnsResponseReason.Unregistered)
                {
                    invalidDeviceIdentityIds.Add(apnsResult.DeviceIdentity.Id);
                }
            }

            if (invalidDeviceIdentityIds.Count > 0) await _deviceService.DeleteMultipleAsync(invalidDeviceIdentityIds);


            _logger.LogDebug($"Pruned {invalidDeviceIdentityIds.Count()} devices from APNs…");
        }

        private async Task PushPrivateMessageNotificationAsync(
            List<DeviceIdentityRecord> devices,
            string toastTitle,
            string toastMessage,
            Guid relatedId,
            CancellationToken cancellationToken = default)
        {
            var firebaseMessages = new List<Message>();
            var apnsTasks = new List<Task<ApnsResult>>();

            foreach (var device in devices)
            {
                switch (device.DeviceType)
                {
                    case DeviceType.Android:
                        firebaseMessages.Add(new Message()
                        {
                            Token = device.DeviceToken,
                            Android = new AndroidConfig()
                            {
                                Notification = new AndroidNotification()
                                {
                                    Title = toastTitle,
                                    Body = toastMessage,
                                    Icon = "notification_icon",
                                    Color = "#006459"
                                }
                            },
                            Data = new Dictionary<string, string>()
                            {
                                // For Legacy Native Android App
                                { "Event", "Notification" },
                                { "Title", toastTitle },
                                { "Message", toastMessage },
                                { "RelatedId", relatedId.ToString() },
                                { "CID", _conventionSettings.ConventionIdentifier },

                                // For Expo / React Native
                                { "experienceId", _firebaseConfiguration.ExpoExperienceId },
                                { "scopeKey", _firebaseConfiguration.ExpoScopeKey }
                            }
                        });
                        break;

                    case DeviceType.Ios:
                        apnsTasks.Add(pushAlertApnsAsync(
                            device,
                            toastTitle,
                            toastMessage,
                            relatedId,
                            "message",
                            "notification"
                        ));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (_firebaseConfiguration.IsConfigured && firebaseMessages.Count() > 0)
            {
                _logger.LogDebug($"Pushing private message {relatedId} to {firebaseMessages.Count()} devices on FCM…");
                await _firebaseMessaging.SendEachAsync(firebaseMessages, cancellationToken);
            }

            if (_apnsConfiguration.IsConfigured && apnsTasks.Count() > 0)
            {
                _logger.LogDebug($"Pushing private message {relatedId} to {apnsTasks.Count()} devices on APNs…");
                var apnsResults = await Task.WhenAll(apnsTasks);
                await pruneInvalidApnsDevicesAsync(apnsResults);
            }
        }

        private async Task<ApnsResult> pushAlertApnsAsync(DeviceIdentityRecord deviceIdentity, string title, string message, Guid messageId, string messageIdType, string eventType)
        {
            return new ApnsResult()
            {
                DeviceIdentity = deviceIdentity,
                ApnsResponse = await _apnsService.SendPush(new ApplePush(ApplePushType.Alert)
                        .AddAlert(title, message)
                        .AddCustomProperty($"{messageIdType}_id", messageId.ToString())
                        .AddCustomProperty("event", eventType)
                        .SetPriority(5)
                        .AddToken(deviceIdentity.DeviceToken), _apnsJwtOptions)
            };
        }

        private async Task<ApnsResult> pushSyncApnsAsync(DeviceIdentityRecord deviceIdentity)
        {
            return new ApnsResult()
            {
                DeviceIdentity = deviceIdentity,
                ApnsResponse = await _apnsService.SendPush(new ApplePush(ApplePushType.Background)
                        .AddCustomProperty("event", "Sync")
                        .AddContentAvailable()
                        .SetPriority(5)
                        .AddToken(deviceIdentity.DeviceToken), _apnsJwtOptions)
            };
        }
    }
}