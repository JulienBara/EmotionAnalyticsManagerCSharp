using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionAnalyticsManagerCore
{
    public class EmotionPicture
    {
        public static string AnalyseEmotionPicture(string imageUrl)
        {
            //var image_local_path = DownloadPicture(imageUrl);
            var emotions = GetEmotionOfThePicture(imageUrl);
            if (true) return null; // no faces
            //DrawEmotion(emotions, image_local_path);
            //return image_local_path;
        }

        private static List<MicrosoftEmotionAnswerFaceDto> GetEmotionOfThePicture(string imageUrl)
        {
            var keyMicrosoftEmotion = ConfigurationManager.AppSettings["KeyMicrosoftEmotion"];

            var url = "https://westus.api.cognitive.microsoft.com";
            var client = new RestClient(url);
            var request = new RestRequest("/emotion/v1.0/recognize", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Ocp-Apim-Subscription-Key", keyMicrosoftEmotion);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { url = imageUrl });

            IRestResponse response = client.Execute(request);
            var emotions = JsonConvert.DeserializeObject<List<MicrosoftEmotionAnswerFaceDto>>(response.Content);

            return emotions;
        }
    }
}

