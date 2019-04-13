using System.Collections.Generic;

namespace EmotionAnalyticManagerCoreStandard.Dtos
{
    public class IbmAnswerDto
    {
        public Dictionary<string,double> Usage { get; set; }
        public string Language { get; set; }
        public EmotionDto Emotion { get; set; }
    }

    public class EmotionDto
    {
        public DocumentDto Document { get; set; }
    }

    public class DocumentDto
    {
        public Dictionary<string,double> Emotion { get; set; }
    }
}
