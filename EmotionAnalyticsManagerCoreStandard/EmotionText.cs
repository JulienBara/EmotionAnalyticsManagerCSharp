using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EmotionAnalyticsManagerCoreStandard.Dtos;
using EmotionAnalyticsManagerCoreStandard.Helpers;
using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.NaturalLanguageUnderstanding.v1;
using IBM.Watson.NaturalLanguageUnderstanding.v1.Model;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;

namespace EmotionAnalyticsManagerCoreStandard
{
    public class EmotionText
    {
        private readonly string _microsoftTranslatorKey;
        private readonly string _microsoftTranslatorRegion;
        private readonly string _ibmApiKey;
        private readonly string _ibmUrl;

        public EmotionText(
            string microsoftTranslatorKey,
            string microsoftTranslatorRegion,
            string ibmApiKey,
            string ibmUrl)
        {
            _microsoftTranslatorKey = microsoftTranslatorKey;
            _microsoftTranslatorRegion = microsoftTranslatorRegion;
            _ibmApiKey = ibmApiKey;
            _ibmUrl = ibmUrl;
        }

        public string AnalyseEmotionText(string text)
        {
            var englishText = Task.Run(() => TranslateText(text)).Result;
            if (string.IsNullOrWhiteSpace(englishText)) { return ""; }

            var display = GetEmotionInEnglishText(englishText);
            return display;
        }

        // inspired by https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-translator?tabs=csharp
        public async Task<string> TranslateText(string text)
        {
            var endpoint = "https://api.cognitive.microsofttranslator.com/";

            // Input and output languages are defined as parameters.
            string route = "translate?api-version=3.0&to=en";
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _microsoftTranslatorKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", _microsoftTranslatorRegion);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var telemetryClient = new TelemetryClient();
                    telemetryClient.TrackException(new ExceptionTelemetry());
                    return "";
                }

                // Read response as a string.
                string result = await response.Content.ReadAsStringAsync();

                var emotions = JsonConvert.DeserializeObject<List<MicrosoftTranslatorAnswerDto>>(result).First();
                return emotions.Translations.FirstOrDefault(x => x.To == "en").Text;
            }
        }

        private string GetEmotionInEnglishText(string englishText)
        {
            IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{_ibmApiKey}");

            NaturalLanguageUnderstandingService naturalLanguageUnderstanding = new NaturalLanguageUnderstandingService("2020-08-01", authenticator);
            naturalLanguageUnderstanding.SetServiceUrl($"{_ibmUrl}");

            var request = naturalLanguageUnderstanding.Analyze(
                html: englishText,
                features: new Features()
                {
                    Emotion = new EmotionOptions()
                }
            );

            var response = request.Result;
            var emotionsResponse = request.Result.Emotion.Document.Emotion;

            var docEmotions = emotionsResponse
                .GetType()
                .GetProperties()
                .ToDictionary(x => x.Name, x => (double)x.GetValue(emotionsResponse));

            var sum = docEmotions.Sum(x => x.Value);

            var displayList = new List<string>
            {
                string.Format("{0} | {1}", Translation.Dictionary["Emotion"], Translation.Dictionary["Value"]),
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
