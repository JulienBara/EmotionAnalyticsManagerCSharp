using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace EmotionAnalyticsManagerCoreStandard
{
    public class EmotionPicture
    {
        private readonly string _azureCognitiveServicesEndpoint;
        private readonly string _azureCognitiveServicesKey;

        public EmotionPicture(
            string azureCognitiveServicesEndpoint,
            string azureCognitiveServicesKey)
        {
            _azureCognitiveServicesEndpoint = azureCognitiveServicesEndpoint;
            _azureCognitiveServicesKey = azureCognitiveServicesKey;
        }

        public string AnalyzeEmotionPicture(string imageUrl)
        {
            var detectedFacesTask = Task.Run(() => GetEmotionOfThePicture(imageUrl));
            var imageTask = Task.Run(() => DownloadPictureAsync(imageUrl));

            var detectedFaces = detectedFacesTask.Result;
            var image = imageTask.Result;
            if (detectedFaces.Count == 0) return null; // no faces
            var imageEmotions = DrawEmotion(detectedFaces, image);
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

        private byte[] DrawEmotion(IList<DetectedFace> detectedFaces, byte[] imageBytes)
        {
            using (Image<Rgba32> image = Image.Load(imageBytes))
            {
                // todo
                // For production application we would recommend you create a FontCollection
                // singleton and manually install the ttf fonts yourself as using SystemFonts
                // can be expensive and you risk font existing or not existing on a deployment
                // by deployment basis.
                var font = SystemFonts.CreateFont("Arial", (float)image.Height / 20, FontStyle.Regular);

                foreach (var face in detectedFaces)
                {
                    image.Mutate(x => x
                        .Draw(
                            Rgba32.Green,
                            image.Height / 100,
                            new Rectangle(
                                face.FaceRectangle.Left,
                                face.FaceRectangle.Top,
                                face.FaceRectangle.Width,
                                face.FaceRectangle.Height))
                        .DrawText(GetMaxEmotion(face), font, Rgba32.Green, PointF.Empty));
                }

                var imgAnswer = ImageToByteArray(image);
                return imgAnswer;
            }
        }

        private async Task<IList<DetectedFace>> GetEmotionOfThePicture(string imageUrl)
        {
            var client = new FaceClient(new ApiKeyServiceClientCredentials(_azureCognitiveServicesKey)) { Endpoint = _azureCognitiveServicesEndpoint };
            var detectedFaces = await client.Face.DetectWithUrlAsync(
                imageUrl,
                returnFaceAttributes: new List<FaceAttributeType?> { FaceAttributeType.Emotion },
                detectionModel: DetectionModel.Detection01,
                recognitionModel: RecognitionModel.Recognition03);

            return detectedFaces;
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

        private string GetMaxEmotion(DetectedFace face)
        {
            var emotions = face.FaceAttributes.Emotion;
            var emotionsDictionnary = emotions.GetType().GetProperties().ToDictionary(x => x.Name, x => (double)x.GetValue(emotions));
            var maxEmotions = emotionsDictionnary.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            var emotionString = maxEmotions + " : " + string.Format("{0:0.00}", emotionsDictionnary[maxEmotions]);

            return emotionString;
        }
    }
}

