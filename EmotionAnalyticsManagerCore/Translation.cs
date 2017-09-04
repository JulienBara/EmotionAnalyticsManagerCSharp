using System.Collections.Generic;

namespace EmotionAnalyticsManagerCore
{
    public class Translation
    {
        public Dictionary<string, string> dictionary;

        public Translation()
        {
            dictionary = new Dictionary<string, string>();
            dictionary.Add("anger", "Colère");
            dictionary.Add("disgust", "Dégout");
            dictionary.Add("fear", "Peur");
            dictionary.Add("joy", "Joie");
            dictionary.Add("sadness", "Tristesse");
            dictionary.Add("emotion", "Emotion");
            dictionary.Add("value", "Valeur");
        }
    }
}
