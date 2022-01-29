using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace Telegram.Bots
{
    class Program
    {
        static ITelegramBotClient botClient;
        static void Main(string[] args)
        {
            botClient = new TelegramBotClient(BotCredentials.BotToken);
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Console.WriteLine("Нажмите любую кнопку для остановки");
            Console.ReadKey();
            botClient.StopReceiving();
            //var me = botClient.GetMeAsync().Result;
            // Console.WriteLine($" Hello my name is {me.FirstName}");

            static async void Bot_OnMessage(object sender, MessageEventArgs e)
            {
                if(e.Message.Text != null)
                {
                    Console.WriteLine($"Полученно сообщение в чате : {e.Message.Chat.Id}");
                    await botClient.SendTextMessageAsync(
                   chatId: e.Message.Chat, text: "Вы написали :\n" + e.Message.Text);
                }
            }

        }

        public static class BotCredentials
        {
            public static readonly string BotToken = "5275938900:AAGSYvfMIJYdrcYym_9SVxEEpfqI8sT5CMw";

        }

    }
    
}