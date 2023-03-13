using System.Runtime.InteropServices;
using System.Text;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;

namespace Syren.Syren.PrefixCommands;

public class Pokemon : ModuleBase<SocketCommandContext>
{
    [Command("pokedex")]
    public async Task Pokedex([Remainder, Optional] string pageQuery)
    {
        pageQuery = string.IsNullOrWhiteSpace(pageQuery) ? "1" : pageQuery;
        var text = await File.ReadAllTextAsync("Data/Pokemon/trainers.json");
        var pokemonJson = JsonConvert.DeserializeObject<TrainerJson.TrainerJsonRoot>(text);

        foreach (var trainer in pokemonJson.Trainers.Where(trainer => trainer.UserId == Context.Message.Author.Id.ToString()))
        {
            var totalPokemon = trainer.Info.CapturedPokemon.Count;
            var (page, pageCount, pagenumber) = Toolbox.CreatePageFromList(trainer.Info.CapturedPokemon, pageQuery, true, 250, false);

            await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
            {
                Description = page,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Page {pagenumber}/{pageCount} | Total Pokemon: {totalPokemon}"
                }
            }.Build());
            return;
        }

        await ReplyAsync("You do not have any pokemon.");
    }
}