using System.Collections.Generic;
using System.Net.Http;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;

namespace EmotionAnalyticManagerCoreStandard
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
            var url = "https://translate.yandex.net";
            var client = new HttpClient();
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

            var telemetryClient = new TelemetryClient();
            telemetryClient.TrackEvent("Translation Request", new Dictionary<string, string>
            {
                {"request text", text},
                {"response content", yandexAnswer.text[0]}
            });

            return yandexAnswer.text[0];
        }

        private static string GetEmotionInEnglishText(string englishText)
        {
            var url = "https://gateway.watsonplatform.net";
            var client = new RestClient(url);
            client.Authenticator = new HttpBasicAuthenticator(ibmEmotionUsername, ibmEmotionPassword);

            var request = new RestRequest("/natural-language-understanding/api/v1/analyze?version=2017-02-27",
                Method.POST);
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                text = englishText,
                features = new
                {
                    emotion = new { }
                }
            };
            request.AddJsonBody(body);

            IRestResponse response = client.Execute(request);

            var telemetryClient = new TelemetryClient();
            telemetryClient.TrackEvent("Emotion Request", new Dictionary<string, string>
            {
                {"request text", englishText},
                {"response content", response.Content}
            });

            var ibmAnswerDto = JsonConvert.DeserializeObject<IbmAnswerDto>(response.Content);

            var docEmotions = ibmAnswerDto.emotion.document.emotion;

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
