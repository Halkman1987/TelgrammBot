using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;

namespace Telegram.Bots
{
    class Program
    {
        public static class BotCredentials // Содержит Токен Бота
        {
            public static readonly string BotToken = "5275938900:AAGSYvfMIJYdrcYym_9SVxEEpfqI8sT5CMw";

        }
        //static ITelegramBotClient botClient;
        static void Main(string[] args)
        {
          var bot  = new BotWorker();  
          bot.Initialize();
            bot.Start();

            string command;
            do
            {
                command = Console.ReadLine();

            } while (command != "stop");

            bot.Stop();

            //var me = botClient.GetMeAsync().Result;     Для проверки работы !!!!
            //Console.WriteLine($" Hello my name is {me.FirstName}");  НЕ НУЖНО !!!!

            //botClient.OnMessage += Bot_OnMessage;
            //botClient.StartReceiving();
            //Console.WriteLine("Нажмите любую кнопку для остановки");
            //Console.ReadKey();
            //botClient.StopReceiving();

        }


        public class BotWorker
        {
            private BotMessageLogic logic;
            private ITelegramBotClient botClient;
            CancellationToken cts = new CancellationToken();
            ReceiverOptions receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };
            public void Initialize()
            {
                botClient = new TelegramBotClient(BotCredentials.BotToken);
                logic = new BotMessageLogic(botClient);
            }
            public async void Start()
            {
                botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions,cancellationToken: cts);
                //botClient.GetUpdatesAsync().Wait(); 
            }
            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
               if (update.Type != UpdateType.Message)
                    return;
                // Only process text messages
                if (update.Message!.Type != MessageType.Text)
                    return;

                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;

                Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

                // Echo received message text
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "You said:\n" + messageText,
                    cancellationToken: cancellationToken);
            }
            Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                      => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
            public void Stop()
            {
               cts.ThrowIfCancellationRequested();
            }
            private async void Bot_OnMessage(object sender, Update e)
            {
                if (e.Message.Text != null)
                {
                    await logic.Response(e);
                }
            }
        }
        public class BotMessageLogic // Управление логикой добавления чата
        {
            private ITelegramBotClient botClient;
            private Messanger messanger;
            private Dictionary<long, Conversation> chatList;

           
            public BotMessageLogic(ITelegramBotClient botClient)
            {
                messanger = new Messanger();
                chatList = new Dictionary<long, Conversation>();
                this.botClient = botClient;
            }
            public async Task Response(Update e)
            {
                var Id = e.Id;
                if (!chatList.ContainsKey(Id))
                {
                    var newchat = new Conversation(e.Message.Chat);
                    chatList.Add(Id, newchat);
                }
                var chatt = chatList[Id];
                chatt.AddMessage(e.Message);
                await SendTextMessage(chatt);
            }
            private async Task SendTextMessage(Conversation chat)
            {
                var text = messanger.CreateTextMessage(chat);
                await botClient.SendTextMessageAsync(
                chatId: chat.GetId(), text: text);
            }
        }
        public class Conversation  //Для хранения чатов ( Объект ЧАТА )
        {
            private Chat telegramChat;
            private List<Message> telegramMessages;
            public Conversation(Chat chat)
            {
                telegramChat = chat;
                telegramMessages = new List<Message>();
            }
            public void AddMessage(Message message)
            {
                telegramMessages.Add(message);
            }
            public long GetId()
            {
                var m = telegramChat.Id;
                return m;
            }
            public List<string> GetTextMessages() /// Возврат всех сообщений 
            {
                var textMessages = new List<string>();
                foreach (var message in telegramMessages)
                {
                    if (message.Text != null)
                    {
                        textMessages.Add(message.Text);
                    }
                }
                return textMessages;
            }
            
        }
        public class Messanger
        {
            public string CreateTextMessage(Conversation chat)
            {
                var delimiter = ",";
                var text = string.Join(delimiter, chat.GetTextMessages().ToArray());
                return text;
            }

        }
    }
    
}