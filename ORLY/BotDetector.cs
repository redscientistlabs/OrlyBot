using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrlyBot
{
    class BotDetector
    {
        private static BotDetector _Instance = null;
        public static BotDetector Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new BotDetector();

                return _Instance;
            }
            set
            {
                _Instance = value;
            }
        }

        public BotDetector()
        {

        }

        public async Task<bool> ScanMessage(SocketCommandContext context, SocketUserMessage msg)
        {
            bool actionTaken = false;

            bool suspicious = await EvalTrust(context, msg);

            if (suspicious)
            {

                await Nuke(context, msg, $"Infected {msg.Author.Username}#{msg.Author.Discriminator} Detected. Removed.");

                actionTaken = true;
            }

            return actionTaken;
        }

        public async Task<bool> EvalTrust(SocketCommandContext context, SocketUserMessage msg)
        {
            bool actionTaken = false;

            SocketGuildUser user = context.Guild.Users.FirstOrDefault(it => it.Id == msg.Author.Id); ;//fetch guild user object
            var userStatus = await OwlBrain.GetUserStatus(user);

            List<msgTimestamp> timestamps = (userStatus.timestamps as List<msgTimestamp>);

            if (timestamps == null)
                timestamps = new List<msgTimestamp>();

            timestamps.Add(new msgTimestamp(msg.Id.ToString(), msg.Channel.Id.ToString()));

            if(timestamps.Count >= 4)
            {
                var firstTimestamp = timestamps.First();
                var lastTimeStamp = timestamps.Last();

                var seconds = (lastTimeStamp.Stamp - firstTimestamp.Stamp).TotalSeconds;

                if(seconds < 90) // if posted 4 messages in 90 seconds
                {
                    var channels = timestamps.Select(it => it.ChannelID);
                    bool allDifferentChannels = channels.Distinct().Count() == channels.Count();

                    if(allDifferentChannels)
                    {
                        actionTaken = true;
                        timestamps.Clear();
                    }

                }

                if(!actionTaken)
                {
                    timestamps.RemoveAt(0);
                }
            }

            //var guild = context.Guild;
            //var channel = context.Channel.Id.ToString();

            var server_id = context.Guild.Id.ToString();
            var user_id = user.Id.ToString();
            await UserDB.db.UpdateUserTimestamps(server_id, user_id, timestamps);

            //check for bot rules



            return actionTaken;
        }

        public async Task<bool> Nuke(SocketCommandContext context, SocketUserMessage msg, string text)
        {
            //nuke here
            var msgId = msg.Id;
            var channelObj = msg.Channel;

            await context.Guild.AddBanAsync(msg.Author, 1, "4 messages posted in 4 different channels under 45 seconds");
            await msg.Channel.SendMessageAsync(text);


            return true;
        }
    }
}
