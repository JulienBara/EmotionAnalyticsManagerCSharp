using System.Collections.Generic;

namespace EmotionAnalyticManagerCoreStandard.Helpers
{
    public static class Translation
    {
        public static readonly Dictionary<string, string> Dictionary = new Dictionary<string, string>
        {
            {"anger", "Colère"},
            {"disgust", "Dégout"},
            {"fear", "Peur"},
            {"joy", "Joie"},
            {"sadness", "Tristesse"},
            {"emotion", "Emotion"},
            {"value", "Valeur"}
        };
    }
}
