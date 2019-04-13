using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EmotionAnalyticManagerCoreStandard.Dtos;
using Newtonsoft.Json;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace EmotionAnalyticManagerCoreStandard
{
    public class EmotionPicture
    {
        private readonly string _azureCognitiveServicesKey;

        public EmotionPicture(string azureCognitiveServicesKey)
        {
            _azureCognitiveServicesKey = azureCognitiveServicesKey;
        }

        public string AnalyzeEmotionPicture(string imageUrl)
        {
            var emotionsTask = Task.Run(() => GetEmotionOfThePicture(imageUrl));
            var imageTask = Task.Run(() => DownloadPictureAsync(imageUrl));

            var emotions = emotionsTask.Result;
            var image = imageTask.Result;
            if (emotions.Count == 0) return null; // no faces
            var imageEmotions = DrawEmotion(emotions, image);
            var imageUrlAnswer = UrlifyImage(imageEmotions);
            return imageUrlAnswer;
        }

        private async Task<byte[]> DownloadPictureAsync(string imageUrl)
        {
            using (var client = new HttpClient())
            {

                using (var result = await client.GetAsync(imageUrl))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return await result.Content.ReadAsByteArrayAsync();
                    }

                    return null;
                }
            }
        }

        private byte[] DrawEmotion(List<MicrosoftEmotionAnswerFaceDto> emotions, byte[] imageBytes)
        {
            using (Image<Rgba32> image = Image.Load(imageBytes))
            {
                // todo
                // For production application we would recommend you create a FontCollection
                // singleton and manually install the ttf fonts yourself as using SystemFonts
                // can be expensive and you risk font existing or not existing on a deployment
                // by deployment basis.
                var font = SystemFonts.CreateFont("Arial", (float)image.Height / 20, FontStyle.Regular);

                foreach (var emotion in emotions)
                {
                    image.Mutate(x => x
                        .Draw(
                            Rgba32.Green,
                            image.Height / 100,
                            new Rectangle(
                                emotion.faceRectangle.left,
                                emotion.faceRectangle.top,
                                emotion.faceRectangle.width,
                                emotion.faceRectangle.height))
                        .DrawText(GetMaxEmotion(emotion), font, Rgba32.Green, PointF.Empty));
                }

                var imgAnswer = ImageToByteArray(image);
                return imgAnswer;
            }
        }

        private List<MicrosoftEmotionAnswerFaceDto> GetEmotionOfThePicture(string imageUrl)
        {
            // todo check if there is a library or inject http client
            var url = "https://westus.api.cognitive.microsoft.com";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _azureCognitiveServicesKey);

            var response = client.PostAsync(
                @"/face/v1.0/detect?returnFaceAttributes=emotion", 
                new StringContent(JsonConvert.SerializeObject(new { url = imageUrl })))
                .Result;

            if (response.StatusCode != HttpStatusCode.OK) return new List<MicrosoftEmotionAnswerFaceDto>();

            var emotions = JsonConvert.DeserializeObject<List<MicrosoftEmotionAnswerFaceDto>>(response.Content.ReadAsStringAsync().Result);

            return emotions;
        }

        private string UrlifyImage(byte[] image)
        {
            string url = "data:image/jpg;base64," + Convert.ToBase64String(image);
            return url;
        }

        private byte[] ImageToByteArray(Image<Rgba32> imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.SaveAsPng(ms);
                return ms.ToArray();
            }
        }

        private string GetMaxEmotion(MicrosoftEmotionAnswerFaceDto emotion)
        {
            var maxEmotions = emotion.faceAttributes.emotion.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            var emotionString = maxEmotions + " : " + string.Format("{0:0.00}", emotion.faceAttributes.emotion[maxEmotions]);

            return emotionString;
        }
    }
}

