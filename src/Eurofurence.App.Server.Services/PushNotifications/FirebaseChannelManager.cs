using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Common.Results;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class FirebaseChannelManager : IFirebaseChannelManager
    {
        private const string ASN_InterruptionLevel_Passive = "passive";
        private const string ASN_InterruptionLevel_Active = "active";
        private const string ASN_InterruptionLevel_TimeSensitive = "time-sensitive";
        private const string ASN_InterruptionLevel_Critical = "critical";

        private readonly AppDbContext _appDbContext;
        private readonly FirebaseConfiguration _configuration;
        private readonly ConventionSettings _conventionSettings;
        private readonly FirebaseApp _firebaseApp;
        private readonly FirebaseMessaging _firebaseMessaging;

        public FirebaseChannelManager(
            AppDbContext appDbContext,
            FirebaseConfiguration configuration,
            ConventionSettings conventionSettings
            )

        {
            _appDbContext = appDbContext;
            _configuration = configuration;
            _conventionSettings = conventionSettings;

            var googleCredential = GoogleCredential.FromFile(_configuration.GoogleServiceCredentialKeyFile);

            _firebaseApp = FirebaseApp.Create(new AppOptions { Credential = googleCredential });
            _firebaseMessaging = FirebaseMessaging.GetMessaging(_firebaseApp);
        }

        private IQueryable<PushNotificationChannelRecord> GetRecipientChannel(string recipientUid)
        {
            return _appDbContext.PushNotificationChannels.Where(
                a => a.Platform == PushNotificationChannelRecord.PlatformEnum.Firebase && a.Uid == recipientUid);
        }

        public Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement)
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

            return _firebaseMessaging.SendAllAsync(new Message[] { androidMessage, apnsMessage });
        }

        public async Task PushPrivateMessageNotificationAsync(string recipientUid, string toastTitle, string toastMessage, Guid relatedId)
        {
            if (!_configuration.IsConfigured) return;

            var recipients = GetRecipientChannel(recipientUid);

            var messages = new List<Message>();

            foreach (var recipient in recipients)
            {
                if (recipient.Topics.Contains("ios", StringComparer.CurrentCultureIgnoreCase))
                {
                    messages.Add(new Message()
                    {
                        Token = recipient.DeviceId.ToString(),
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
                else
                {
                    messages.Add(new Message()
                    {
                        Token = recipient.DeviceId.ToString(),
                        Android = new AndroidConfig() { 
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
            }

            await _firebaseMessaging.SendAllAsync(messages);
        }

        public Task PushSyncRequestAsync()
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
                    { "body", JsonConvert.SerializeObject(new
                        {
                            @event = "Sync",
                            cid = _conventionSettings.ConventionIdentifier
                        }
                    )}
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

            return _firebaseMessaging.SendAllAsync(new Message[] { androidMessage, apnsMessage });
        }


        public async Task RegisterDeviceAsync(string deviceId, string uid, string[] topics)
        {
            var record = (await _appDbContext.PushNotificationChannels.FirstOrDefaultAsync(a => a.DeviceId == deviceId));

            var isNewRecord = record == null;

            if (isNewRecord)
            {
                record = new PushNotificationChannelRecord()
                {
                    Platform = PushNotificationChannelRecord.PlatformEnum.Firebase,
                    DeviceId = deviceId
                };
                record.NewId();
            }

            record.Touch();
            record.Uid = uid;
            record.Topics = topics.ToList();

            if (isNewRecord)
                _appDbContext.PushNotificationChannels.Add(record);
            else
                _appDbContext.PushNotificationChannels.Update(record);

            await _appDbContext.SaveChangesAsync();
        }

        public async Task<IResult> SubscribeToTopicAsync(string deviceId, string topic)
        {
            if (!_configuration.FirebaseTopics.Contains(topic)) return Result.Error("INVALID_TOPIC", "Topic not accepted");

            var response = await _firebaseMessaging.SubscribeToTopicAsync(new string[] { deviceId }, topic);

            return response.FailureCount > 0
                ? Result.Error("FCM_ERROR", response.Errors.FirstOrDefault()?.Reason)
                : Result.Ok;
        }

        public async Task<IResult> UnsubscribeFromTopicAsync(string deviceId, string topic)
        {
            if (!_configuration.FirebaseTopics.Contains(topic)) return Result.Error("INVALID_TOPIC", "Topic not accepted");

            var response = await _firebaseMessaging.UnsubscribeFromTopicAsync(new string[] { deviceId }, topic);

            return response.FailureCount > 0
                ? Result.Error("FCM_ERROR", response.Errors.FirstOrDefault()?.Reason)
                : Result.Ok;
        }

    }
}