using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionAnalyticsManagerCore
{
    class IbmAnswerDto
    {
        public Dictionary<string,double> usage;
        public string language;
        public IbmAnswerEmotionDto emotion;
    }
    
    class IbmAnswerEmotionDto
    {
        public IbmAnswerEmotionDocumentDto document;
    }
    
    class IbmAnswerEmotionDocumentDto
    {
        public Dictionary<string,double> emotion;
    }
}
