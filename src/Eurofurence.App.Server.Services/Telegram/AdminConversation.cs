using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Common.Validation;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class AdminConversation : Conversation, IConversation
    {
        private readonly AppDbContext _appDbContext;
        private readonly IUserManager _userManager;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly ITableRegistrationService _tableRegistrationService;
        private readonly IImageService _imageService;
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
            TableRegistrationAdmin = 1 << 8,
            All = (1 << 9) - 1,
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
            AppDbContext appDbContext,
            IUserManager userManager,
            IPrivateMessageService privateMessageService,
            ITableRegistrationService tableRegistrationService,
            IImageService imageService,
            ICollectingGameService collectingGameService,
            ConventionSettings conventionSettings,
            ILoggerFactory loggerFactory)
        {
            _appDbContext = appDbContext;
            _logger = loggerFactory.CreateLogger(GetType());
            _userManager = userManager;
            _privateMessageService = privateMessageService;
            _tableRegistrationService = tableRegistrationService;
            _imageService = imageService;
            _collectingGameService = collectingGameService;
            _conventionSettings = conventionSettings;

            _commands = new List<CommandInfo>()
            {
                new CommandInfo()
                {
                    Command = "/users",
                    Description = "Manage Users",
                    RequiredPermission = PermissionFlags.UserAdmin,
                    CommandHandler = CommandUserAdmin
                },
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
                    Command = "/fursuitBadgeById",
                    Description = "Lookup fursuit badge (by id)",
                    RequiredPermission = PermissionFlags.CollectionGameAdmin,
                    CommandHandler = CommandFursuitBadge
                },
                new CommandInfo()
                {
                    Command = "/collectionGameRegisterFursuit",
                    Description = "Register a fursuit for the game",
                    RequiredPermission = PermissionFlags.CollectionGameAdmin,
                    CommandHandler = CommandCollectionGameRegisterFursuit
                },
                new CommandInfo()
                {
                    Command = "/collectionGameUnban",
                    Description = "Unban someone from the game",
                    RequiredPermission = PermissionFlags.CollectionGameAdmin,
                    CommandHandler = CommandCollectionGameUnban
                },
                new CommandInfo()
                {
                    Command = "/tableRegistration",
                    Description = "Manage Table Registrations",
                    RequiredPermission = PermissionFlags.TableRegistrationAdmin,
                    CommandHandler = CommandTableRegistration
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
                        await _appDbContext.FursuitBadges
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.ExternalReference == badgeNo.ToString());

                    if (badge == null)
                    {
                        await ReplyAsync($"*{title} - Result*\nNo badge found with it *{badgeNo}*.");
                        return;
                    }

                    await ReplyAsync(
                        $"*{title} - Result*\nNo: *{badgeNo}*\nOwner: *{badge.OwnerUid}*\nName: *{badge.Name.RemoveMarkdown()}*\nSpecies: *{badge.Species.RemoveMarkdown()}*\nGender: *{badge.Gender.RemoveMarkdown()}*\nWorn By: *{badge.WornBy.RemoveMarkdown()}*\n\nLast Change (UTC): {badge.LastChangeDateTimeUtc}");

                    if (badge.ImageId != null)
                        await BotClient.SendPhotoAsync(chatId: ChatId,
                            photo: new InputFileStream(
                                await _imageService.GetImageContentByImageIdAsync((Guid)badge.ImageId)));
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
            var requesterUid = $"Telegram:@{_user.Username}";

            c1 = () => AskAsync(
                $"*{title} - Step 1 of 1*\nWhat's the attendees _registration number (including the letter at the end)_ on the badge?",
                async c1a =>
                {
                    await ClearLastAskResponseOptions();

                    int regNo = 0;
                    var regNoWithLetter = c1a.Trim().ToUpper();
                    if (!BadgeChecksum.TryParse(regNoWithLetter, out regNo))
                    {
                        await ReplyAsync(
                            $"_{regNoWithLetter} is not a valid badge number - checksum letter is missing or wrong._");
                        await c1();
                        return;
                    }

                    var devices = await _appDbContext.RegistrationIdentities
                        .Where(x => x.RegSysId == regNo.ToString())
                        .Join(
                            _appDbContext.DeviceIdentities,
                            x => x.IdentityId,
                            x => x.IdentityId,
                            (_, x) => x
                        )
                        .ToListAsync();

                    if (devices.Count == 0)
                    {
                        await ReplyAsync(
                            $"*WARNING: RegNo {regNo} is not logged in on any known device - they will receive the message when they login.*");
                    }

                    c2 = () => AskAsync($"*{title} - Step 2 of 3*\nPlease specify the subject of the message.",
                        async subject =>
                        {
                            await ClearLastAskResponseOptions();
                            c3 = () => AskAsync($"*{title} - Step 3 of 3*\nPlease specify the body of the message.",
                                async body =>
                                {
                                    await ClearLastAskResponseOptions();
                                    var from = $"{_user.FirstName} {_user.LastName}";

                                    c4 = () => AskAsync(
                                        $"*{title} - Review*\n\nFrom: *{from.EscapeMarkdown()}*\nTo: *{regNo}*\nSubject: *{subject.EscapeMarkdown()}*\n\nMessage:\n*{body.EscapeMarkdown()}*\n\n_Message will be placed in the users inbox and directly pushed to _*{devices.Count}*_ devices._",
                                        async c4a =>
                                        {
                                            if (c4a != "*send")
                                            {
                                                await c3();
                                                return;
                                            }

                                            var recipientUid =
                                                $"RegSys:{_conventionSettings.ConventionIdentifier}:{regNo}";
                                            var messageId = await _privateMessageService.SendPrivateMessageAsync(
                                                new SendPrivateMessageByRegSysRequest()
                                                {
                                                    AuthorName = $"{from}",
                                                    RecipientUid = regNo.ToString(),
                                                    Message = body,
                                                    Subject = subject,
                                                    ToastTitle = "You received a new personal message",
                                                    ToastMessage = "Open the Eurofurence App to read it."
                                                });

                                            _logger.LogInformation(
                                                LogEvents.Audit,
                                                "{requesterUid} sent a PM {messsageId} to {recipientUid} via Telegram Bot",
                                                requesterUid, messageId, recipientUid);

                                            await ReplyAsync("Message sent.");
                                        }, "Send=*send", "Cancel=/cancel");
                                    await c4();
                                }, "Cancel=/cancel");
                            await c3();
                        }, "Cancel=/cancel");

                    await c2();
                }, "Cancel=/cancel");
            await c1();
        }

        public async Task CommandCollectionGameUnban()
        {
            Func<Task> c1 = null;
            var title = "Collecting Game Unban";

            c1 = () => AskAsync(
                $"*{title} - Step 1 of 1*\nWhat's the attendees _registration number (including the letter at the end)_ on the badge?",
                async regNoAsString =>
                {
                    await ClearLastAskResponseOptions();

                    int regNo = 0;
                    var regNoWithLetter = regNoAsString.Trim().ToUpper();
                    if (!BadgeChecksum.TryParse(regNoWithLetter, out regNo))
                    {
                        await ReplyAsync(
                            $"_{regNoWithLetter} is not a valid badge number - checksum letter is missing or wrong._");
                        await c1();
                        return;
                    }

                    var result = await _collectingGameService.UnbanPlayerAsync(
                        $"RegSys:{_conventionSettings.ConventionIdentifier}:{regNo}");

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

            c1 = () => AskAsync(
                $"*{title} - Step 1 of 1*\nWhat's the attendees _registration number (including the letter at the end)_ on the badge?",
                async c1a =>
                {
                    await ClearLastAskResponseOptions();

                    int regNo = 0;
                    var regNoWithLetter = c1a.Trim().ToUpper();
                    if (!BadgeChecksum.TryParse(regNoWithLetter, out regNo))
                    {
                        await ReplyAsync(
                            $"_{regNoWithLetter} is not a valid badge number - checksum letter is missing or wrong._");
                        await c1();
                        return;
                    }

                    var devices = await _appDbContext.RegistrationIdentities
                        .Where(x => x.RegSysId == regNo.ToString())
                        .Join(
                            _appDbContext.DeviceIdentities,
                            x => x.IdentityId,
                            x => x.IdentityId,
                            (_, x) => x
                        )
                        .ToListAsync();

                    if (devices.Count == 0)
                    {
                        await ReplyAsync($"*{title} - Result*\nRegNo {regNo} is not logged in on any known device.");
                        return;
                    }

                    var response = new StringBuilder();
                    response.AppendLine($"*{title} - Result*");
                    response.AppendLine($"RegNo *{regNo}* is logged in on *{devices.Count}* devices:");
                    foreach (var device in devices)
                    {
                        response.AppendLine(
                            $"`{Enum.GetName(device.DeviceType)} {device.IdentityId} ({device.LastChangeDateTimeUtc})`");
                    }


                    await ReplyAsync(response.ToString());
                }, "Cancel=/cancel");
            await c1();
        }

        public async Task CommandStatistics()
        {
            var devicesWithSessionCount = await _appDbContext.DeviceIdentities.CountAsync();

            var uniqueUserIds = await _appDbContext.RegistrationIdentities
                .Join(
                    _appDbContext.DeviceIdentities,
                    x => x.IdentityId,
                    x => x.IdentityId,
                    (x, _) => x.RegSysId
                )
                .Distinct()
                .CountAsync();

            var groups = await _appDbContext.DeviceIdentities
                .AsNoTracking()
                .GroupBy(x => x.DeviceType)
                .ToListAsync();

            var message = new StringBuilder();
            message.AppendLine(
                $"*{await _appDbContext.DeviceIdentities.CountAsync()}* devices in reach of global / targeted push.");

            foreach (var group in groups.OrderByDescending(g => g.Count()))
            {
                message.AppendLine($"`{group.Count().ToString().PadLeft(5)} {Enum.GetName(group.Key)}`");
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

                                        var verbs = new[] { "add", "remove" }.ToList();
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
                                                await ReplyAsync(
                                                    "Sorry - user administration flag may not be changed via the bot interface.");
                                                await c4();
                                                return;
                                            }

                                            var myPermissions = await GetPermissionsAsync();
                                            if (!HasPermission(myPermissions, typedFlag))
                                            {
                                                await ReplyAsync(
                                                    "Sorry - you may only add/remove flags that you own yourself.");
                                                await c4();
                                                return;
                                            }

                                            if (verb == "add") acl |= typedFlag;
                                            if (verb == "remove") acl &= ~typedFlag;

                                            await _userManager.SetAclForUserAsync(username, acl);
                                            await c3();
                                        }, "Back=*back", "Cancel=/cancel");
                                        await c4();
                                    }, "Add=add", "Remove=remove", "Back=*back", "Cancel=/cancel");
                                await c3();
                            }, "Cancel=/cancel");
                            await c2();
                            break;

                        case ("*listusers"):
                            var users = _userManager.GetUsers();

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

        private IReplyMarkup MarkupFromCommands(string[] commands, bool pivot = false)
        {
            if (commands == null || commands.Length == 0) return new ReplyKeyboardRemove();
            //return new InlineKeyboardMarkup(commands.Select(c  => new[] { new InlineKeyboardButton(c, c) }).ToArray());


            var inlineCommands = commands.Select(c =>
            {
                var parts = c.Split('=');

                if (parts.Length == 2) return InlineKeyboardButton.WithCallbackData(parts[0], parts[1]);
                return InlineKeyboardButton.WithCallbackData(c, c);
            }).ToArray();


            if (pivot)
                return new InlineKeyboardMarkup(inlineCommands.Select(_ => new[] { _ }).ToArray());

            return new InlineKeyboardMarkup(new[] { inlineCommands });
        }

        public Task AskAsync(string question, Func<string, Task> responseCallBack, params string[] commandOptions)
        {
            return AskAsync(question, responseCallBack, false, commandOptions);
        }

        public async Task AskAsync(string question, Func<string, Task> responseCallBack, bool pivot,
            params string[] commandOptions)
        {
            Debug.WriteLine($"Bot -> @{_user.Username}: {question}");

            var message = await BotClient.SendTextMessageAsync(chatId: ChatId, text: question,
                parseMode: ParseMode.Markdown, replyMarkup: MarkupFromCommands(commandOptions, pivot: pivot));
            _awaitingResponseCallback = responseCallBack;
            _lastAskMessage = message;
        }

        public ChatId ChatId { get; set; }

        public async Task ReplyAsync(string message, params string[] commandOptions)
        {
            Debug.WriteLine($"Bot -> @{_user.Username}: {message}");

            await BotClient.SendTextMessageAsync(chatId: ChatId, text: message, parseMode: ParseMode.Markdown,
                replyMarkup: MarkupFromCommands(commandOptions));
        }

        private async Task CommandTableRegistration()
        {
            Func<Task> c1 = null, c2 = null, c3 = null;
            var title = "Table Registration";
            var requesterUid = $"Telegram:@{_user.Username}";

            c1 = () => AskAsync($"*{title}* - What would you like to do?",
                async c1a =>
                {
                    if (c1a == "*list")
                    {
                        var records = _tableRegistrationService.GetRegistrations(Domain.Model.ArtistsAlley
                            .TableRegistrationRecord.RegistrationStateEnum.Pending);

                        var list = records.Select(record =>
                            $"{record.OwnerUid} {record.OwnerUsername.RemoveMarkdown()}=*edit-{record.Id}"
                        ).ToList();

                        list.Add("Cancel=/cancel");

                        var response = $"There are currently *{records.Count()}* registrations pending reviews.";

                        await ReplyAsync(response);

                        c2 = () => AskAsync("Which one would you like to process?",
                            async c2a =>
                            {
                                var id = Guid.Parse(c2a.Split(new[] { '-' }, 2)[1]);

                                var nextRecord = records.SingleOrDefault(a =>
                                    a.Id == id && a.State == Domain.Model.ArtistsAlley.TableRegistrationRecord
                                        .RegistrationStateEnum.Pending);

                                if (nextRecord == null)
                                {
                                    await ReplyAsync("Registration not found (maybe it has already been processed?)");
                                }
                                else
                                {
                                    var message = new StringBuilder();

                                    message.AppendLine("You are reviewing:");
                                    message.AppendLine(
                                        $"*Id:*`{nextRecord.Id}` (from: {nextRecord.OwnerUid} {nextRecord.OwnerUsername.RemoveMarkdown()})\n");

                                    message.AppendLine($"Location: `{nextRecord.Location.RemoveMarkdown()}`");
                                    message.AppendLine($"Display Name: *{nextRecord.DisplayName.RemoveMarkdown()}*");
                                    message.AppendLine($"Website URL: *{nextRecord.WebsiteUrl.RemoveMarkdown()}*");
                                    message.AppendLine(
                                        $"Telegram Handle: *{nextRecord.TelegramHandle.RemoveMarkdown()}*");
                                    message.AppendLine(
                                        $"Short Description: _{nextRecord.ShortDescription.RemoveMarkdown()}_\n");
                                    message.AppendLine(
                                        $"Image: {(nextRecord.Image != null ? "Available (see below)" : "Not provided")}");

                                    await ReplyAsync(message.ToString());
                                    if (nextRecord.Image != null)
                                    {
                                        var stream =
                                            await _imageService.GetImageContentByImageIdAsync(nextRecord.Image.Id);
                                        await BotClient.SendPhotoAsync(ChatId, new InputFileStream(stream));
                                        await stream.DisposeAsync();
                                    }

                                    c3 = () => AskAsync(
                                        $"Do you wish to approve `{nextRecord.Id}`? Doing so will trigger an post both to the Telegram announcement channel.",
                                        async c3a =>
                                        {
                                            await ReplyAsync("This will take just a moment... please wait...");

                                            if (c3a == "*approve")
                                                await _tableRegistrationService.ApproveByIdAsync(nextRecord.Id,
                                                    requesterUid);
                                            if (c3a == "*reject")
                                                await _tableRegistrationService.RejectByIdAsync(nextRecord.Id,
                                                    requesterUid);

                                            await ReplyAsync(
                                                "Done. Send /tableRegistration again if you wish to review/view further items.");
                                        }, "Approve=*approve", "Reject=*reject", "Cancel=/cancel");

                                    await c3();
                                }
                            },
                            pivot: true,
                            list.ToArray());

                        await c2();
                    }
                }, "List all pending=*list", "Cancel=/cancel");
            await c1();
        }

        private async Task CommandCollectionGameRegisterFursuit()
        {
            Func<Task> askForRegNo = null, askForFursuitBadgeNo = null, askTokenValue = null;

            var title = "Collection Game Fursuit Registration";

            askForRegNo = () =>
                AskAsync($"*{title} - Step 1 of 3*\nPlease enter the `attendee registration number` on the con badge.",
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

                        askForFursuitBadgeNo = () =>
                            AskAsync($"*{title} - Step 2 of 3*\nPlease enter the `fursuit badge number`.",
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

                                    var badge = await _appDbContext.FursuitBadges
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(
                                            a => a.ExternalReference == fursuitBadgeNo.ToString());

                                    if (badge == null)
                                    {
                                        await ReplyAsync(
                                            $"*Error:* No fursuit badge with no *{fursuitBadgeNo}* found. Aborting.");
                                        return;
                                    }

                                    if (badge.OwnerUid != $"RegSys:{_conventionSettings.ConventionIdentifier}:{regNo}")
                                    {
                                        await ReplyAsync(
                                            $"*Error*: Fursuit badge with no *{fursuitBadgeNo}* exists, but does *not* belong to reg no *{regNo}*. Aborting.");
                                        return;
                                    }

                                    await ReplyAsync(
                                        $"*{badge.Name.EscapeMarkdown()}* ({badge.Species.EscapeMarkdown()}, {badge.Gender.EscapeMarkdown()})");

                                    if (badge.ImageId != null)
                                    {
                                        var imageContent =
                                            await _imageService.GetImageContentByImageIdAsync((Guid)badge.ImageId);
                                        await BotClient.SendPhotoAsync(ChatId, new InputFileStream(imageContent));
                                        await imageContent.DisposeAsync();
                                    }

                                    askTokenValue = () => AskAsync(
                                        $"*{title} - Step 3 of 3*\nPlease enter the `code/token` on the sticker that was applied to the badge.",
                                        async tokenValue =>
                                        {
                                            tokenValue = tokenValue.ToUpper();
                                            var registrationResult = await _collectingGameService
                                                .RegisterTokenForFursuitBadgeForOwnerAsync(
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
                        "Sorry, I don't have any commands for you that you have access to. If you think this is in error, contact @Fenrikur");
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

            var matchingCommand =
                _commands.SingleOrDefault(a => a.Command.Equals(message, StringComparison.CurrentCultureIgnoreCase));
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

        public Task OnCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            _user = callbackQuery.Message.From;

            return BotClient.EditMessageReplyMarkupAsync(
                    callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, null)
                .ContinueWith(_ => ProcessMessageAsync(callbackQuery.Data, callbackQuery.Message));
        }


        public Task OnMessageAsync(Message message)
        {
            _user = message.From;
            return ProcessMessageAsync(message.Text, message);
        }
    }
}