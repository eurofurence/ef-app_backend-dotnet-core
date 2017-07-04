using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Common.Validation;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Eurofurence.App.Server.Services.Abstractions.Telegram;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class AdminConversation : Conversation, IConversation
    {
        private readonly ITelegramUserManager _telegramUserManager;
        private readonly IRegSysAlternativePinAuthenticationProvider _regSysAlternativePinAuthenticationProvider;
        private readonly IEntityRepository<PushNotificationChannelRecord> _pushNotificationChannelRepository;


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
            Locate = 1 << 4
        }

        private User _user;

        private Task<PermissionFlags> GetPermissionsAsync()
        {
            return _telegramUserManager.GetAclForUserAsync<PermissionFlags>(_user.Username);
        }

        private bool HasPermission(PermissionFlags userPermissionFlags, PermissionFlags requiredPermission)
        {
            return userPermissionFlags.HasFlag(requiredPermission);
        }


        public ITelegramBotClient BotClient { get; set; }

        private Func<MessageEventArgs, Task> _awaitingResponseCallback = null;

        public AdminConversation(
            ITelegramUserManager telegramUserManager,
            IRegSysAlternativePinAuthenticationProvider regSysAlternativePinAuthenticationProvider,
            IEntityRepository<PushNotificationChannelRecord> pushNotificationChannelRepository
            )
        {
            _telegramUserManager = telegramUserManager;
            _regSysAlternativePinAuthenticationProvider = regSysAlternativePinAuthenticationProvider;
            _pushNotificationChannelRepository = pushNotificationChannelRepository;

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
                }
            };
        }

        public async Task CommandLocate()
        {
            Func<Task> c1 = null, c2 = null, c3 = null;
            var title = "Locate User";

            c1 = () => AskAsync($"*{title} - Step 1 of 1*\nWhat's the attendees _registration number (including the digit at the end)_ on the badge? (or /cancel)",
                async c1a =>
                {
                    int regNo = 0;
                    var regNoWithLetter = c1a.Message.Text.Trim().ToUpper();
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
                });
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

            c1 = () => AskAsync($"*{title}*\nWhat do you want to do?\n\n/listUsers\n/editUser\n\n/cancel",
                async c1a =>
                {
                    switch (c1a.Message.Text.ToLower())
                    {
                        case ("/edituser"):
                            c2 = () => AskAsync($"*{title}*\nPlease type the user name (without @ prefix)", async c2a =>
                            {
                                var username = c2a.Message.Text;
                                var acl = await _telegramUserManager.GetAclForUserAsync<PermissionFlags>(username);

                                c3 = () => AskAsync($"*{title}*\nUser @{username} has flags: `{acl}`\n\n/add or /remove a flag\n/cancel (changes are already saved)",
                                    async c3a =>
                                    {
                                        var verbs = new[] {"/add", "/remove"}.ToList();
                                        var verb = c3a.Message.Text;
                                        if (!verbs.Contains(verb))
                                        {
                                            await c3();
                                            return;
                                        }

                                        verb = verb.Replace("/", "");

                                        var values = Enum.GetValues(typeof(PermissionFlags)).Cast<PermissionFlags>()
                                            .Where(a => acl.HasFlag(a) == (verb == "remove"))
                                            .ToList();

                                        await ReplyAsync(
                                            $"*{title}*\nModifying: @{username}\n\nAvailable flags to *{verb}*: `{string.Join(",", values.Select(a => $"{a}"))}`");

                                        c4 = () => AskAsync($"Please type which flag to *{verb}* or /cancel.", async c4a =>
                                        {
                                            var selectedFlag = c4a.Message.Text;
                                            PermissionFlags typedFlag;
                                            if (!Enum.TryParse(selectedFlag, out typedFlag))
                                            {
                                                await ReplyAsync("Invalid flag.");
                                                await c4();
                                                return;
                                            }

                                            if (verb == "add") acl |= typedFlag;
                                            if (verb == "remove") acl &= ~typedFlag;

                                            await _telegramUserManager.SetAclForUserAsync(username, acl);

                                            await c3();
                                        });
                                        await c4();

                                    });
                                await c3();

                            });
                            await c2();
                            break;

                        case ("/listusers"):
                            var users = await _telegramUserManager.GetUsersAsync();

                            var response = new StringBuilder();
                            response.AppendLine($"*{title}*\nCurrent Users in Database:\n");
                            foreach (var user in users)
                            {
                                response.AppendLine($"@{user.Username} - `{user.Acl}`");
                            }

                            await ReplyAsync(response.ToString());
                            await c1();
                            break;
                    }
                });
            await c1();
        }

        public async Task AskAsync(string question, Func<MessageEventArgs, Task> responseCallBack)
        {
            Debug.WriteLine($"Bot -> @{_user.Username}: {question}");

            await BotClient.SendTextMessageAsync(ChatId, question, ParseMode.Markdown);
            _awaitingResponseCallback = responseCallBack;
        }


        public ChatId ChatId { get; set; }

        public async Task ReplyAsync(string message)
        {
            Debug.WriteLine($"Bot -> @{_user.Username}: {message}");

            await BotClient.SendTextMessageAsync(ChatId, message, ParseMode.Markdown);
        }

        public async Task OnCallbackQueryAsync(CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Task.Delay(0);
        }

        private async Task CommandPinInfo()
        {
            Func<Task> c1 = null, c2 = null, c3 = null;
            var title = "PIN Info";

            c1 = () => AskAsync($"*{title} - Step 1 of 1*\nWhat's the attendees _registration number (including the digit at the end)_ on the badge? (or /cancel)",
                async c1a =>
                {
                    int regNo = 0;
                    var regNoWithLetter = c1a.Message.Text.Trim().ToUpper();
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
                    response.AppendLine($"NameOnBadge: *{record.NameOnBadge}*");
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
                });
            await c1();


        }

        private async Task CommandPinRequest()
        {
            Func<Task> c1 = null, c2 = null, c3 = null;

            var title = "PIN Creation";
            var requesterUid = $"Telegram:@{_user.Username}";

            c1 = () => AskAsync($"*{title} - Step 1 of 3*\nWhat's the attendees _registration number (including the digit at the end)_ on the badge? (or /cancel)", 
                async c1a =>
                {
                    int regNo = 0;
                    var regNoWithLetter = c1a.Message.Text.Trim().ToUpper();
                    if (!BadgeChecksum.TryParse(regNoWithLetter, out regNo))
                    {
                        await ReplyAsync($"_{regNoWithLetter} is not a valid badge number - checksum letter is missing or wrong._");
                        await c1();
                        return;
                    }

                    c2 = () => AskAsync($"*{title} - Step 2 of 3*\nOn badge no {regNo}, what is the _nickname_ printed on the badge (not the real name)? (or /cancel)",
                        async c2a =>
                        {
                            var nameOnBadge = c2a.Message.Text.Trim();

                            c3 = () => AskAsync(
                                $"*{title} - Step 3 of 3*\nPlease confirm:\n\nThe badge no. is *{regNoWithLetter}*\n\nThe nickname on the badge is *{nameOnBadge}*" +
                                "\n\n*You have verified the identity of the attendee by matching their real name on badge against a legal form of identification.*\n\nYou may /confirm, /restart or /cancel",
                                async c3a =>
                                {
                                    if (c3a.Message.Text.Equals("/restart", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        await c1();
                                        return;
                                    }

                                    if (!c3a.Message.Text.Equals("/confirm", StringComparison.CurrentCultureIgnoreCase))
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
                                    response.AppendLine($"Name on Badge: *{result.NameOnBadge}*");
                                    response.AppendLine($"PIN: *{result.Pin}*");
                                    response.AppendLine();
                                    response.AppendLine($"User can login to the Eurofurence Apps (mobile devices and web) with their registration number (*{result.RegNo}*) and PIN (*{result.Pin}*) as their password. They can type in any username, it does not matter.");
                                    response.AppendLine($"\n_Generation/Access of this PIN by {requesterUid} has been recorded._");

                                    await ReplyAsync(response.ToString());
                                });
                            await c3();
                        });
                    await c2();
                });
            await c1();
        }

        public async Task OnMessageAsync(MessageEventArgs e)
        {
            Debug.WriteLine($"@{e.Message.From.Username} -> Bot: {e.Message.Text}");
            _user = e.Message.From;

            if (e.Message.Text == "/cancel")
            {
                _awaitingResponseCallback = null;
                await BotClient.SendTextMessageAsync(ChatId, "Cancelled. Send /start for a list of commands.",
                    replyMarkup: new ReplyKeyboardRemove());
                return;
            }

            if (_awaitingResponseCallback != null)
            {
                var invokable = _awaitingResponseCallback.Invoke(e);
                _awaitingResponseCallback = null;
                await invokable;
                return;
            }

            var userPermissions = await GetPermissionsAsync();
            if (e.Message.Text == "/start")
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

            var matchingCommand = _commands.SingleOrDefault(a => a.Command.Equals(e.Message.Text, StringComparison.CurrentCultureIgnoreCase));
            if (matchingCommand != null && HasPermission(userPermissions, matchingCommand.RequiredPermission))
            {
                await matchingCommand.CommandHandler();
                return;
            }
        }
    }
}
