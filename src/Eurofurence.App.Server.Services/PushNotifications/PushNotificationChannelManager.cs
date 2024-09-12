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
        private readonly ExpoConfiguration _expoConfiguration;
        private readonly IApnsService _apnsService;
        private readonly ILogger _logger;

        public PushNotificationChannelManager(
            IDeviceIdentityService deviceService,
            IRegistrationIdentityService registrationService,
            AppDbContext appDbContext,
            FirebaseConfiguration configuration,
            ConventionSettings conventionSettings,
            ExpoConfiguration expoConfiguration,
            ApnsConfiguration apnsConfiguration,
            IApnsService apnsService,
            ILoggerFactory loggerFactory)
        {
            _deviceService = deviceService;
            _registrationService = registrationService;
            _appDbContext = appDbContext;
            _firebaseConfiguration = configuration;
            _conventionSettings = conventionSettings;
            _expoConfiguration = expoConfiguration;
            _apnsConfiguration = apnsConfiguration;
            _apnsService = apnsService;
            _logger = loggerFactory.CreateLogger(GetType());

            if (_firebaseConfiguration.IsConfigured)
            {
                _firebaseMessaging = FirebaseMessaging.GetMessaging(FirebaseApp.DefaultInstance);
            }
        }

        public async Task PushAnnouncementNotificationAsync(
            AnnouncementRecord announcement,
            CancellationToken cancellationToken = default)
        {
            if (_firebaseConfiguration.IsConfigured)
            {
                var androidMessage = createAndroidFcmMessage(null, PushEventType.Announcement, announcement.Title.RemoveMarkdown(), announcement.Content.RemoveMarkdown(), announcement.Id);

                await _firebaseMessaging.SendAsync(androidMessage, cancellationToken);
            }

            if (_apnsConfiguration.IsConfigured)
            {
                var apnsDeviceIdentities = await _deviceService.FindAll(d => d.DeviceType == DeviceType.Ios).ToArrayAsync();

                _logger.LogInformation($"Pushing announcement {announcement.Id} to {apnsDeviceIdentities.Count()} devices on APNs…");

                var apnsTasks = apnsDeviceIdentities.Select(apnsDeviceIdentity => pushApnsAsync(
                    apnsDeviceIdentity,
                    PushEventType.Announcement,
                    announcement.Title.RemoveMarkdown(),
                    announcement.Content.RemoveMarkdown(),
                    announcement.Id
                ));
                var apnsResults = await Task.WhenAll(apnsTasks);
                await pruneInvalidApnsDevicesAsync(apnsResults);
            }
        }

        public async Task PushPrivateMessageNotificationToIdentityIdAsync(
            string identityId,
            string title,
            string message,
            Guid relatedId,
            CancellationToken cancellationToken = default)
        {
            if (!_firebaseConfiguration.IsConfigured && !_apnsConfiguration.IsConfigured) return;

            var devices = await _deviceService.FindByIdentityId(identityId, cancellationToken);

            await pushPrivateMessageNotificationAsync(devices,
                title,
                message,
                relatedId,
                cancellationToken
            );
        }

        public async Task PushPrivateMessageNotificationToRegSysIdAsync(
            string regSysId,
            string title,
            string message,
            Guid relatedId,
            CancellationToken cancellationToken = default)
        {
            if (!_firebaseConfiguration.IsConfigured && !_apnsConfiguration.IsConfigured) return;

            var devices = await _deviceService.FindByRegSysId(regSysId, cancellationToken);

            await pushPrivateMessageNotificationAsync(devices,
                title,
                message,
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
                    { "experienceId", _expoConfiguration.ExperienceId },
                    { "scopeKey", _expoConfiguration.ScopeKey },
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

                var apnsTasks = apnsDeviceIdentities.Select(pushSyncApnsAsync);

                _logger.LogInformation($"Pushing sync request to {apnsTasks.Count()} devices on APNs…");

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

        /// <summary>
        /// Types of potential push events as expected by the apps.
        /// </summary>
        private enum PushEventType {
            /// <summary>
            /// Broadcast announcement to all registered devices.
            /// </summary>
            Announcement,
            /// <summary>
            /// Targeted notification sent to single user (potentially multiple devices).
            /// </summary>
            Notification,
            /// <summary>
            /// Ask app to refresh its data from <c>/Sync</c> endpoint
            /// </summary>
            Sync
        }

        /// <summary>
        /// Used to pair each APNs response with the device it was received for.
        /// </summary>
        private struct ApnsResult
        {
            public DeviceIdentityRecord DeviceIdentity { get; set; }
            public ApnsResponse ApnsResponse { get; set; }
        }

        /// <summary>
        /// Prunes invalid APNs device tokens from the database if the APNs request was not
        /// successful and failed for any of the following reasons:
        /// <list type="bullet">
        /// <item>
        /// <description><c>ApnsResponseReason.BadDeviceToken</c></description>
        /// </item>
        /// <item>
        /// <description><c>ApnsResponseReason.BadDeviceToken</c></description>
        /// </item>
        /// <item>
        /// <description><c>ApnsResponseReason.ExpiredToken</c></description>
        /// </item>
        /// <item>
        /// <description><c>ApnsResponseReason.Unregistered</c></description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="apnsResults">Results returned from APNs together with the associated device to be pruned on specific errors.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Push a private message to a number of Android or Apple devices.
        /// </summary>
        /// <param name="devices">List of target devices</param>
        /// <param name="title">Title of the private message</param>
        /// <param name="message">Body of the private message</param>
        /// <param name="relatedId">ID of the related private message object</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Unsupported device type</exception>
        private async Task pushPrivateMessageNotificationAsync(
            List<DeviceIdentityRecord> devices,
            string title,
            string message,
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
                        firebaseMessages.Add(createAndroidFcmMessage(
                            device,
                            PushEventType.Notification,
                            title,
                            message,
                            relatedId
                        ));
                        break;

                    case DeviceType.Ios:
                        apnsTasks.Add(pushApnsAsync(
                            device,
                            PushEventType.Notification,
                            title,
                            message,
                            relatedId
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

        /// <summary>
        /// Send a sync notification to Apple Push Notification service (APNs) causing the target
        /// device to refresh its data against the sync endpoint of the backend.
        /// </summary>
        /// <param name="deviceIdentity">Target device</param>
        /// <returns>Response from APNs together with the target device to allow pruning invalid tokens.</returns>
        private async Task<ApnsResult> pushSyncApnsAsync(DeviceIdentityRecord deviceIdentity)
        {
            return await pushApnsAsync(deviceIdentity, PushEventType.Sync);
        }

        /// <summary>
        /// Push a notification to Apple Push Notification service (APNs).
        /// </summary>
        /// <param name="deviceIdentity">Target device</param>
        /// <param name="eventType">Type of the notification event</param>
        /// <param name="title">Title of the message or null for type <c>Sync</c></param>
        /// <param name="message">Body of the message or null for type <c>Sync</c></param>
        /// <param name="relatedId">Id of entity that caused this notification or null for type <c>Sync</c></param>
        /// <returns>Response from APNs together with the target device to allow pruning invalid tokens.</returns>
        private async Task<ApnsResult> pushApnsAsync(DeviceIdentityRecord deviceIdentity, PushEventType eventType, string title = null, string message = null, Guid? relatedId = null)
        {
            var pushType = eventType == PushEventType.Sync ? ApplePushType.Background : ApplePushType.Alert;

            var applePush = new ApplePush(pushType)
                        .AddCustomProperty("Event", eventType.ToString())
                        .AddCustomProperty("CID", _conventionSettings.ConventionIdentifier)
                        .AddCustomProperty("experienceId", _expoConfiguration.ExperienceId)
                        .AddCustomProperty("scopeKey", _expoConfiguration.ScopeKey)
                        .SetPriority(5)
                        .AddToken(deviceIdentity.DeviceToken);

            if (pushType == ApplePushType.Alert)
                applePush.AddAlert(title, message)
                        .AddCustomProperty("Title", title)
                        .AddCustomProperty("Message", message);

            if (eventType == PushEventType.Sync)
                applePush.AddContentAvailable();

            if (relatedId != null)
                applePush.AddCustomProperty("RelatedId", relatedId.ToString());

            if (_apnsConfiguration.UseDevelopmentServer)
                applePush.SendToDevelopmentServer();

            return new ApnsResult()
            {
                DeviceIdentity = deviceIdentity,
                ApnsResponse = await _apnsService.SendPush(applePush, _apnsConfiguration.ApnsJwtOptions)
            };
        }

        /// <summary>
        /// Create a push message for Android devices via Firebase Cloud Messaging (FCM).
        /// </summary>
        /// <param name="deviceIdentity">Target device or <c>null</c> to send to CID-based topic</param>
        /// <param name="eventType">Type of the notification event (either <c>Notification</c> or `Announcement)</param>
        /// <param name="title">Title of the message</param>
        /// <param name="message">Body of the message</param>
        /// <param name="relatedId">Id of entity that caused this notification</param>
        /// <returns>Message object that can be submitted to FCM.</returns>
        private Message createAndroidFcmMessage(DeviceIdentityRecord deviceIdentity, PushEventType eventType, string title = null, string message = null, Guid? relatedId = null)
        {
            var fcmMessage = new Message()
            {
                Android = new AndroidConfig()
                {
                    Notification = new AndroidNotification()
                    {
                        Title = title,
                        Body = message,
                        Icon = "notification_icon",
                        Color = "#006459"
                    }
                },
                Data = new Dictionary<string, string>()
                {
                    // For Legacy Native Android App
                    { "Event", eventType.ToString() },
                    { "Title", title },
                    { "Text", message },
                    { "RelatedId", relatedId.ToString() },
                    { "CID", _conventionSettings.ConventionIdentifier },

                    // For Expo / React Native
                    { "experienceId", _expoConfiguration.ExperienceId },
                    { "scopeKey", _expoConfiguration.ScopeKey },
                }
            };

            if (deviceIdentity != null)
                fcmMessage.Token = deviceIdentity.DeviceToken;
            else
                fcmMessage.Topic = $"{_conventionSettings.ConventionIdentifier}-android";

            return fcmMessage;
        }
    }
}