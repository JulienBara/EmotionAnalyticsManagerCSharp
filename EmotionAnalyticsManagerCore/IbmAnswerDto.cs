using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionAnalyticsManagerCore
{
    class IbmAnswerDto
    {
        public string status;
        public string usage;
        public int totalTransactions;
        public string language;
        public DocEmotions docEmotions;
    }

    class DocEmotions
    {
        public double anger;
        public double disgust;
        public double fear;
        public double joy;
        public double sadness;
    }
}
