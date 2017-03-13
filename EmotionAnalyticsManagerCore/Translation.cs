using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
