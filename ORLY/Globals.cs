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
using System.Threading.Tasks;

namespace OrlyBot
{
    static class Globals
    {
        internal static DirectoryInfo baseDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        internal static DiscordSocketClient discord = null;
        internal static LoggingService logger = null;

        public static class words
        {
            public static string BLACKLISTED_ROLES = nameof(BLACKLISTED_ROLES);
            public static string REPORT_CHANNEL = nameof(REPORT_CHANNEL);
            public static string SUSPICION_WORDS = nameof(SUSPICION_WORDS);
            public static string IGNORED_CHANNELS = nameof(IGNORED_CHANNELS);
        }

    }
}
