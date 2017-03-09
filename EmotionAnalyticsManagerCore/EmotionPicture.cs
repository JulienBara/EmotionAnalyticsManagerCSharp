using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;

namespace EmotionAnalyticsManagerCore
{
    public class EmotionPicture
    {
        public static string AnalyseEmotionPicture(string imageUrl)
        {
            var emotions = GetEmotionOfThePicture(imageUrl);
            if (emotions.Count == 0) return null; // no faces
            var image = DownloadPicture(imageUrl);
            var imageEmotions = DrawEmotion(emotions, image);
            var imageUrlAnswer = UrlifyImage(imageEmotions);
            return imageUrlAnswer;
        }

        private static byte[] DownloadPicture(string imageUrl)
        {
            var client = new RestClient(imageUrl);
            var request = new RestRequest();
            var image = client.DownloadData(request);
            return image;
        }

        private static byte[] DrawEmotion(List<MicrosoftEmotionAnswerFaceDto> emotions, byte[] image)
        {
            var img = ByteArrayToImage(image);
            using (var g = Graphics.FromImage(img))
            {
                var color = Color.Green;
                var pen = new Pen(color);
                foreach (var emotion in emotions)
                {
                    g.DrawRectangle(pen, emotion.faceRectangle.left, emotion.faceRectangle.top,
                        emotion.faceRectangle.width, emotion.faceRectangle.height);
                }
            }
            var imgAnswer = ImageToByteArray(img);
            return imgAnswer;
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

        private static byte[] ImageToByteArray(Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }

        private static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }
        }
    }
}

