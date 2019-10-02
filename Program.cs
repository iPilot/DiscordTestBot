using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var botConfig = new BotConfig();
            await botConfig.LoadAsync();

            using (var client = new DiscordSocketClient())
            {
                client.Log += Log;
                await client.LoginAsync(TokenType.Bot, botConfig.Token);
                await client.StartAsync();

                var commands = new CommandService();
                
                await Task.Delay(-1);
            }
        }

        private static Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}