using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using EmotionAnalyticsManagerCore;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;

namespace EmotionAnalyticsManagerBotFW
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var message = activity.Text ?? string.Empty;

                if (message.StartsWith("/emo"))
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                    var wordsArray = message.Split().Skip(1);
                    var words = string.Join(" ", wordsArray);

                    if (words != "")
                    {
                        var answer = "";
                        //try
                        //{
                            answer = EmotionText.AnalyseEmotionText(words);
                        //}
                        //catch (Exception ex)
                        //{
                        //    var telemetryClient = new TelemetryClient();
                        //    telemetryClient.TrackException(ex);
                        //}

                        // return our reply to the user
                        Activity reply = activity.CreateReply(answer);
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }

                if (activity.Attachments != null)
                {
                    foreach (var attachement in activity.Attachments)
                    {
                        // Telegram seems to convert most of pictures to JPEG
                        // Microsoft Api 13/03/2017:
                        // "The supported input image formats includes JPEG, PNG, GIF(the first frame), BMP. Image file size should be no larger than 4MB."
                        if ((attachement.ContentType == "image/jpeg" || attachement.ContentType == "image/png" ||
                            attachement.ContentType == "image/gif" || attachement.ContentType == "image/bmp")
                            && attachement.Name != null && attachement.Name.StartsWith("/emo"))
                        {
                            var imageUrl = EmotionPicture.AnalyseEmotionPicture(attachement.ContentUrl);
                            if (imageUrl != null)
                            {
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                                var answer = activity.CreateReply();
                                answer.Attachments = new List<Attachment>();
                                answer.Attachments.Add(new Attachment
                                {
                                    ContentUrl = imageUrl,
                                    ContentType = "image/jpeg",
                                    Name = " "
                                });

                                try
                                {
                                    await connector.Conversations.ReplyToActivityAsync(answer);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}