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
        public Dictionary<string,double> docEmotions;
    }
}
