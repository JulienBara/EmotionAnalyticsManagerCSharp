using System.Collections.Generic;

namespace EmotionAnalyticsManagerCore
{
    class IbmAnswerDto
    {
        public Dictionary<string,double> usage;
        public string language;
        public EmotionDto emotion;
    }
    
    class EmotionDto
    {
        public DocumentDto document;
    }
    
    class DocumentDto
    {
        public Dictionary<string,double> emotion;
    }
}
