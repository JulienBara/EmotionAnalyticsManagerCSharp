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

                    var answer = EmotionText.AnalyseEmotionText(words);

                    // return our reply to the user
                    Activity reply = activity.CreateReply(answer);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

                if (activity.Attachments.Count > 0)
                {
                    foreach (var attachement in activity.Attachments)
                    {
                        EmotionPicture.AnalyseEmotionPicture(attachement.ContentUrl);
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