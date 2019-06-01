using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmotionAnalyticsManagerCoreStandard;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace EmotionAnalyticsManagerBotFWCore
{
    public class EmotionAnalyticsManagerBotFWCoreBot : IBot
    {
        private readonly ILogger _logger;
        private readonly EmotionPicture _emotionPictureService;
        private readonly EmotionText _emotionTextService;

        public EmotionAnalyticsManagerBotFWCoreBot(
            ILoggerFactory loggerFactory,
            EmotionPicture emotionPicture,
            EmotionText emotionText)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<EmotionAnalyticsManagerBotFWCoreBot>();
            _emotionPictureService = emotionPicture ?? throw new System.ArgumentNullException(nameof(emotionPicture));
            _emotionTextService = emotionText ?? throw new System.ArgumentNullException(nameof(emotionText));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                _ = Task.Run(() => EmotionTextAsync(turnContext));
            }

            if (turnContext.Activity.Attachments != null)
            {
                _ = Task.Run(() => EmotionImageAsync(turnContext));
            }

            // acknowledge answer fast to prevent timeout / to make timeout less likely
            await turnContext.SendActivityAsync("", cancellationToken: cancellationToken); 
        }

        private async void EmotionTextAsync(ITurnContext turnContext)
        {
            var message = turnContext.Activity.Text ?? string.Empty;

            // clean string for Telegram client
            var emoString = "/emo@EmotionsAnalyticManagerbot";
            if (message.StartsWith(emoString))
            {
                message = message.Substring(emoString.Length);
            }

            try
            {
                var answer = _emotionTextService.AnalyseEmotionText(message);
                await turnContext.SendActivityAsync(answer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while analyzing text");
            }
        }

        private async void EmotionImageAsync(ITurnContext turnContext)
        {
            foreach (var attachment in turnContext.Activity.Attachments)
            {
                // Telegram seems to convert most of pictures to JPEG
                // Microsoft Api 13/03/2017:
                // "The supported input image formats includes JPEG, PNG, GIF(the first frame), BMP. Image file size should be no larger than 4MB."
                if (attachment.ContentType == "image/jpeg" || attachment.ContentType == "image/png" ||
                    attachment.ContentType == "image/gif" || attachment.ContentType == "image/bmp")
                {
                    var imageUrl = _emotionPictureService.AnalyzeEmotionPicture(attachment.ContentUrl);
                    if (imageUrl != null)
                    {
                        var answer = turnContext.Activity.CreateReply();
                        answer.Attachments = new List<Attachment>
                            {
                                new Attachment {ContentUrl = imageUrl, ContentType = "image/jpeg", Name = " "}
                            };


                        await turnContext.SendActivityAsync(answer);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("No face found.");
                    }
                }
            }
        }
    }
}
