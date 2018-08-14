using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Common.Validation;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.InlineQueryResults;
using System.IO;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class AdminConversation : Conversation, IConversation
    {
        private readonly IUserManager _userManager;
        private readonly IRegSysAlternativePinAuthenticationProvider _regSysAlternativePinAuthenticationProvider;
        private readonly IEntityRepository<PushNotificationChannelRecord> _pushNotificationChannelRepository;
        private readonly IEntityRepository<FursuitBadgeRecord> _fursuitBadgeRepository;
        private readonly IEntityRepository<FursuitBadgeImageRecord> _fursuitBadgeImageRepository;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly ICollectingGameService _collectingGameService;
        private readonly ConventionSettings _conventionSettings;


        private class CommandInfo
        {
            public string Command { get; set; }
            public string Description { get; set; }
            public PermissionFlags RequiredPermission { get; set; }
            public Func<Task> CommandHandler { get; set; }
        }

        private List<CommandInfo> _commands;

        [Flags]
        enum PermissionFlags
        {
            None = 0,
            UserAdmin = 1 << 0,
            PinCreate = 1 << 1,
            PinQuery = 1 << 2,
            Statistics = 1 << 3,
            Locate = 1 << 4,
            SendPm = 1 << 5,
            BadgeChecksum = 1 << 6,
            CollectionGameAdmin = 1 << 7,
            All = (1 << 8) - 1,
        }

        private User _user;

        private Task<PermissionFlags> GetPermissionsAsync()
        {
            return _userManager.GetAclForUserAsync<PermissionFlags>(_user.Username);
        }

        private bool HasPermission(PermissionFlags userPermissionFlags, PermissionFlags requiredPermission)
        {
            return userPermissionFlags.HasFlag(requiredPermission);
        }


        public ITelegramBotClient BotClient { get; set; }

        private Func<string, Task> _awaitingResponseCallback = null;
        private Message _lastAskMessage;
        private ILogger _logger;

        public AdminConversation(
            IUserManager userManager,
            IRegSysAlternativePinAuthenticationProvider regSysAlternativePinAuthenticationProvider,
            IEntityRepository<PushNotificationChannelRecord> pushNotificationChannelRepository,
            IEntityRepository<FursuitBadgeRecord> fursuitBadgeRepository,
            IEntityRepository<FursuitBadgeImageRecord> fursuitBadgeImageRepository,
            IPrivateMessageService privateMessageService,
            ICollectingGameService collectingGameService,
            ConventionSettings conventionSettings,
            ILoggerFactory loggerFactory
            )
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _userManager = userManager;
            _regSysAlternativePinAuthenticationProvider = regSysAlternativePinAuthenticationProvider;
            _pushNotificationChannelRepository = pushNotificationChannelRepository;
            _fursuitBadgeRepository = fursuitBadgeRepository;
            _fursuitBadgeImageRepository = fursuitBadgeImageRepository;
            _privateMessageService = privateMessageService;
            _collectingGameService = collectingGameService;
            _conventionSettings = conventionSettings;

            _commands = new List<CommandInfo>()
            {
                new CommandInfo()
                {
                    Command = "/pin",
                    Description = "Create an alternative password for an attendee to login",
                    RequiredPermission = PermissionFlags.PinCreate,
                    CommandHandler = CommandPinRequest
                },
                new CommandInfo()
                {
                    Command = "/pinInfo",
                    Description = "Show the pin & issue log for a given registration number",
                    RequiredPermission = PermissionFlags.PinQuery,
                    CommandHandler = CommandPinInfo
                },
                new CommandInfo()
                {
                    Command = "/users",
                    Description = "Manage Users",
                    RequiredPermission = PermissionFlags.UserAdmin,
                    CommandHandler = CommandUserAdmin
                }
                ,
                new CommandInfo()
                {
                    Command = "/statistics",
                    Description = "Show some statistics",
                    RequiredPermission = PermissionFlags.Statistics,
                    CommandHandler = CommandStatistics
                },
                new CommandInfo()
                {
                    Command = "/locate",
                    Description = "Figure out if a given regNo is signed in on any device",
                    RequiredPermission = PermissionFlags.Locate,
                    CommandHandler = CommandLocate
                },
                new CommandInfo()
                {
                    Command = "/pm",
                    Description = "Send a personal message to a specific RegNo",
                    RequiredPermission = PermissionFlags.SendPm,
                    CommandHandler = CommandSendMessage
                },
                new CommandInfo()
                {
                    Command = "/badgeChecksum",
                    Description = "Calculate the checksum letter of a given reg no.",
                    RequiredPermission = PermissionFlags.BadgeChecksum,
                    CommandHandler = CommandBadgeChecksum
                },
                new CommandInfo()
                {
                    Command ="/fursuitBadgeById",
                    Description = "Lookup fursuit badge (by id)",
                    RequiredPermission = PermissionFlags.CollectionGameAdmin,
                    CommandHandler = CommandFursuitBadge
                },
                new CommandInfo()
                {
                    Command ="/collectionGameRegisterFursuit",
                    Description = "Register a fursuit for the game",
                    RequiredPermission = PermissionFlags.CollectionGameAdmin,
                    CommandHandler = CommandCollectionGameRegisterFursuit
                },
                new CommandInfo()
                {
                    Command ="/collectionGameUnban",
                    Description = "Unban someone from the game",
                    RequiredPermission = PermissionFlags.CollectionGameAdmin,
                    CommandHandler = CommandCollectionGameUnban
                }
            };
        }

        public async Task CommandFursuitBadge()
        {
            Func<Task> c1 = null;
            var title = "Fursuit Badge";

            c1 = () => AskAsync($"*{title} - Step 1 of 1*\nType in the fursuit badge number.",
                async input =>
                {
                    await ClearLastAskResponseOptions();

                    int badgeNo = 0;
                    if (!int.TryParse(input, out badgeNo))
                    {
                        await ReplyAsync("That's not a valid number.");
                        await c1();
                        return;
                    }

                    var badge =
                        await _fursuitBadgeRepository.FindOneAsync(a => a.ExternalReference == badgeNo.ToString());

                    if (badge == null)
                    {
                        await ReplyAsync($"*{title} - Result*\nNo badge found with it *{badgeNo}*.");
                        return;
                    }

                    await ReplyAsync(
                        $"*{title} - Result*\nNo: *{badgeNo}*\nOwner: *{badge.OwnerUid}*\nName: *{badge.Name.RemoveMarkdown()}*\nSpecies: *{badge.Species.RemoveMarkdown()}*\nGender: *{badge.Gender.RemoveMarkdown()}*\nWorn By: *{badge.WornBy.RemoveMarkdown()}*\n\nLast Change (UTC): {badge.LastChangeDateTimeUtc}");

                    var imageContent = new MemoryStream((await _fursuitBadgeImageRepository.FindOneAsync(badge.Id)).ImageBytes);
                    await BotClient.SendPhotoAsync(ChatId, new FileToSend(badge.Id.ToString(), imageContent));
                }, "Cancel=/cancel");
            await c1();
        }

        public async Task CommandBadgeChecksum()
        {
            Func<Task> c1 = null;
            var title = "Badge Checksum";

            c1 = () => AskAsync($"*{title} - Step 1 of 1*\nWhat's the _registration number_?",
                async regNoAsString =>
                {
                    await ClearLastAskResponseOptions();

                    int regNo = 0;
                    if (!int.TryParse(regNoAsString, out regNo))
                    {
                        await ReplyAsync($"_{regNoAsString} is not a number._");
                        await c1();
                        return;
                    }

                    var checksumLetter = BadgeChecksum.CalculateChecksum(regNo);

                    await ReplyAsync($"*{title} - Result*\nBadge will show *{regNoAsString}{checksumLetter}*.");
                }, "Cancel=/cancel");
            await c1();
        }

        public async Task CommandSendMessage()
        {
            Func<Task> c1 = null, c2 = null, c3 = null, c4 = null;
            var title = "Send Message";

            c1 = () => AskAsync($"*{title} - Step 1 of 1*\nWhat's the attendees _registration number (including the letter at the end)_ on the badge?",
                async c1a =>
                {
                    await ClearLastAskResponseOptions();

                    int regNo = 0;
                    var regNoWithLetter = c1a.Trim().ToUpper();
                    if (!BadgeChecksum.TryParse(regNoWithLetter, out regNo))
                    {
                        await ReplyAsync($"_{regNoWithLetter} is not a valid badge number - checksum letter is missing or wrong._");
                        await c1();
                        return;
                    }

                    var records =
                        (await _pushNotificationChannelRepository.FindAllAsync(a => a.Uid.StartsWith("RegSys:") && a.Uid.EndsWith($":{regNo}")))
                        .ToList();

                    if (records.Count == 0)
                    {
                        await ReplyAsync($"*WARNING: RegNo {regNo} is not logged in on any known device - they will receive the message when they login.*");
                    }

                    c2 = () => AskAsync($"*{title} - Step 2 of 3*\nPlease specify the subject of the message.", async subject =>
                    {
                        await ClearLastAskResponseOptions();
                        c3 = () => AskAsync($"*{title} - Step 3 of 3*\nPlease specify the body of the message.", async body =>
                        {
                            await ClearLastAskResponseOptions();
                            var from = $"{_user.FirstName} {_user.LastName}";

                            c4 = () => AskAsync(

                                $"*{title} - Review*\n\nFrom: *{from.EscapeMarkdown()}*\nTo: *{regNo}*\nSubject: *{subject.EscapeMarkdown()}*\n\nMessage:\n*{body.EscapeMarkdown()}*\n\n_Message will be placed in the users inbox and directly pushed to _*{records.Count}*_ devices._",
                                async c4a =>
                                {
                                    if (c4a != "*send")
                                    {
                                        await c3();
                                        return;
                                    }

                                    var recipientUid = $"RegSys:{_conventionSettings.ConventionNumber}:{regNo}";
                                    var messageId = await _privateMessageService.SendPrivateMessageAsync(new SendPrivateMessageRequest()
                                    {
                                        AuthorName = $"{from}",
                                        RecipientUid = recipientUid,
                                        Message = body,
                                        Subject = subject,
                                        ToastTitle = "You received a new personal message",
                                        ToastMessage = "Open the Eurofurence App to read it."
                                    });

                                    _logger.LogInformation(
                                        LogEvents.Audit,
                                        "{requesterUid} sent a PM {messsageId} to {recipientUid} via Telegram Bot",
                                        $"Telegram:@{_user.Username}", messageId, recipientUid);

                                    await ReplyAsync("Message sent.");
                                }, "Send=*send", "Cancel=/cancel");
                            await c4();

                        }, "Cancel=/cancel");
                        await c3();

                    },"Cancel=/cancel");

                    await c2();


                }, "Cancel=/cancel");
            await c1();
        }

        public async Task CommandCollectionGameUnban()
        {
            Func<Task> c1 = null;
            var title = "Collecting Game Unban";

            c1 = () => AskAsync($"*{title} - Step 1 of 1*\nWhat's the attendees _registration number (including the letter at the end)_ on the badge?",
                async regNoAsString =>
                {
                    await ClearLastAskResponseOptions();

                    int regNo = 0;
                    var regNoWithLetter = regNoAsString.Trim().ToUpper();
                    if (!BadgeChecksum.TryParse(regNoWithLetter, out regNo))
                    {
                        await ReplyAsync($"_{regNoWithLetter} is not a valid badge number - checksum letter is missing or wrong._");
                        await c1();
                        return;
                    }

                    var result = await _collectingGameService.UnbanPlayerAsync(
                            $"RegSys:{_conventionSettings.ConventionNumber}:{regNo}");

                    if (result.IsSuccessful)
                    {
                        await ReplyAsync($"*{title}* - Success - {regNo} unbanned.");
                    }
                    else
                    {
                        await ReplyAsync($"*{title}* - Error\n\n{result.ErrorMessage} ({result.ErrorCode})");
                    }


                }, "Cancel=/cancel");
            await c1();
        }

        public async Task CommandLocate()
        {
            Func<Task> c1 = null;
            var title = "Locate User";

            c1 = () => AskAsync($"*{title} - Step 1 of 1*\nWhat's the attendees _registration number (including the letter at the end)_ on the badge?",
                async c1a =>
                {
                    await ClearLastAskResponseOptions();

                    int regNo = 0;
                    var regNoWithLetter = c1a.Trim().ToUpper();
                    if (!BadgeChecksum.TryParse(regNoWithLetter, out regNo))
                    {
                        await ReplyAsync($"_{regNoWithLetter} is not a valid badge number - checksum letter is missing or wrong._");
                        await c1();
                        return;
                    }

                    var records =
                        (await _pushNotificationChannelRepository.FindAllAsync(a => a.Uid.StartsWith("RegSys:") && a.Uid.EndsWith($":{regNo}")))
                        .ToList();

                    if (records.Count == 0)
                    {
                        await ReplyAsync($"*{title} - Result*\nRegNo {regNo} is not logged in on any known device.");
                        return;
                    }

                    var response = new StringBuilder();
                    response.AppendLine($"*{title} - Result*");
                    response.AppendLine($"RegNo *{regNo}* is logged in on *{records.Count}* devices:");
                    foreach (var record in records)
                        response.AppendLine(
                            $"`{record.Platform} {string.Join(",", record.Topics)} ({record.LastChangeDateTimeUtc})`");


                    await ReplyAsync(response.ToString());
                },"Cancel=/cancel");
            await c1();
        }

        public async Task CommandStatistics()
        {
            var records = (await _pushNotificationChannelRepository.FindAllAsync()).ToList();

            var devicesWithSessions =
                records.Where(a => a.Uid.StartsWith("RegSys:", StringComparison.CurrentCultureIgnoreCase));

            var devicesWithSessionCount = devicesWithSessions.Count();
            var uniqueUserIds = devicesWithSessions.Select(a => a.Uid).Distinct().Count();

            var groups = records.GroupBy(a => new Tuple<string, string>(a.Platform.ToString(),
                String.Join(", ", a.Topics.OrderByDescending(b => b))));

            var message = new StringBuilder();
            message.AppendLine($"*{records.Count}* devices in reach of global / targeted push.");

            foreach (var group in groups.OrderByDescending(g => g.Count()))
            {
                message.AppendLine($"`{group.Count().ToString().PadLeft(5)} {group.Key.Item1} - {group.Key.Item2}`");
            }

            message.AppendLine();
            message.AppendLine($"*{devicesWithSessionCount}* devices have an user signed in.");
            message.AppendLine($"*{uniqueUserIds}* unique user ids are present.");

            await ReplyAsync(message.ToString());
        }

        public async Task CommandUserAdmin()
        {
            Func<Task> c1 = null, c2 = null, c3 = null, c4 = null;
            var title = "User Management";

            c1 = () => AskAsync($"*{title}*\nWhat do you want to do?",
                async c1a =>
                {
                    switch (c1a.ToLower())
                    {
                        case ("*edituser"):
                            c2 = () => AskAsync($"*{title}*\nPlease type the user name (without @ prefix)", async c2a =>
                            {
                                await ClearLastAskResponseOptions();

                                var username = c2a;
                                var acl = await _userManager.GetAclForUserAsync<PermissionFlags>(username);

                                c3 = () => AskAsync($"*{title}*\nUser @{username.EscapeMarkdown()} has flags: `{acl}`",
                                    async c3a =>
                                    {
                                        if (c3a == "*back")
                                        {
                                            await c1();
                                            return;
                                        }

                                        var verbs = new[] {"add", "remove"}.ToList();
                                        var verb = c3a;
                                        if (!verbs.Contains(verb))
                                        {
                                            await c3();
                                            return;
                                        }

                                        var values = Enum.GetValues(typeof(PermissionFlags)).Cast<PermissionFlags>()
                                            .Where(a => acl.HasFlag(a) == (verb == "remove"))
                                            .ToList();

                                        await ReplyAsync(
                                            $"*{title}*\nModifying: @{username.EscapeMarkdown()}\n\nAvailable flags to *{verb}*: `{string.Join(",", values.Select(a => $"{a}"))}`");

                                        c4 = () => AskAsync($"Please type which flag to *{verb}*.", async c4a =>
                                        {
                                            await ClearLastAskResponseOptions();

                                            if (c4a == "*back")
                                            {
                                                await c3();
                                                return;
                                            }

                                            var selectedFlag = c4a;
                                            PermissionFlags typedFlag;
                                            if (!Enum.TryParse(selectedFlag, out typedFlag))
                                            {
                                                await ReplyAsync("Invalid flag.");
                                                await c4();
                                                return;
                                            }

                                            if (typedFlag.HasFlag(PermissionFlags.UserAdmin))
                                            {
                                                await ReplyAsync("Sorry - user administration flag may not be changed via the bot interface.");
                                                await c4();
                                                return;
                                            }

                                            var myPermissions = await GetPermissionsAsync();
                                            if (!HasPermission(myPermissions, typedFlag))
                                            {
                                                await ReplyAsync("Sorry - you may only add/remove flags that you own yourself.");
                                                await c4();
                                                return;
                                            }

                                            if (verb == "add") acl |= typedFlag;
                                            if (verb == "remove") acl &= ~typedFlag;

                                            await _userManager.SetAclForUserAsync(username, acl);
                                            await c3();
                                        },"Back=*back", "Cancel=/cancel");
                                        await c4();

                                    },"Add=add","Remove=remove","Back=*back","Cancel=/cancel");
                                await c3();

                            }, "Cancel=/cancel");
                            await c2();
                            break;

                        case ("*listusers"):
                            var users = await _userManager.GetUsersAsync();

                            var response = new StringBuilder();
                            response.AppendLine($"*{title}*\nCurrent Users in Database:\n");
                            foreach (var user in users.OrderBy(a => a.Username))
                            {
                                response.AppendLine($"@{user.Username.EscapeMarkdown()} - `{user.Acl}`");
                            }

                            await ReplyAsync(response.ToString());
                            await c1();
                            break;
                    }
                }, "List Users=*listUsers", "Edit User=*editUser", "Cancel=/cancel");
            await c1();
        }

        private IReplyMarkup MarkupFromCommands(string[] commands)
        {
            if (commands == null || commands.Length == 0) return new ReplyKeyboardRemove();
            //return new InlineKeyboardMarkup(commands.Select(c  => new[] { new InlineKeyboardButton(c, c) }).ToArray());
            return new InlineKeyboardMarkup(new [] {commands.Select(c =>
            {
                var parts = c.Split('=');

                if (parts.Length == 2) return InlineKeyboardButton.WithCallbackData(parts[0], parts[1]);
                return InlineKeyboardButton.WithCallbackData(c, c);
            }).ToArray()});
        }

        public async Task AskAsync(string question, Func<string, Task> responseCallBack, params string[] commandOptions)
        {
            Debug.WriteLine($"Bot -> @{_user.Username}: {question}");

            var message = await BotClient.SendTextMessageAsync(ChatId, question, ParseMode.Markdown, replyMarkup: MarkupFromCommands(commandOptions));
            _awaitingResponseCallback = responseCallBack;
            _lastAskMessage = message;
        }


        public ChatId ChatId { get; set; }

        public async Task ReplyAsync(string message, params string[] commandOptions)
        {
            Debug.WriteLine($"Bot -> @{_user.Username}: {message}");

            await BotClient.SendTextMessageAsync(ChatId, message, ParseMode.Markdown, replyMarkup: MarkupFromCommands(commandOptions));
        }


        private async Task CommandPinInfo()
        {
            Func<Task> c1 = null;
            var title = "PIN Info";

            c1 = () => AskAsync($"*{title} - Step 1 of 1*\nWhat's the attendees _registration number (including the letter at the end)_ on the badge?",
                async c1a =>
                {
                    await ClearLastAskResponseOptions();

                    int regNo = 0;
                    var regNoWithLetter = c1a.Trim().ToUpper();
                    if (!BadgeChecksum.TryParse(regNoWithLetter, out regNo))
                    {
                        await ReplyAsync($"_{regNoWithLetter} is not a valid badge number - checksum letter is missing or wrong._");
                        await c1();
                        return;
                    }

                    var record = await _regSysAlternativePinAuthenticationProvider.GetAlternativePinAsync(regNo);
                    
                    if (record == null)
                    {
                        await ReplyAsync($"Sorry, there is no pin record for RegNo {regNo}.");
                        return;
                    }

                    var response = new StringBuilder();
                    response.AppendLine($"RegNo: *{record.RegNo}*");
                    response.AppendLine($"NameOnBadge: *{record.NameOnBadge.EscapeMarkdown()}*");
                    response.AppendLine($"Pin: *{record.Pin}*");
                    response.AppendLine("\n```\nAll times are UTC.\n");
                    response.AppendLine($"Issued on {record.IssuedDateTimeUtc} by {record.IssuedByUid}");
                    response.AppendLine("");
                    response.AppendLine("Issue Log:");
                    response.AppendLine(string.Join("\n",
                        record.IssueLog.Select(a => $"- {a.RequestDateTimeUtc} {a.RequesterUid}")));

                    response.AppendLine("\nUsed for login at:");
                    response.AppendLine(string.Join("\n", record.PinConsumptionDatesUtc.Select(a => $"- {a}")));

                    response.AppendLine("```");

                    await ReplyAsync(response.ToString());
                },"Cancel=/cancel");
            await c1();


        }

        private async Task CommandCollectionGameRegisterFursuit()
        {
            Func<Task> askForRegNo = null, askForFursuitBadgeNo = null, askTokenValue = null;

            var title = "Collection Game Fursuit Registration";

            askForRegNo = () => AskAsync($"*{title} - Step 1 of 3*\nPlease enter the `attendee registration number` on the con badge.",
                async regNoAsString =>
                {
                    await ClearLastAskResponseOptions();

                    int regNo;
                    if (!Int32.TryParse(regNoAsString, out regNo))
                    {
                        await ReplyAsync($"*{regNoAsString}* is not a valid number.");
                        await askForRegNo();
                        return;
                    }

                    askForFursuitBadgeNo = () => AskAsync($"*{title} - Step 2 of 3*\nPlease enter the `fursuit badge number`.",
                        async fursuitBadgeNoAsString =>
                        {
                            await ClearLastAskResponseOptions();

                            int fursuitBadgeNo;
                            if (!Int32.TryParse(fursuitBadgeNoAsString, out fursuitBadgeNo))
                            {
                                await ReplyAsync($"*{fursuitBadgeNo}* is not a valid number.");
                                await askForFursuitBadgeNo();
                                return;
                            }

                            var badge = await _fursuitBadgeRepository.FindOneAsync(
                                a => a.ExternalReference == fursuitBadgeNo.ToString());

                            if (badge == null)
                            {
                                await ReplyAsync($"*Error:* No fursuit badge with no *{fursuitBadgeNo}* found. Aborting.");
                                return;
                            }

                            if (badge.OwnerUid != $"RegSys:{_conventionSettings.ConventionNumber}:{regNo}")
                            {
                                await ReplyAsync($"*Error*: Fursuit badge with no *{fursuitBadgeNo}* exists, but does *not* belong to reg no *{regNo}*. Aborting.");
                                return;
                            }

                            await ReplyAsync(
                                $"*{badge.Name.EscapeMarkdown()}* ({badge.Species.EscapeMarkdown()}, {badge.Gender.EscapeMarkdown()})");

                            var imageContent = new MemoryStream((await _fursuitBadgeImageRepository.FindOneAsync(badge.Id)).ImageBytes);
                            await BotClient.SendPhotoAsync(ChatId, new FileToSend(badge.Id.ToString(), imageContent));

                            askTokenValue = () => AskAsync(
                                $"*{title} - Step 3 of 3*\nPlease enter the `code/token` on the sticker that was applied to the badge.",
                                async tokenValue =>
                                {
                                    tokenValue = tokenValue.ToUpper();
                                    var registrationResult = await _collectingGameService.RegisterTokenForFursuitBadgeForOwnerAsync(
                                        badge.OwnerUid, badge.Id, tokenValue);

                                    if (registrationResult.IsSuccessful)
                                    {
                                        await ReplyAsync(
                                            $"*{title} Result*\n`Success!` - Token *{tokenValue}* successfully linked to *({badge.ExternalReference}) {badge.Name}*!\n\nNext one? /collectionGameRegisterFursuit");
                                    }
                                    else
                                    {
                                        await ReplyAsync(
                                            $"*Error*: `{registrationResult.ErrorMessage}`");
                                        await askTokenValue();
                                    }
                                }, "Cancel=/cancel");
                            await askTokenValue();
                        }, "Cancel=/cancel");
                    await askForFursuitBadgeNo();
                }, "Cancel=/cancel");
            await askForRegNo();
        }

        private async Task CommandPinRequest()
        {
            Func<Task> c1 = null, c2 = null, c3 = null;

            var title = "PIN Creation";
            var requesterUid = $"Telegram:@{_user.Username}";

            c1 = () => AskAsync($"*{title} - Step 1 of 3*\nWhat's the attendees _registration number (including the letter at the end)_ on the badge?", 
                async c1a =>
                {
                    await ClearLastAskResponseOptions();

                    int regNo = 0;
                    var regNoWithLetter = c1a.Trim().ToUpper();
                    if (!BadgeChecksum.TryParse(regNoWithLetter, out regNo))
                    {
                        await ReplyAsync($"_{regNoWithLetter} is not a valid badge number - checksum letter is missing or wrong._");
                        await c1();
                        return;
                    }

                    c2 = () => AskAsync($"*{title} - Step 2 of 3*\nOn badge no {regNo}, what is the _nickname_ printed on the badge (not the real name)?",
                        async c2a =>
                        {
                            await ClearLastAskResponseOptions();

                            var nameOnBadge = c2a.Trim();

                            c3 = () => AskAsync(
                                $"*{title} - Step 3 of 3*\nPlease confirm:\n\nThe badge no. is *{regNoWithLetter}*\n\nThe nickname on the badge is *{nameOnBadge.EscapeMarkdown()}*" +
                                "\n\n*You have verified the identity of the attendee by matching their real name against a legal form of identification.*",
                                async c3a =>
                                {
                                    if (c3a.Equals("*restart", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        await c1();
                                        return;
                                    }

                                    if (!c3a.Equals("*confirm", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        await c3();
                                        return;
                                    }

                                    var result = await _regSysAlternativePinAuthenticationProvider.RequestAlternativePinAsync(
                                        new RegSysAlternativePinRequest()
                                        {
                                            NameOnBadge = nameOnBadge,
                                            RegNoOnBadge = regNoWithLetter,
                                        }, requesterUid: requesterUid);

                                    var response = new StringBuilder();

                                    response.AppendLine($"*{title} - Completed*");
                                    response.AppendLine($"Registration Number: *{result.RegNo}*");
                                    response.AppendLine($"Name on Badge: *{result.NameOnBadge.EscapeMarkdown()}*");
                                    response.AppendLine($"PIN: *{result.Pin}*");
                                    response.AppendLine();
                                    response.AppendLine($"User can login to the Eurofurence Apps (mobile devices and web) with their registration number (*{result.RegNo}*) and PIN (*{result.Pin}*) as their password. They can type in any username, it does not matter.");
                                    response.AppendLine($"\n_Generation/Access of this PIN by {requesterUid.EscapeMarkdown()} has been recorded._");

                                    _logger.LogInformation(LogEvents.Audit, "{requesterUid} created PIN for {regNo} {nameOnBadge}", requesterUid, result.RegNo, result.NameOnBadge);

                                    await ReplyAsync(response.ToString());
                                }, "Confirm=*confirm","Restart=*restart","Cancel=/cancel");
                            await c3();
                        }, "Cancel=/cancel");
                    await c2();
                }, "Cancel=/cancel");
            await c1();
        }

        private async Task ProcessMessageAsync(string message, Message rawMessage = null)
        {
            if (message == "/cancel")
            {
                _awaitingResponseCallback = null;
                await BotClient.SendTextMessageAsync(ChatId, "Send /start for a list of commands.",
                    replyMarkup: new ReplyKeyboardRemove());
                return;
            }

            if (_awaitingResponseCallback != null)
            {
                var invokable = _awaitingResponseCallback.Invoke(message);
                _awaitingResponseCallback = null;
                await invokable;
                return;
            }

            _logger.LogInformation("@{username} sent {message} on root conversation.", _user.Username, message);

            var userPermissions = await GetPermissionsAsync();
            if (message == "/start")
            {
                var availableCommands = _commands
                    .Where(cmd => HasPermission(userPermissions, cmd.RequiredPermission))
                    .OrderBy(a => a.Command)
                    .ToList();

                var response = new StringBuilder();

                if (availableCommands.Count == 0)
                {
                    response.AppendLine(
                        "Sorry, I don't have any commands for you that you have access to. If you think this is in error, contact @Pinselohrkater");
                }
                else
                {
                    response.AppendLine("You have access to the following commands:\n");
                    foreach (var cmd in availableCommands)
                    {
                        response.AppendLine($"{cmd.Command} - {cmd.Description}");
                    }
                }
                await ReplyAsync(response.ToString());
                return;
            }

            var matchingCommand = _commands.SingleOrDefault(a => a.Command.Equals(message, StringComparison.CurrentCultureIgnoreCase));
            if (matchingCommand != null && HasPermission(userPermissions, matchingCommand.RequiredPermission))
            {
                await matchingCommand.CommandHandler();
                return;
            }

            if (rawMessage != null && HasPermission(PermissionFlags.All, userPermissions))
            {
                if (message.StartsWith("/chatid@", StringComparison.CurrentCultureIgnoreCase))
                {
                    await BotClient.SendTextMessageAsync(rawMessage.Chat.Id,
                        $"ChatId={rawMessage.Chat.Id} ({rawMessage.Chat.Title})");
                }
                if (message.StartsWith("/leave@", StringComparison.CurrentCultureIgnoreCase))
                {
                    await BotClient.LeaveChatAsync(rawMessage.Chat.Id);
                }
            }
        }

        private Task ClearInlineResponseOptions(int messageId)
        {
            return BotClient.EditMessageReplyMarkupAsync(ChatId, messageId, null);
        }

        private Task ClearLastAskResponseOptions()
        {
            try
            {
                if (_lastAskMessage != null) return ClearInlineResponseOptions(_lastAskMessage.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError("ClearLastAskResponseOptions failed: {Message} {StackTrace}",
                    ex.Message, ex.StackTrace);
            }

            return Task.CompletedTask;
        }

        public Task OnCallbackQueryAsync(CallbackQueryEventArgs e)
        {
            _user = e.CallbackQuery.Message.From;

            return BotClient.EditMessageReplyMarkupAsync(
                    e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, null)
                .ContinueWith(_ => ProcessMessageAsync(e.CallbackQuery.Data, e.CallbackQuery.Message));
        }


        public Task OnMessageAsync(MessageEventArgs e)
        {
            _user = e.Message.From;
            return ProcessMessageAsync(e.Message.Text, e.Message);
        }
    }
}
