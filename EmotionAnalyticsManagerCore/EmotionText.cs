using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;

namespace EmotionAnalyticsManagerCore
{
    public static class EmotionText
    {
        public static string AnalyseEmotionText(string text)
        {
            var text_english = TranslateToEnglish(text);
            var display = GetEmotionInEnglishText(text_english);
            return display;
        }

        private static string TranslateToEnglish(string text)
        {
            var keyYandexTranslation = ConfigurationManager.AppSettings["KeyYandexTranslation"];

            var url = "https://translate.yandex.net";
            var client = new RestClient(url);
            var request = new RestRequest("/api/v1.5/tr.json/translate", Method.POST);
            request.AddParameter("key", keyYandexTranslation);
            request.AddParameter("lang", "en");
            request.AddParameter("text", text);

            IRestResponse response = client.Execute(request);
            var yandexAnswerDto = JsonConvert.DeserializeObject<YandexAnswerDto>(response.Content);

            return yandexAnswerDto.text[0];
        }

        private static string GetEmotionInEnglishText(string englishText)
        {
            // var keyIbmEmotion = ConfigurationManager.AppSettings["KeyIbmWatsonTextToEmotion"];
            var ibmEmotionUsername = ConfigurationManager.AppSettings["IbmEmotionUsername"];
            var ibmEmotionPassword = ConfigurationManager.AppSettings["IbmEmotionPassword"];

            // var url = "http://gateway-a.watsonplatform.net";
            // var client = new RestClient(url);
            // var request = new RestRequest("/calls/text/TextGetEmotion", Method.POST);
            // request.AddParameter("apikey", keyIbmEmotion);
            // request.AddParameter("text", englishText);
            // request.AddParameter("outputMode", "json");
            var url = "https://gateway.watsonplatform.net";
            var client = new RestClient(url);
            client.Authenticator = new SimpleAuthenticator("username", ibmEmotionUsername, "password", ibmEmotionPassword);
            
            var request = new RestRequest("/natural-language-understanding/api/v1/analyze?version=2017-02-27", Method.POST);
            request.AddParameter("Content-Type", "application/json");

            IRestResponse response = client.Execute(request);
            var ibmAnswerDto = JsonConvert.DeserializeObject<IbmAnswerDto>(response.Content);
            var docEmotions = ibmAnswerDto.docEmotions;

            var sum = docEmotions.Sum(x => x.Value);

            var translation = new Translation();

            var displayList = new List<string>();

            displayList.Add(string.Format("{0} | {1}", translation.dictionary["emotion"],
                translation.dictionary["value"]));
            displayList.Add("-|-");

            foreach (var emotion in docEmotions)
            {
                var emotionTranslated = translation.dictionary[emotion.Key];
                var emotionValue = emotion.Value / sum;
                displayList.Add(string.Format("{0} | {1,5:N2}", emotionTranslated, emotionValue));
            }

            var display = string.Join("\n", displayList);

            return display;
        }
    }
}
