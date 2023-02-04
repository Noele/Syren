using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Syren.Syren.DataTypes;
using Syren.Syren.Events;
using Victoria;
using Spotify = Syren.Syren.DataTypes.Spotify;

namespace Syren.Syren
{
    public class Ship
    {
        
        private string _prefix;
        private string _token;
        private Spawn _spawn;
        private AudioHandler _audioHandler;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private LavaNode _lavaNode;
        private SyrenSpotifyClient _syrenSpotify;
        public async Task SetSail(string token, string prefix, string channelId, string spotifyToken)
        {
            _prefix = prefix;
            _token = token;

            var lavaConfig = new LavaConfig()
            {
                Authorization = "The Syren",
                Hostname = "localhost",
                Port = 2333,
                ReconnectAttempts = 100
            };

            var config = new DiscordSocketConfig // GatewayIntents = GatewayIntents.All
            {
                GatewayIntents =  GatewayIntents.All

            };
            _syrenSpotify = new SyrenSpotifyClient(spotifyToken);
            _syrenSpotify.Init();
            _client = new DiscordSocketClient(config); 
            _commands = new CommandService();
            _spawn = new Spawn(channelId);
            _lavaNode = new LavaNode(_client, lavaConfig);
            _services = new ServiceCollection()  
                .AddSingleton(_client)      
                .AddSingleton(_commands)     
                .AddSingleton(_lavaNode)
                .AddSingleton(_spawn)
                .AddSingleton(_syrenSpotify)
                .AddSingleton(lavaConfig)
                .BuildServiceProvider();
            
            _audioHandler = new AudioHandler(_lavaNode);
            _audioHandler.RegisterEvents();
            _client.Ready += OnReadyAsync;
            _client.Log += OnLog; // Signup to the Log event, this outputs almost all messages to the console

            await RegisterEvents(); // Register our events
            
            await RegisterCommandsAsync(); // Register our commands

            await _client.LoginAsync(TokenType.Bot, _token); // Login to discord

            await _client.StartAsync(); // Start the bot Asynchronously 

            await Task.Delay(-1); // Delay the end of the task for an infinite amount of time

        }
        
        private async Task OnReadyAsync() {
            if (!_lavaNode.IsConnected) {
                _lavaNode.ConnectAsync();
            }
        }
        
        private async Task RegisterEvents()
        {
            _client.MessageReceived += new Events.EventHandler(_spawn, _client).OnMessage;
        }


        private static Task OnLog(LogMessage message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }

        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
  
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message != null && message.Author.IsBot) return;

            var argPos = 0;
            if (message.HasStringPrefix(_prefix, ref argPos)) // If the message contains the prefix
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services); // Execute  the respective command
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason); // If the command failed, output the error reason
                if (result.Error.Equals(CommandError.UnmetPrecondition)) 
                    if (message != null) // If the message is not null (we have an error to send the user)
                        await message.Channel.SendMessageAsync(result.ErrorReason); // send the error reason
            }
        }
        
    }
}