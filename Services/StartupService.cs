using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OrlyBot
{
    public class StartupService
    {
        private readonly IServiceProvider _provider;
        public static DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            IServiceProvider provider,
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config)
        {
            _provider = provider;
            _config = config;
            _discord = discord;
            _commands = commands;
        }

        public async Task StartAsync()
        {
            string discordToken = _config["tokens:discord"];     // Get the discord token from the config file
            if (string.IsNullOrWhiteSpace(discordToken) || discordToken == "PUT_TOKEN_HERE")
            {
                Console.WriteLine("");
                Console.WriteLine("Error: You have not set a discord token for the bot.");
                Console.WriteLine("");
                Console.WriteLine("Please open the config.yml file and change the PUT_TOKEN_HERE");
                Console.WriteLine(" for the token to be used by the bot.");
                Console.WriteLine("");
                Console.WriteLine("The token will look like this:");
                Console.WriteLine("OTISPDE0NDMpNzY4OTXcFLQoE2.X3vIamyMDA.J2oFCPOmG8nODUyOuwJOc");
                Console.WriteLine(" (this is not a real key btw)");
                Console.WriteLine("");
                Console.WriteLine("Press any key to leave the program");

                Console.ReadKey();
                Environment.Exit(0);
            }

            await _discord.LoginAsync(TokenType.Bot, discordToken);     // Login to discord
            await _discord.StartAsync();                                // Connect to the websocket

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);     // Load commands and modules into the command service
        }
    }
}
