using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EmotionAnalyticsManagerCoreStandard.Dtos;
using EmotionAnalyticsManagerCoreStandard.Helpers;
using Newtonsoft.Json;

namespace EmotionAnalyticsManagerCoreStandard
{
    public class EmotionText
    {
        private readonly string _ibmEmotionUsername;
        private readonly string _ibmEmotionPassword;
        private readonly string _yandexTranslationKey;

        public EmotionText(
            string ibmEmotionUsername,
            string ibmEmotionPassword,
            string yandexTranslationKey)
        {
            _ibmEmotionUsername = ibmEmotionUsername;
            _ibmEmotionPassword = ibmEmotionPassword;
            _yandexTranslationKey = yandexTranslationKey;
        }

        public string AnalyseEmotionText(string text)
        {
            var textEnglish = TranslateToEnglish(text);
            var display = GetEmotionInEnglishText(textEnglish);
            return display;
        }

        private string TranslateToEnglish(string text)
        {
            // todo inject http client
            // todo check last version
            var url = "https://translate.yandex.net";
            var client = new HttpClient { BaseAddress = new Uri(url) };
            var response = client.PostAsync(
                "/api/v1.5/tr.json/translate",
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("key", _yandexTranslationKey),
                    new KeyValuePair<string, string>("lang", "en"),
                    new KeyValuePair<string, string>("text", text)
                }))
                .Result;

            var yandexAnswer = JsonConvert.DeserializeObject<YandexAnswerDto>(response.Content.ReadAsStringAsync().Result);

            return yandexAnswer.text[0];
        }

        private string GetEmotionInEnglishText(string englishText)
        {
            // todo inject http client
            // todo check last version
            var url = "https://gateway.watsonplatform.net";
            var client = new HttpClient { BaseAddress = new Uri(url) };
            var byteArray = Encoding.ASCII.GetBytes($"{_ibmEmotionUsername}:{_ibmEmotionPassword}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");

            var body = new
            {
                text = englishText,
                features = new
                {
                    emotion = new { }
                }
            };

            var request = client.PostAsync(
                "/natural-language-understanding/api/v1/analyze?version=2017-02-27",
                new StringContent(JsonConvert.SerializeObject(body)))
                .Result;

            var response = request.Content.ReadAsStringAsync().Result;

            var ibmAnswerDto = JsonConvert.DeserializeObject<IbmAnswerDto>(response);

            var docEmotions = ibmAnswerDto.Emotion.Document.Emotion;

            var sum = docEmotions.Sum(x => x.Value);

            var displayList = new List<string>
            {
                string.Format("{0} | {1}", Translation.Dictionary["emotion"], Translation.Dictionary["value"]),
                "-|-"
            };


            foreach (var emotion in docEmotions)
            {
                var emotionTranslated = Translation.Dictionary[emotion.Key];
                var emotionValue = emotion.Value / sum;
                displayList.Add(string.Format("{0} | {1,5:N2}", emotionTranslated, emotionValue));
            }

            var display = string.Join("\n", displayList);

            return display;
        }
    }
}
