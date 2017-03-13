using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
