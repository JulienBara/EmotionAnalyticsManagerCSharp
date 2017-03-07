using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace EmotionAnalyticsManagerTelegramBot
{
    class Program
    {
        private static readonly string keyTelegramPath = @"D:\OneDrive - TCDUD.onmicrosoft.com\ProjetsPerso\Ana\AnaKeys\cleAnaTest";
        private static readonly string keyTelegram = System.IO.File.ReadAllLines(keyTelegramPath)[0];
        private static readonly TelegramBotClient Bot = new TelegramBotClient(keyTelegram);

        static void Main(string[] args)
        {

            Bot.OnMessage += BotOnMessageReceived;

            //var me = Bot.GetMeAsync().Result;

            //Console.Title = me.Username;

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.TextMessage) return;

            if(message.Text.StartsWith("/emo"))
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                var wordsArray = message.Text.Split().Skip(1);
                var words = string.Join(" ", wordsArray);

                var ret = EmotionAnalyticsManagerCore.EmotionText.AnalyseEmotionText(words);

                await Bot.SendTextMessageAsync(message.Chat.Id, ret);
            }
        }
    }
}
