using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions;
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
            var emotions = GetEmotionOfThePicture(imageUrl);
            if (emotions.Count == 0) return null; // no faces
            var image = DownloadPicture(imageUrl);
            //DrawEmotion(emotions, image_local_path);
            var imageUrlAnswer = UrlifyImage(image);
            return imageUrlAnswer;
        }

        private static byte[] DownloadPicture(string imageUrl)
        {
            var client = new RestClient(imageUrl);
            var request = new RestRequest();
            var image = client.DownloadData(request);
            return image;
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

        private static string UrlifyImage(byte[] image)
        {
            string url = "data:image/png;base64," + Convert.ToBase64String(image);
            return url;
        }
    }
}

