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
        public Scores scores;
    }

    class FaceRectangle
    {
        public int height;
        public int left;
        public int top;
        public int width;
    }

    class Scores
    {
        public double anger;
        public double contempt;
        public double disgust;
        public double fear;
        public double happiness;
        public double neutral;
        public double sadness;
        public double surprise;
    }
}
