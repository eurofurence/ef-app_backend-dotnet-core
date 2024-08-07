using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class FirebaseChannelManager : IFirebaseChannelManager
    {
        private readonly IDeviceService _deviceService;
        private readonly FirebaseConfiguration _configuration;
        private readonly ConventionSettings _conventionSettings;
        private readonly FirebaseApp _firebaseApp;
        private readonly FirebaseMessaging _firebaseMessaging;

        public FirebaseChannelManager(
            IDeviceService deviceService,
            FirebaseConfiguration configuration,
            ConventionSettings conventionSettings)
        {
            _deviceService = deviceService;
            _configuration = configuration;
            _conventionSettings = conventionSettings;

            var googleCredential = GoogleCredential.FromFile(_configuration.GoogleServiceCredentialKeyFile);

            _firebaseApp = FirebaseApp.Create(new AppOptions { Credential = googleCredential });
            _firebaseMessaging = FirebaseMessaging.GetMessaging(_firebaseApp);
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
                    { "experienceId", _configuration.ExpoExperienceId },
                    { "scopeKey", _configuration.ExpoScopeKey },
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
            string toastTitle,
            string toastMessage,
            Guid relatedId,
            CancellationToken cancellationToken = default)
        {
            if (!_configuration.IsConfigured) return;

            var devices = await _deviceService
                .FindAll(x => x.RegSysId == null && x.IdentityId == identityId)
                .ToListAsync(cancellationToken);

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
            if (!_configuration.IsConfigured) return;

            var devices = await _deviceService
                .FindAll(x => x.RegSysId == regSysId)
                .ToListAsync(cancellationToken);

            await PushPrivateMessageNotificationAsync(devices,
                toastTitle,
                toastMessage,
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
                    { "experienceId", _configuration.ExpoExperienceId },
                    { "scopeKey", _configuration.ExpoScopeKey },
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
            bool isAndroid,
            CancellationToken cancellationToken = default)
        {
            if (await _deviceService.FindAll(a => a.DeviceToken == deviceToken).AnyAsync(cancellationToken))
            {
                return;
            }

            await _deviceService.InsertOneAsync(new DeviceRecord
            {
                IdentityId = identityId,
                RegSysId = null,
                DeviceToken = deviceToken,
                IsAndroid = isAndroid
            }, cancellationToken);

            foreach (var id in regSysIds)
            {
                await _deviceService.InsertOneAsync(new DeviceRecord
                {
                    IdentityId = identityId,
                    RegSysId = id,
                    DeviceToken = deviceToken,
                    IsAndroid = isAndroid
                }, cancellationToken);
            }
        }

        private async Task PushPrivateMessageNotificationAsync(
            List<DeviceRecord> devices,
            string toastTitle,
            string toastMessage,
            Guid relatedId,
            CancellationToken cancellationToken = default)
        {
            var messages = new List<Message>();

            foreach (var device in devices)
            {
                if (device.IsAndroid)
                {
                    messages.Add(new Message()
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
                            { "experienceId", _configuration.ExpoExperienceId },
                            { "scopeKey", _configuration.ExpoScopeKey }
                        }
                    });
                }
                else
                {
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
                            Title = toastTitle,
                            Body = toastMessage,
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
                }
            }

            await _firebaseMessaging.SendEachAsync(messages, cancellationToken);
        }
    }
}