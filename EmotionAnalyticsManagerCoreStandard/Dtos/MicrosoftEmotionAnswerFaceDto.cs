using System.Collections.Generic;

namespace EmotionAnalyticManagerCoreStandard.Dtos
{
    public class MicrosoftEmotionAnswerFaceDto
    {
        public FaceRectangle faceRectangle;
        public FaceAttributes faceAttributes;
    }

    public class FaceRectangle
    {
        public int height;
        public int left;
        public int top;
        public int width;
    }

    public class FaceAttributes
    {
        public Dictionary<string, double> emotion;
    }
}
