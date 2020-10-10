using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrlyBot
{
    class Handlers
    {

        internal static async Task HELP(SocketCommandContext context)
        {
            await context.Channel.SendMessageAsync(@$"
:owl: **ORLY Bot Commands and help** :owl:
```
ORLY will only respond to users with an Administrator role.

> The blacklist roles are basically roles that are never
acted upon. The bot will pretend they don't exist.

orly.add_blacklisted_roles @role1 @role2 @role3
orly.remove_blacklisted_roles @role1 @role2 @role3
orly.clear_blacklisted_roles
orly.show_blacklisted_roles

> The report channel (disabled by default) is a channel
where the bot will report suspicious activity such as:

>> Quick leaving/rejoining of the server
>> Rejoining a server under a new username
>> Writing a message containing a suspicious word
>> Deleting a message containing a suspicious word within 5 minutes

orly.set_report_channel #channel_name
orly.disable_report_channel
orly.show_report_channel

>> You must separate words or group of words with a comma

orly.add_suspicious_words fries, ketchup, hot dog, double cheeseburger
orly.remove_suspicious_words bicycle, tennis, golf shoes, soccer ball
orly.clear_suspicious_words
orly.show_suspicious_words

>> You can ignore channels for suspicious detection

orly.add_ignored_channels #channel1 #channel2 #channel3
orly.remove_ignored_channels #channel1 #channel2 #channel3
orly.clear_ignored_channels
orly.show_ignored_channels

```
");
        }

        internal static async Task ADD_BLACKLISTED_ROLES(SocketUserMessage msg, SocketCommandContext context, SocketGuild guild)
        {
            var roles = msg.MentionedRoles.Select(it => it.Id.ToString()).ToList();
            if (roles.Count == 0)
            {
                await context.Channel.SendMessageAsync("no roles were provided");
                return;
            }

            dynamic dbBlacklistedRoles = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.BLACKLISTED_ROLES);
            if (dbBlacklistedRoles == null)
                dbBlacklistedRoles = new ExpandoObject();

            List<string> currentBlacklistedRoles = (dbBlacklistedRoles.roles as IEnumerable<object>)?.Select(it => it.ToString()).ToList();

            if (currentBlacklistedRoles == null)
                currentBlacklistedRoles = new List<string>();

            foreach (var role in roles)
                if (!currentBlacklistedRoles.Contains(role))
                    currentBlacklistedRoles.Add(role);

            dbBlacklistedRoles = new ExpandoObject();
            dbBlacklistedRoles.roles = currentBlacklistedRoles;

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.BLACKLISTED_ROLES, dbBlacklistedRoles);

            await context.Channel.SendMessageAsync("roles added to blacklist");
        }

        internal static async Task REMOVE_BLACKLISTED_ROLES(SocketUserMessage msg, SocketCommandContext context, SocketGuild guild)
        {
            var roles = msg.MentionedRoles.Select(it => it.Id.ToString()).ToList();
            if (roles.Count == 0)
            {
                await context.Channel.SendMessageAsync("no roles were provided");
                return;
            }

            dynamic dbBlacklistedRoles = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.BLACKLISTED_ROLES);
            if (dbBlacklistedRoles == null)
            {
                await context.Channel.SendMessageAsync("there are no blacklisted roles");
                return;
            }

            var currentBlacklistedRoles = (dbBlacklistedRoles.roles as IEnumerable<object>)?.Select(it => it.ToString()).ToList();
            if (currentBlacklistedRoles != null)
                foreach (var role in roles)
                    currentBlacklistedRoles.Remove(role);

            dbBlacklistedRoles = new ExpandoObject();
            dbBlacklistedRoles.roles = currentBlacklistedRoles;

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.BLACKLISTED_ROLES, dbBlacklistedRoles);

            await context.Channel.SendMessageAsync("roles removed from blacklist");
        }

        internal static async Task CLEAR_BLACKLISTED_ROLES(SocketCommandContext context, SocketGuild guild)
        {
            dynamic dbBlacklistedRoles = new ExpandoObject();
            dbBlacklistedRoles.roles = new List<string>();

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.BLACKLISTED_ROLES, dbBlacklistedRoles);

            await context.Channel.SendMessageAsync("roles cleared from blacklist");
        }

        internal static async Task SHOW_BLACKLISTED_ROLES(SocketCommandContext context, SocketGuild guild)
        {
            dynamic dbBlacklistedRoles = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.BLACKLISTED_ROLES);
            if (dbBlacklistedRoles == null)
            {
                await context.Channel.SendMessageAsync("there are no blacklisted roles");
                return;
            }
            var currentBlacklistedRoles = (dbBlacklistedRoles.roles as IEnumerable<object>)?.Select(it => it.ToString()).ToList();

            if (currentBlacklistedRoles == null || currentBlacklistedRoles.Count == 0)
            {
                await context.Channel.SendMessageAsync("there are no blacklisted roles");
                return;
            }

            var humanReadableBlacklistedRoles = guild.Roles.Where(it => currentBlacklistedRoles.Contains(it.Id.ToString())).Select(it2 => it2.Name);

            List<string> mess = new List<string>();

            mess.AddRange(new string[]
            {
                                "Currently blacklisted roles",
                                "```"
            });

            mess.AddRange(humanReadableBlacklistedRoles);
            mess.Add("```");

            string messAll = string.Join("\n", mess);

            await context.Channel.SendMessageAsync(messAll);
        }

        internal static async Task DISABLE_REPORT_CHANNEL(SocketCommandContext context, SocketGuild guild)
        {
            dynamic dbReportChannel = new ExpandoObject();

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.REPORT_CHANNEL, dbReportChannel);

            await context.Channel.SendMessageAsync("Reporting disabled");
        }

        internal static async Task SET_REPORT_CHANNEL(SocketMessage s, SocketCommandContext context, SocketGuild guild)
        {
            dynamic dbReportChannel = new ExpandoObject();
            var channel = s.MentionedChannels.Select(it => it.Id.ToString()).ToList();

            if (channel.Count == 0)
            {
                await context.Channel.SendMessageAsync("No channel provided");
                return;
            }
            else if (channel.Count > 1)
            {
                await context.Channel.SendMessageAsync("You can only set one channel");
                return;
            }

            dbReportChannel.channel = channel[0];

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.REPORT_CHANNEL, dbReportChannel);

            await context.Channel.SendMessageAsync("Channel set");
        }

        internal static async Task SHOW_IGNORED_CHANNELS(SocketCommandContext context, SocketGuild guild)
        {
            dynamic dbIgnoredChannels = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.IGNORED_CHANNELS);
            if (dbIgnoredChannels == null)
            {
                await context.Channel.SendMessageAsync("there are no ignored channels");
                return;
            }
            var currentIgnoredChannels = (dbIgnoredChannels.channels as IEnumerable<object>)?.Select(it => it.ToString()).ToList();

            if (currentIgnoredChannels == null || currentIgnoredChannels.Count == 0)
            {
                await context.Channel.SendMessageAsync("there are no ignored channels");
                return;
            }

            var humanReadableChannels = guild.Channels.Where(it => currentIgnoredChannels.Contains(it.Id.ToString())).Select(it2 => it2.Name);

            List<string> mess = new List<string>();

            mess.AddRange(new string[]
            {
                                "Currently ignored channels",
                                "```"
            });

            mess.AddRange(humanReadableChannels);
            mess.Add("```");

            string messAll = string.Join("\n", mess);

            await context.Channel.SendMessageAsync(messAll);
        }

        internal static async Task CLEAR_IGNORED_CHANNELS(SocketCommandContext context, SocketGuild guild)
        {
            dynamic dbIgnoredChannels = new ExpandoObject();
            dbIgnoredChannels.channels = new List<string>();

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.IGNORED_CHANNELS, dbIgnoredChannels);

            await context.Channel.SendMessageAsync("channels cleared from ignored list");
        }

        internal static async Task REMOVE_IGNORED_CHANNELS(SocketUserMessage msg, SocketCommandContext context, SocketGuild guild)
        {
            var channels = msg.MentionedChannels.Select(it => it.Id.ToString()).ToList();
            if (channels.Count == 0)
            {
                await context.Channel.SendMessageAsync("no channels were provided");
                return;
            }

            dynamic dbIgnoredChannels = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.IGNORED_CHANNELS);
            if (dbIgnoredChannels == null)
            {
                await context.Channel.SendMessageAsync("there are no ignored channels");
                return;
            }

            var currentIgnoredChannels = (dbIgnoredChannels.channels as IEnumerable<object>)?.Select(it => it.ToString()).ToList();
            if (currentIgnoredChannels != null)
                foreach (var channel in channels)
                    currentIgnoredChannels.Remove(channel);

            dbIgnoredChannels = new ExpandoObject();
            dbIgnoredChannels.channels = currentIgnoredChannels;

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.IGNORED_CHANNELS, dbIgnoredChannels);

            await context.Channel.SendMessageAsync("channels removed from ignored list");
        }

        internal static async Task ADD_IGNORED_CHANNELS(SocketUserMessage msg, SocketCommandContext context, SocketGuild guild)
        {
            var channels = msg.MentionedChannels.Select(it => it.Id.ToString()).ToList();
            if (channels.Count == 0)
            {
                await context.Channel.SendMessageAsync("no channels were provided");
                return;
            }

            dynamic dbIgnoredChannels = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.IGNORED_CHANNELS);
            if (dbIgnoredChannels == null)
                dbIgnoredChannels = new ExpandoObject();

            List<string> currentIgnoredChannels = (dbIgnoredChannels.channels as IEnumerable<object>)?.Select(it => it.ToString()).ToList();

            if (currentIgnoredChannels == null)
                currentIgnoredChannels = new List<string>();

            foreach (var channel in channels)
                if (!currentIgnoredChannels.Contains(channel))
                    currentIgnoredChannels.Add(channel);

            dbIgnoredChannels = new ExpandoObject();
            dbIgnoredChannels.channels = currentIgnoredChannels;

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.IGNORED_CHANNELS, dbIgnoredChannels);

            await context.Channel.SendMessageAsync("channels added to ignore list");
        }

        internal static async Task SHOW_SUSPICIOUS_WORDS(SocketCommandContext context, SocketGuild guild)
        {
            dynamic dbSuspiciousWords = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.SUSPICION_WORDS);
            if (dbSuspiciousWords == null)
            {
                await context.Channel.SendMessageAsync("the suspicious words list is empty");
                return;
            }

            var currentSuspiciousWords = (dbSuspiciousWords.words as IEnumerable<object>)?.Select(it => it.ToString()).ToList();

            if (currentSuspiciousWords == null || currentSuspiciousWords.Count == 0)
            {
                await context.Channel.SendMessageAsync("the suspicious words list is empty");
                return;
            }


            List<string> mess = new List<string>();

            mess.AddRange(new string[]
            {
                                "Suspicious words list",
                                "```"
            });

            mess.AddRange(currentSuspiciousWords);

            mess.Add("```");


            string messAll = string.Join("\n", mess);

            await context.Channel.SendMessageAsync(messAll);
        }

        internal static async Task CLEAR_SUSPICIOUS_WORDS(SocketCommandContext context, SocketGuild guild)
        {
            dynamic dbSuspiciousWords = new ExpandoObject();
            dbSuspiciousWords.words = new List<string>();

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.SUSPICION_WORDS, dbSuspiciousWords);

            await context.Channel.SendMessageAsync("words cleared from supicious word list");
        }

        internal static async Task REMOVE_SUSPICIOUS_WORDS(SocketUserMessage msg, SocketCommandContext context, SocketGuild guild, string[] msgParts)
        {
            var commandPart = msgParts[0];
            var remainder = msg.Content.Replace(commandPart, "");

            if (string.IsNullOrWhiteSpace(remainder))
            {
                await context.Channel.SendMessageAsync("no words provided");
                return;
            }

            var wordGroups = remainder.Split(',').Select(it => it.Trim().ToUpper()).ToList();

            dynamic dbSuspiciousWords = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.SUSPICION_WORDS);
            if (dbSuspiciousWords == null)
            {
                await context.Channel.SendMessageAsync("there are no words in the list");
                return;
            }

            var currentSuspiciousWords = (dbSuspiciousWords.words as IEnumerable<object>)?.Select(it => it.ToString()).ToList();
            if (currentSuspiciousWords != null)
                foreach (var word in wordGroups)
                    currentSuspiciousWords.Remove(word);

            dbSuspiciousWords = new ExpandoObject();
            dbSuspiciousWords.words = currentSuspiciousWords;

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.SUSPICION_WORDS, dbSuspiciousWords);

            await context.Channel.SendMessageAsync("words removed from supicious word list");
        }

        internal static async Task ADD_SUSPICIOUS_WORDS(SocketUserMessage msg, SocketCommandContext context, SocketGuild guild, string[] msgParts)
        {
            var commandPart = msgParts[0];
            var remainder = msg.Content.Replace(commandPart, "");

            if (string.IsNullOrWhiteSpace(remainder))
            {
                await context.Channel.SendMessageAsync("no words provided");
                return;
            }

            var wordGroups = remainder.Split(',').Select(it => it.Trim().ToUpper()).ToList();

            dynamic dbSuspiciousWords = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.SUSPICION_WORDS);

            if (dbSuspiciousWords == null)
                dbSuspiciousWords = new ExpandoObject();

            var currentSuspiciousWords = (dbSuspiciousWords.words as IEnumerable<object>)?.Select(it => it.ToString()).ToList();
            if (currentSuspiciousWords == null)
                currentSuspiciousWords = new List<string>();

            foreach (var word in wordGroups)
                if (!currentSuspiciousWords.Contains(word))
                    currentSuspiciousWords.Add(word);

            dbSuspiciousWords = new ExpandoObject();
            dbSuspiciousWords.words = currentSuspiciousWords;

            UserDB.db.SetDB(guild.Id.ToString(), Globals.words.SUSPICION_WORDS, dbSuspiciousWords);

            await context.Channel.SendMessageAsync("words added to supicious word list");
        }

        internal static async Task SHOW_REPORT_CHANNEL(SocketCommandContext context, SocketGuild guild)
        {
            dynamic dbReportChannel = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.REPORT_CHANNEL);

            if (dbReportChannel == null)
            {
                await context.Channel.SendMessageAsync("No reporting channel set");
                return;
            }

            string channel = dbReportChannel.channel.ToString();

            if (string.IsNullOrWhiteSpace(channel))
            {
                await context.Channel.SendMessageAsync("No reporting channel set");
                return;
            }

            List<string> mess = new List<string>();

            mess.AddRange(new string[]
            {
                                "Reporting channel",
                                "```"
            });

            var channelName = guild.Channels.First(it => it.Id.ToString() == channel).Name;

            mess.Add(channelName);
            mess.Add("```");

            string messAll = string.Join("\n", mess);

            await context.Channel.SendMessageAsync(messAll);
        }

        internal static async Task UserLeft(SocketGuildUser user)
        {
            //handle user left
            await OwlBrain.WatchForQuickReconnect(user);

        }


        internal static async Task UserJoined(SocketGuildUser user)
        {
            //handle user joined
            await OwlBrain.RestoreRoles(user);
        }

        internal static async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            if (before == null || after == null)
                return;

            if (before.Roles.Count != after.Roles.Count) //check if a role has changed
            {
                //a role was added or removed

                var server_id = after.Guild.Id.ToString();
                var user_id = after.Id.ToString();
                var roles = after.Roles.Select(it => it.Id.ToString()).ToList();

                var rolesAdded = after.Roles.Where(newRole => !before.Roles.Any(oldRole => oldRole == newRole));
                var rolesRemoved = before.Roles.Where(oldRole => !after.Roles.Any(newRole => newRole == oldRole));

                if (rolesAdded.Count() > 0 || rolesRemoved.Count() > 0)
                {
                    // a role was changed
                    UserDB.db.SetUser(server_id, user_id, roles);
                }

                if (rolesAdded.Count() > 0)
                {
                    // a role was added
                }

                if (rolesAdded.Count() > 0 || rolesRemoved.Count() > 0)
                {
                    // a role was removed
                }


            }
        }

    }
}
