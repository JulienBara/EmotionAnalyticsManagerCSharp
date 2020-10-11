﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EmotionAnalyticsManagerCoreStandard.Dtos;
using EmotionAnalyticsManagerCoreStandard.Helpers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;

namespace EmotionAnalyticsManagerCoreStandard
{
    public class EmotionText
    {
        private readonly string _ibmEmotionUsername;
        private readonly string _ibmEmotionPassword;

        public EmotionText(
            string ibmEmotionUsername,
            string ibmEmotionPassword)
        {
            _ibmEmotionUsername = ibmEmotionUsername;
            _ibmEmotionPassword = ibmEmotionPassword;
        }

        public string AnalyseEmotionText(string text)
        {
            var display = GetEmotionInEnglishText(text);
            return display;
        }

        private string GetEmotionInEnglishText(string englishText)
        {
            // todo inject http client
            // todo check last version
            var url = "https://gateway.watsonplatform.net";
            var client = new HttpClient { BaseAddress = new Uri(url) };
            var byteArray = Encoding.ASCII.GetBytes($"{_ibmEmotionUsername}:{_ibmEmotionPassword}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = new
            {
                text = englishText,
                features = new
                {
                    emotion = new { }
                }
            };

            var request = client.PostAsync(
                "/natural-language-understanding/api/v1/analyze?version=2018-11-16",
                new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"))
                .Result;

            if (!request.IsSuccessStatusCode)
            {
                var telemetryClient = new TelemetryClient();
                telemetryClient.TrackException(new ExceptionTelemetry());
                return "";
            }

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
