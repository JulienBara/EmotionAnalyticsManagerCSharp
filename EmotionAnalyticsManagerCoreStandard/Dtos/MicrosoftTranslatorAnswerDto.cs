using System.Collections.Generic;

namespace EmotionAnalyticsManagerCoreStandard.Dtos
{
    public class MicrosoftTranslatorAnswerDto
    {
        public List<TextTranslation> Translations { get; set; }
    }

    public class TextTranslation
    {
        public string Text { get; set; }
        public string To { get; set; }
    }
}
