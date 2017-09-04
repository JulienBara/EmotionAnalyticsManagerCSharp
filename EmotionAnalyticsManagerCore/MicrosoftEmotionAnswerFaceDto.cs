using System.Collections.Generic;

namespace EmotionAnalyticsManagerCore
{
    class MicrosoftEmotionAnswerFaceDto
    {
        public FaceRectangle faceRectangle;
        public Dictionary<string, double> scores;
    }

    class FaceRectangle
    {
        public int height;
        public int left;
        public int top;
        public int width;
    }
}
