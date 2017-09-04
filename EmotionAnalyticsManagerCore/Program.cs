using System;

namespace EmotionAnalyticsManagerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var ret = EmotionText.AnalyseEmotionText("Bonjour Monde !");
            Console.WriteLine(ret);
        }
    }
}
