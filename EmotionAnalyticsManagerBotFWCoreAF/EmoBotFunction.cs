using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmotionAnalyticsManagerCoreStandard;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace EmotionAnalyticsManagerBotFWCoreAF
{
    public static class EmoBotFunction
    {
        private static readonly BotFrameworkAdapter _botAdapter;
        private static readonly EmotionText _emotionTextService;
        private static readonly EmotionPicture _emotionPictureService;

        static EmoBotFunction()
        {
            var appId = Environment.GetEnvironmentVariable("MicrosoftAppId");
            var pwd = Environment.GetEnvironmentVariable("MicrosoftAppPassword");
            var credentialManager = new SimpleCredentialProvider(appId, pwd);
            _botAdapter = new BotFrameworkAdapter(credentialManager);

            var ibmEmotionUsername = Environment.GetEnvironmentVariable("IbmEmotionUsername");
            var ibmEmotionPassword = Environment.GetEnvironmentVariable("IbmEmotionPassword");
            _emotionTextService = new EmotionText(ibmEmotionUsername, ibmEmotionPassword);

            var azureCognitiveServicesKey = Environment.GetEnvironmentVariable("KeyMicrosoftEmotion");
            _emotionPictureService = new EmotionPicture(azureCognitiveServicesKey);
        }

        [FunctionName("EmoBot")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "messages")] HttpRequest req,
            CancellationToken token)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var activity = JsonConvert.DeserializeObject<Activity>(requestBody);
            try
            {
                await _botAdapter.ProcessActivityAsync(authHeader: req.Headers[@"Authorization"].FirstOrDefault(), activity, OnTurnAsync, token);

                return new OkResult();
            }
            catch (Exception ex)
            {
                var telemetryClient = new TelemetryClient();
                return new ObjectResult(ex) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        private static async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                try
                {
                    EmotionTextAsync(turnContext);
                }
                catch (Exception ex)
                {
                    var telemetryClient = new TelemetryClient();
                    telemetryClient.TrackException(ex);
                }
            }

            if (turnContext.Activity.Attachments != null)
            {
                try
                {
                    EmotionImageAsync(turnContext);
                }
                catch (Exception ex)
                {
                    var telemetryClient = new TelemetryClient();
                    telemetryClient.TrackException(ex);
                }
            }
        }

        private static async void EmotionTextAsync(ITurnContext turnContext)
        {
            var message = turnContext.Activity.Text ?? string.Empty;

            // clean string for Telegram client
            var emoString = "/emo@EmotionsAnalyticManagerbot";
            if (message.StartsWith(emoString))
            {
                message = message.Substring(emoString.Length);
            }

            var answer = _emotionTextService.AnalyseEmotionText(message);

            if(!string.IsNullOrWhiteSpace(answer))
                await turnContext.SendActivityAsync(answer);
        }

        private static async void EmotionImageAsync(ITurnContext turnContext)
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
