using Newtonsoft.Json;
using RestSharp;
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
            var keyIbmEmotion = ConfigurationManager.AppSettings["KeyIbmWatsonTextToEmotion"];

            var url = "http://gateway-a.watsonplatform.net";
            var client = new RestClient(url);
            var request = new RestRequest("/calls/text/TextGetEmotion", Method.POST);
            request.AddParameter("apikey", keyIbmEmotion);
            request.AddParameter("text", englishText);
            request.AddParameter("outputMode", "json");

            IRestResponse response = client.Execute(request);
            var ibmAnswerDto = JsonConvert.DeserializeObject<IbmAnswerDto>(response.Content);
            var docEmotions = ibmAnswerDto.docEmotions;

            var sum = docEmotions.Sum(x => x.Value);

            var display = "";

            var translation = new Translation();

            foreach (var emotion in docEmotions)
            {
                display += translation.dictionary[emotion.Key] + " = " + string.Format("{0:0.00}", emotion.Value / sum) + "\n\n";
            }

            return display;
        }
    }
}
