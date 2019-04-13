using System.Collections.Generic;

namespace EmotionAnalyticsManagerCore
{
    class MicrosoftEmotionAnswerFaceDto
    {
        public FaceRectangle faceRectangle;
        public FaceAttributes faceAttributes;
    }

    class FaceRectangle
    {
        public int height;
        public int left;
        public int top;
        public int width;
    }

    class FaceAttributes
    {
        public Dictionary<string, double> emotion;
    }
}
