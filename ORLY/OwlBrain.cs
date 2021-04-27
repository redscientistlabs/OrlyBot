using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrlyBot
{
    static class OwlBrain
    {
        internal static async Task<dynamic> GetUserStatus(SocketGuildUser user)
        {
            dynamic userStatus = new ExpandoObject();

            var server_id = user.Guild.Id.ToString();
            var user_id = user.Id.ToString();
            var roles = user.Roles.Select(it => it.Id.ToString()).ToList();

            //fetch local data for user, or create new data if doesn't exist

            var dbUser = UserDB.db.GetUser(server_id, user_id);
            if (dbUser == null)
                dbUser = UserDB.db.SetUser(server_id, user_id, roles);

            var user_db_roles = (dbUser.roles as IEnumerable<object>)?.Select(it => it.ToString()).ToList();
            var user_live_roles = user.Roles.Select(it => it.Id.ToString()).ToList();

            var roles_only_in_db = user_db_roles.Where(db_role => !user_live_roles.Any(live_role => live_role == db_role)).ToList();
            var roles_only_in_live = user_live_roles.Where(live_role => !user_db_roles.Any(db_role => db_role == live_role)).ToList();
            var roles_to_add = user.Guild.Roles.Where(role => roles_only_in_db.Contains(role.Id.ToString())).ToList();
            var roles_to_remove = user.Guild.Roles.Where(role => roles_only_in_live.Contains(role.Id.ToString())).ToList();

            //dynamic object userStatus
            userStatus.user_db_roles = user_db_roles;               //List<string>
            userStatus.user_live_roles = user_live_roles;           //List<string>

            userStatus.roles_only_in_db = roles_only_in_db;         //List<string>
            userStatus.roles_only_in_live = roles_only_in_live;     //List<string>
            userStatus.roles_to_add = roles_to_add;                 //List<SocketRole>
            userStatus.roles_to_remove = roles_to_remove;           //List<SocketRole>

            return userStatus;
        }
        internal static async Task RunStartupJob(ServiceCollection services, bool restoreAllFromDb = false)
        {
            if (services != null && Globals.discord == null)
            {
                //set local references to services
                Globals.discord = (DiscordSocketClient)services.FirstOrDefault(it => it.ServiceType == typeof(DiscordSocketClient))?.ImplementationInstance;
            }

            //wait a fat 20 seconds for the bot to connect to the servers
            await Task.Delay(1000 * 20);


            //Startup job for resyncing accounts.
            foreach (var guild in Globals.discord.Guilds)   //for each server
            {
                //run server-level stuff

                var allUsers = new List<SocketGuildUser>();
                await guild.DownloadUsersAsync();

                var server_id = guild.Id.ToString();
                var blacklistedRolesDb = UserDB.db.GetDB(server_id, Globals.words.BLACKLISTED_ROLES);
                List<string> blacklistedRoles;

                if(blacklistedRolesDb != null)
                    blacklistedRoles = (blacklistedRolesDb.roles as List<string> ?? new List<string>());
                else
                    blacklistedRoles = new List<string>();


                foreach (var user in guild.Users)   //for each user
                {
                    //run user-level stuff

                    //Getting the user status automatically updates the DB with new users that
                    //might have joined while server was off
                    var userStatus = await GetUserStatus(user);

                    //Sync roles with Local Priority, add missing roles to users
                    if ((userStatus.roles_to_add as List<SocketRole>).Count() > 0)
                    {

                        foreach (var role in (userStatus.roles_to_add as List<SocketRole>))
                        {
                            if (blacklistedRoles.Contains(role.Id.ToString()) || 
                                role.ToString().Contains("Nitro") ||
                                role.ToString().Contains("Booster"))
                                continue;

                            try
                            {
                                await user.AddRoleAsync(role);
                            }
                            catch (Exception ex)
                            {
                                LogMessage msg = new LogMessage(LogSeverity.Error, "UserDB", $"Could not add role {role.Name} for user {user.Username}", ex);
                                Globals.logger.OnLogAsync(msg).Start();
                            }
                        }
                    }

                    // for the init sync job, we only want to add the roles that were removed.
                    // consider the server's data move up to date for removed roles during server being off


                    //Sync roles with Local Priority, remove extra roles (When required for restoring all)
                    if (restoreAllFromDb && (userStatus.roles_to_remove as List<SocketRole>).Count() > 0)
                    {
                        foreach (var role in (userStatus.roles_to_remove as List<SocketRole>))
                        {
                            if (blacklistedRoles.Contains(role.Id.ToString()))
                                continue;

                            try
                            {
                                await user.RemoveRoleAsync(role);
                            }
                            catch (Exception ex)
                            {
                                LogMessage msg = new LogMessage(LogSeverity.Error, "UserDB", $"Could not remove role {role.Name} for user {user.Username}", ex);
                                Globals.logger.OnLogAsync(msg).Start();
                            }
                        }
                    }


                }
            }
        }
        internal static async Task RestoreRoles(SocketGuildUser user)
        {
            var server_id = user.Guild.Id.ToString();
            var blacklistedRolesDb = UserDB.db.GetDB(server_id, Globals.words.BLACKLISTED_ROLES);
            var blacklistedRoles = (blacklistedRolesDb?.roles as List<string>);

            if (blacklistedRoles == null)
                blacklistedRoles = new List<string>();

            var userStatus = await GetUserStatus(user);

            var local_roles = (userStatus.user_db_roles as List<string>);
            var roles_to_add = user.Guild.Roles.Where(role => local_roles.Contains(role.Id.ToString())).ToList();

            if ((userStatus.roles_to_add as List<SocketRole>).Count() > 0)
            {

                foreach (var role in roles_to_add)//.Where(role => role != user.Guild.EveryoneRole))
                {
                    if (blacklistedRoles.Contains(role.Id.ToString()))
                        continue;

                    try
                    {
                        await user.AddRoleAsync(role);
                    }
                    catch (Exception ex)
                    {
                        LogMessage msg = new LogMessage(LogSeverity.Error, "UserDB", $"Could not restore role {role.Name} for user {user.Username}", ex);
                        Globals.logger.OnLogAsync(msg).Start();
                    }
                }
            }

        }

        internal static async Task<bool> ReportSuspiciousness(SocketCommandContext context, SocketUserMessage msg)
        {
            bool actionTaken = false;

            bool suspicious = await EvaluateSuspiciousness(context, msg);

            if (suspicious)
            {
                var msgId = msg.Id;
                var channelObj = msg.Channel;

                await ReportSuspicious(context, msg, "Message contains suspicious words");

                actionTaken = true;
            }

            return actionTaken;
        }
        internal static async Task<bool> EvaluateSuspiciousness(SocketCommandContext context, SocketUserMessage msg)
        {
            var guild = context.Guild;
            var channel = context.Channel.Id.ToString();

            dynamic dbIgnoredChannels = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.IGNORED_CHANNELS);
            if (dbIgnoredChannels != null)
            {
                var currentIgnoredChannels = (dbIgnoredChannels.channels as IEnumerable<object>)?.Select(it => it.ToString()).ToList();
                if (currentIgnoredChannels != null)
                    if (currentIgnoredChannels.Contains(channel))
                        return false;
            }


            dynamic dbSuspiciousWords = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.SUSPICION_WORDS);

            if (dbSuspiciousWords != null)
            {
                string noCaseMessage = msg.Content.ToUpper();

                foreach (var word in dbSuspiciousWords.words)
                    if (noCaseMessage.Contains((string)word))
                        return true;

            }

            return false;
        }
        internal static async Task ReportSuspicious(SocketCommandContext context, IUserMessage msg, string reportMessage) => await ReportSuspicious(context.Guild, context.Channel, msg.Author, msg.Content, reportMessage);
        internal static async Task ReportSuspicious(SocketGuild guild, ISocketMessageChannel channel, IUser Author, string Content, string reportMessage)
        {

            var dbReportChannel = UserDB.db.GetDB(guild.Id.ToString(), Globals.words.REPORT_CHANNEL);
            if (dbReportChannel == null || dbReportChannel.channel == null)
                return;

            var guildUser = guild.Users.FirstOrDefault(iterator => iterator.Id == Author.Id);
            var isUserAdmin = guildUser.GuildPermissions.Administrator;
            var reportChannel = Globals.discord.GetChannel(Convert.ToUInt64(dbReportChannel.channel)) as IMessageChannel;

            List<string> mess = new List<string>();

            mess.AddRange(new string[]
            {
                reportMessage,
                "```"
            });

            mess.Add($"Timestamp: {DateTime.Now.ToString("MM/dd/yy H:mm:ss zzz")}");

            if(channel != null)
                mess.Add($"Channel: #{channel.Name}");

            mess.Add($"User: {guildUser.Username}#{guildUser.Discriminator}");

            if (!string.IsNullOrWhiteSpace(guildUser.Nickname))
                mess.Add($"Nickname: {guildUser.Nickname}");

            if (isUserAdmin)
                mess.Add($"Admnistrator User");

            if (!string.IsNullOrWhiteSpace(Content))
                mess.Add($"Message: {Content}");

            mess.Add("```");

            string messAll = string.Join("\n", mess);

            await reportChannel.SendMessageAsync(messAll);

        }
        internal static async Task WatchForQuickReconnect(SocketGuildUser user)
        {
            ulong userId = user.Id;

            //run this in another thread to not block the gateway thread

            (new Thread(() =>
            {
                // wait some time
                int minutes = 10;
                Thread.Sleep(1000 * 60 * minutes);

                var guild = user.Guild;
                var dl = guild.DownloadUsersAsync();
                dl.Wait();

                var userReturned = guild.Users.FirstOrDefault(iterator => iterator.Id == userId);
                if (userReturned != null)
                {
                    var rps = OwlBrain.ReportSuspicious(guild, null, user, null, $"User has left the server and rejoined within {minutes} minute{(minutes > 1 ? "s" : "")}. Ban/Role evade attempt?");
                    rps.Wait();
                }
            })).Start();
        }

    }
}
