using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using PochinkiBot.Repositories.Interfaces;

namespace PochinkiBot.Client.Commands.ExperimentalCommands
{
    [Command("я родился")]
    public class BirthDayCommand : IBotCommand
    {
        private readonly IBirthDayStore _birthDayStore;

        public BirthDayCommand(IBirthDayStore birthDayStore)
        {
            _birthDayStore = birthDayStore;
        }

        public async Task Execute(SocketUserMessage userMessage, int argsPos)
        {
            var args = userMessage.Content.Substring(argsPos).Trim();
            if (DateTime.TryParse(args, out var date))
            {
                var birthDate = await _birthDayStore.GetBirthDate(userMessage.Author.Id);
                if (!birthDate.HasValue)
                {
                    await _birthDayStore.SaveBirthDate(userMessage.Author.Id, date);
                    await userMessage.Channel.SendMessageAsync("Записал.");
                }
            }

            await userMessage.DeleteAsync();
        }
    }
}