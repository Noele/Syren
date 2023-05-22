using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Microsoft.Extensions.DependencyInjection;
using Syren.Syren.DataTypes;
using Syren.Syren.Events;

namespace Syren.Syren
{
    public class Ship
    {
        private string _prefix;
        private string _token;
        private PokemonSpawn _pokemonSpawn;
        private LeagueOfLegendsSpawn _leagueOfLegendsSpawn;
        private ApiKeys _apiKeys;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private SyrenSpotifyClient _syrenSpotify;
        private InteractionService _interactionService;
        public async Task SetSail(string token, string prefix, string pokemonChannelID, string leagueoflegendsChannelID,  string spotifyClientId, string spotifyClientSecret, string aiApiKey, string fortniteApiKey)
        {
            _prefix = prefix;
            _token = token;
            _apiKeys = new ApiKeys() { aiApiKey = aiApiKey, spotifyClientId = spotifyClientId, spotifyClientSecret = spotifyClientSecret, fortniteApiKey = fortniteApiKey };
           
            var config = new DiscordSocketConfig
            {
                GatewayIntents =  GatewayIntents.All
                
            };
            _syrenSpotify = new SyrenSpotifyClient(_apiKeys);
            _client = new DiscordSocketClient(config); 
            _commands = new CommandService();
            _pokemonSpawn = new PokemonSpawn(pokemonChannelID);
            _leagueOfLegendsSpawn = new LeagueOfLegendsSpawn(leagueoflegendsChannelID);
            _interactionService = new InteractionService(_client, new InteractionServiceConfig());
            
            var lavalinkNodeOptions = new LavalinkNodeOptions
            {
                RestUri = "http://localhost:8080/",
                WebSocketUri = "ws://localhost:8080/",
                Password = "youshallnotpass"
            };

            _services = new ServiceCollection()  
                .AddSingleton(_client)      
                .AddSingleton<IAudioService, LavalinkNode>()
                .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()
                .AddSingleton(lavalinkNodeOptions)
                .AddSingleton(_commands)
                .AddSingleton(_interactionService)
                .AddSingleton(_apiKeys)
                .AddSingleton(_pokemonSpawn)
                .AddSingleton(_leagueOfLegendsSpawn)
                .AddSingleton(_syrenSpotify)
                .BuildServiceProvider();
            
            _client.Ready += OnReadyAsync;
            _client.Log += OnLog; // Signup to the Log event, this outputs almost all messages to the console

            await RegisterEvents(); // Register our events
            
            await RegisterCommandsAsync(); // Register our prefix commands

            await RegisterInteractions(); // Register our slash commands

            await _client.LoginAsync(TokenType.Bot, _token); // Login to discord

            await _client.StartAsync(); // Start the bot Asynchronously 

            await Task.Delay(-1); // Delay the end of the task for an infinite amount of time

        }
        
        private async Task RegisterInteractions()
        {
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.InteractionCreated += HandleInteraction;
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
           {
               var ctx = new SocketInteractionContext(_client, arg);
               await _interactionService.ExecuteCommandAsync(ctx, _services);
           }
           catch(Exception e) { Console.WriteLine(e); }
        }

        private async Task OnReadyAsync() {
            var audioService = _services.GetRequiredService<IAudioService>();
            await audioService.InitializeAsync();
            await _interactionService.RegisterCommandsGloballyAsync();
        }
        
        private async Task RegisterEvents()
        {
            _client.MessageReceived += new Events.EventHandler(_pokemonSpawn, _client, _leagueOfLegendsSpawn, _apiKeys).OnMessage;
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

            int argPos = 0;

            var result = await _commands.ExecuteAsync(context, argPos, _services, MultiMatchHandling.Best);
            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.UnmetPrecondition)
                {
                    if (message != null)
                        await message.Channel.SendMessageAsync(result.ErrorReason);
                }
                else
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }

    }
}