using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;

namespace OrlyBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;

        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public CommandHandler(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;

            _discord.MessageReceived += OnMessageReceivedAsync;
            _discord.GuildMemberUpdated += Handlers.GuildMemberUpdated;
            _discord.UserJoined += Handlers.UserJoined;
            _discord.UserLeft += Handlers.UserLeft;
            _discord.MessageDeleted += Handlers.MessageDeleted;


        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {

            SocketUserMessage msg = s as SocketUserMessage;     // Ensure the message is from a user/bot
            if (msg == null) return;
            if (msg.Author.Id == _discord.CurrentUser.Id) return;     // Ignore self when checking commands
            if (msg.Channel.ToString().StartsWith('@')) return;     // Ignore DMs

            var context = new SocketCommandContext(_discord, msg);     // Create the command context

            var guild = context.Guild;
            var user = guild.Users.FirstOrDefault(iterator => iterator.Id == msg.Author.Id);

            if (user == null)
                return;

            var isUserAdmin = user.GuildPermissions.Administrator;

            if (string.IsNullOrWhiteSpace(msg.Content))
                return;

            if (await BotDetector.Instance.ScanMessage(context, msg)) // returns true if an action was taken
                return; //don't evaluate further if an action was taken

            if (await OwlBrain.ReportSuspiciousness(context, msg)) // returns true if an action was taken
                return; //don't evaluate further if an action was taken

            var mentionnedUsers = s.MentionedUsers;
            var mentionnedRoles = s.MentionedRoles;

            var msgParts = msg.Content.Split(" ");
            if (isUserAdmin)
                switch (msgParts[0].ToUpper())
                {

                    case "ORLY.HELP":
                        await Handlers.HELP(context);
                        return;

                    case "ORLY.ADD_BLACKLISTED_ROLES":
                        await Handlers.ADD_BLACKLISTED_ROLES(msg, context, guild);
                        return;

                    case "ORLY.REMOVE_BLACKLISTED_ROLES":
                        await Handlers.REMOVE_BLACKLISTED_ROLES(msg, context, guild);
                        return;

                    case "ORLY.CLEAR_BLACKLISTED_ROLES":
                        await Handlers.CLEAR_BLACKLISTED_ROLES(context, guild);
                        return;

                    case "ORLY.SHOW_BLACKLISTED_ROLES":
                        await Handlers.SHOW_BLACKLISTED_ROLES(context, guild);
                        return;

                    case "ORLY.SET_REPORT_CHANNEL":
                        await Handlers.SET_REPORT_CHANNEL(s, context, guild);
                        return;

                    case "ORLY.DISABLE_REPORT_CHANNEL":
                        await Handlers.DISABLE_REPORT_CHANNEL(context, guild);
                        return;

                    case "ORLY.SHOW_REPORT_CHANNEL":
                        await Handlers.SHOW_REPORT_CHANNEL(context, guild);
                        return;

                    case "ORLY.ADD_SUSPICIOUS_WORDS":
                        await Handlers.ADD_SUSPICIOUS_WORDS(msg, context, guild, msgParts);
                        return;

                    case "ORLY.REMOVE_SUSPICIOUS_WORDS":
                        await Handlers.REMOVE_SUSPICIOUS_WORDS(msg, context, guild, msgParts);
                        return;

                    case "ORLY.CLEAR_SUSPICIOUS_WORDS":
                        await Handlers.CLEAR_SUSPICIOUS_WORDS(context, guild);
                        return;

                    case "ORLY.SHOW_SUSPICIOUS_WORDS":
                        await Handlers.SHOW_SUSPICIOUS_WORDS(context, guild);
                        return;

                    case "ORLY.ADD_IGNORED_CHANNELS":
                        await Handlers.ADD_IGNORED_CHANNELS(msg, context, guild);
                        return;

                    case "ORLY.REMOVE_IGNORED_CHANNELS":
                        await Handlers.REMOVE_IGNORED_CHANNELS(msg, context, guild);
                        return;

                    case "ORLY.CLEAR_IGNORED_CHANNELS":
                        await Handlers.CLEAR_IGNORED_CHANNELS(context, guild);
                        return;

                    case "ORLY.SHOW_IGNORED_CHANNELS":
                        await Handlers.SHOW_IGNORED_CHANNELS(context, guild);
                        return;
                }


        }

    }
}
