using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;

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
                var pen = new Pen(color, img.Height / 100);
                var fontFamily = new FontFamily("Arial");
                var font = new Font(fontFamily, img.Height / 20, FontStyle.Regular,GraphicsUnit.Pixel);
                var brush = new SolidBrush(color);
                foreach (var emotion in emotions)
                {
                    g.DrawRectangle(pen, emotion.faceRectangle.left, emotion.faceRectangle.top,
                        emotion.faceRectangle.width, emotion.faceRectangle.height);
                    g.DrawString(GetMaxEmotion(emotion), font, brush, emotion.faceRectangle.left, emotion.faceRectangle.top - img.Height / 15);
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

            if (response.StatusCode != HttpStatusCode.OK) return new List<MicrosoftEmotionAnswerFaceDto>(); 

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

        private static string GetMaxEmotion(MicrosoftEmotionAnswerFaceDto emotion)
        {
            // TODO : Change class MicrosoftEmotionAnswerFaceDto to have a Dictionnary of emotions 

            var emotions = new Dictionary<string, double>();
            emotions.Add("anger", emotion.scores.anger);
            emotions.Add("contempt", emotion.scores.contempt);
            emotions.Add("disgust", emotion.scores.disgust);
            emotions.Add("fear", emotion.scores.fear);
            emotions.Add("happiness", emotion.scores.happiness);
            emotions.Add("neutral", emotion.scores.neutral);
            emotions.Add("sadness", emotion.scores.sadness);
            emotions.Add("surprise", emotion.scores.surprise);

            var maxEmotions = emotions.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            var emotionString = maxEmotions + " : " + string.Format("{0:0.00}", emotions[maxEmotions]);

            return emotionString;
        }
    }
}

