using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
            var keyYandexPath = @".\keys\keyYandexTranslation";
            var keyYandexTranslation = System.IO.File.ReadAllLines(keyYandexPath)[0];

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
            var keyIbmPath = @".\keys\keyIbmWatsonTextToEmotion";
            var keyIbmEmotion = System.IO.File.ReadAllLines(keyIbmPath)[0];

            var url = "http://gateway-a.watsonplatform.net";
            var client = new RestClient(url);
            var request = new RestRequest("/calls/text/TextGetEmotion", Method.POST);
            request.AddParameter("apikey", keyIbmEmotion);
            request.AddParameter("text", englishText);
            request.AddParameter("outputMode", "json");

            IRestResponse response = client.Execute(request);
            var ibmAnswerDto = JsonConvert.DeserializeObject<IbmAnswerDto>(response.Content);
            var docEmotions = ibmAnswerDto.docEmotions;

            var sum = docEmotions.anger
                + docEmotions.disgust
                + docEmotions.fear
                + docEmotions.joy
                + docEmotions.sadness;

            var display = "Colère = " + docEmotions.anger / sum + "\n"
                + "Dégout = " + docEmotions.disgust / sum + "\n"
                + "Peur = " + docEmotions.fear / sum + "\n"
                + "Joie = " + docEmotions.joy / sum + "\n"
                + "Tristesse = " + docEmotions.sadness / sum + "\n";

            return display;
        }
    }
}
