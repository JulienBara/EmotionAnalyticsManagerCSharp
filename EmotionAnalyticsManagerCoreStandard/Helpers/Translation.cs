using System.Collections.Generic;

namespace EmotionAnalyticsManagerCoreStandard.Helpers
{
    public static class Translation
    {
        public static readonly Dictionary<string, string> Dictionary = new Dictionary<string, string>
        {
            {"Anger", "Colère"},
            {"Disgust", "Dégout"},
            {"Fear", "Peur"},
            {"Joy", "Joie"},
            {"Sadness", "Tristesse"},
            {"Emotion", "Emotion"},
            {"Value", "Valeur"}
        };
    }
}
