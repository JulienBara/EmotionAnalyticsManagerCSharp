using System.Collections.Generic;

namespace EmotionAnalyticManagerCoreStandard.Dtos
{
    public class IbmAnswerDto
    {
        public Dictionary<string,double> usage;
        public string language;
        public EmotionDto emotion;
    }

    public class EmotionDto
    {
        public DocumentDto document;
    }

    public class DocumentDto
    {
        public Dictionary<string,double> emotion;
    }
}
