using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Common.Validation;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class AdminConversation : Conversation, IConversation
    {
        private readonly IRegSysAlternativePinAuthenticationProvider _regSysAlternativePinAuthenticationProvider;


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
            RequestPin = 1
        }

        private User _user;

        private async Task<PermissionFlags> GetPermissionsAsync()
        {
            await Task.Delay(0); // Lookup.

            var tempAccessUsers = new string[] {"pinselohrkater", "zefirodragon", "fenrikur", "requinard "};

            if (tempAccessUsers.Any(a => a.Equals(_user.Username, StringComparison.CurrentCulture)))
                return PermissionFlags.RequestPin;
            
            return PermissionFlags.None;
        }

        private bool HasPermission(PermissionFlags userPermissionFlags, PermissionFlags requiredPermission)
        {
            return userPermissionFlags.HasFlag(requiredPermission);
        }


        public ITelegramBotClient BotClient { get; set; }

        private Func<MessageEventArgs, Task> _awaitingResponseCallback = null;

        public AdminConversation(IRegSysAlternativePinAuthenticationProvider regSysAlternativePinAuthenticationProvider)
        {
            _regSysAlternativePinAuthenticationProvider = regSysAlternativePinAuthenticationProvider;
            _commands = new List<CommandInfo>()
            {
                new CommandInfo()
                {
                    Command = "/pin",
                    Description = "Create an alternative password for an attendee to login",
                    RequiredPermission = PermissionFlags.RequestPin,
                    CommandHandler = CommandPinRequest
                }
            };
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

            var matchingCommand = _commands.SingleOrDefault(a => a.Command == e.Message.Text);
            if (matchingCommand != null && HasPermission(userPermissions, matchingCommand.RequiredPermission))
            {
                await matchingCommand.CommandHandler();
                return;
            }
        }
    }
}
