using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class FirebaseChannelManager : IPushNotificationChannelManager
    {
        private readonly IDeviceIdentityService _deviceService;
        private readonly IRegistrationIdentityService _registrationService;
        private readonly AppDbContext _appDbContext;
        private readonly FirebaseConfiguration _configuration;
        private readonly ExpoConfiguration _expoConfiguration;
        private readonly ConventionSettings _conventionSettings;
        private readonly FirebaseMessaging _firebaseMessaging;

        public FirebaseChannelManager(
            IDeviceIdentityService deviceService,
            IRegistrationIdentityService registrationService,
            AppDbContext appDbContext,
            FirebaseConfiguration configuration,
            ExpoConfiguration expoConfiguration,
            ConventionSettings conventionSettings)
        {
            _deviceService = deviceService;
            _registrationService = registrationService;
            _appDbContext = appDbContext;
            _configuration = configuration;
            _expoConfiguration = expoConfiguration;
            _conventionSettings = conventionSettings;

            if (_configuration.GoogleServiceCredentialKeyFile is not { Length: > 0 } file) return;

            var googleCredential = GoogleCredential.FromFile(file);
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions { Credential = googleCredential });
            }

            _firebaseMessaging = FirebaseMessaging.GetMessaging(FirebaseApp.DefaultInstance);
        }

        public Task PushAnnouncementNotificationAsync(
            AnnouncementRecord announcement,
            CancellationToken cancellationToken = default)
        {
            if (!_configuration.IsConfigured) return Task.CompletedTask;

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
                    { "experienceId", _expoConfiguration.ExperienceId },
                    { "scopeKey", _expoConfiguration.ScopeKey },
                }
            };

            var apnsMessage = new Message()
            {
                Topic = $"{_conventionSettings.ConventionIdentifier}-ios",
                Data = new Dictionary<string, string>()
                {
                    { "event", "Announcement" },
                    { "announcement_id", announcement.Id.ToString() }
                },
                Notification = new Notification()
                {
                    Title = announcement.Title.RemoveMarkdown(),
                    Body = announcement.Content.RemoveMarkdown()
                },
                Apns = new ApnsConfig()
                {
                    Headers = new Dictionary<string, string>()
                    {
                        { "apns-priority", "5" },
                        { "apns-push-type", "alert" }
                    },
                    Aps = new Aps()
                    {
                        Sound = "generic_notification.caf",
                    }
                }
            };

            return _firebaseMessaging.SendEachAsync(new[] { androidMessage, apnsMessage }, cancellationToken);
        }

        public async Task PushPrivateMessageNotificationToIdentityIdAsync(
            string identityId,
            string title,
            string message,
            Guid relatedId,
            CancellationToken cancellationToken = default)
        {
            if (!_configuration.IsConfigured) return;

            var devices = await _deviceService.FindByIdentityId(identityId, cancellationToken);

            await PushPrivateMessageNotificationAsync(devices,
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
            if (!_configuration.IsConfigured) return;

            var devices = await _deviceService.FindByRegSysId(regSysId, cancellationToken);

            await PushPrivateMessageNotificationAsync(devices,
                title,
                message,
                relatedId,
                cancellationToken
            );
        }

        public Task PushSyncRequestAsync(CancellationToken cancellationToken = default)
        {
            if (!_configuration.IsConfigured) return Task.CompletedTask;

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

            var apnsMessage = new Message()
            {
                Topic = $"{_conventionSettings.ConventionIdentifier}-ios",
                Data = new Dictionary<string, string>()
                {
                    { "event", "Sync" },
                },
                Apns = new ApnsConfig()
                {
                    Headers = new Dictionary<string, string>()
                    {
                        { "apns-priority", "5" },
                        { "apns-push-type", "background" }
                    },
                    Aps = new Aps()
                    {
                        ContentAvailable = true
                    }
                }
            };

            return _firebaseMessaging.SendEachAsync(new[] { androidMessage, apnsMessage }, cancellationToken);
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
                        await _firebaseMessaging.SubscribeToTopicAsync([deviceToken], $"{_conventionSettings.ConventionIdentifier}-ios");
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

        private async Task PushPrivateMessageNotificationAsync(
            List<DeviceIdentityRecord> devices,
            string title,
            string message,
            Guid relatedId,
            CancellationToken cancellationToken = default)
        {
            var messages = new List<Message>();

            foreach (var device in devices)
            {
                switch (device.DeviceType)
                {
                    case DeviceType.Android:
                        messages.Add(new Message()
                        {
                            Token = device.DeviceToken,
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
                                { "Event", "Notification" },
                                { "Title", title },
                                { "Message", message },
                                { "RelatedId", relatedId.ToString() },
                                { "CID", _conventionSettings.ConventionIdentifier },

                                // For Expo / React Native
                                { "experienceId", _expoConfiguration.ExperienceId },
                                { "scopeKey", _expoConfiguration.ScopeKey }
                            }
                        });
                        break;

                    case DeviceType.Ios:
                        messages.Add(new Message()
                        {
                            Token = device.DeviceToken,
                            Data = new Dictionary<string, string>()
                            {
                                { "event", "notification" },
                                { "message_id", relatedId.ToString() }
                            },
                            Notification = new Notification()
                            {
                                Title = title,
                                Body = message,
                            },
                            Apns = new ApnsConfig()
                            {
                                Headers = new Dictionary<string, string>()
                                {
                                    { "apns-priority", "5" },
                                    { "apns-push-type", "alert" }
                                },
                                Aps = new Aps()
                                {
                                    Sound = "personal_notification.caf",
                                }
                            }
                        });
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            await _firebaseMessaging.SendEachAsync(messages, cancellationToken);
        }
    }
}