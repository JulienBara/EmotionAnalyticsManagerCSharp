using System.Collections.Generic;
using System.Linq;
using EmotionAnalyticsManagerCoreStandard.Helpers;
using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.NaturalLanguageUnderstanding.v1;
using IBM.Watson.NaturalLanguageUnderstanding.v1.Model;

namespace EmotionAnalyticsManagerCoreStandard
{
    public class EmotionText
    {
        private readonly string _ibmApiKey;
        private readonly string _ibmUrl;

        public EmotionText(
            string ibmApiKey,
            string ibmUrl)
        {
            _ibmApiKey = ibmApiKey;
            _ibmUrl = ibmUrl;
        }

        public string AnalyseEmotionText(string text)
        {
            var display = GetEmotionInEnglishText(text);
            return display;
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
