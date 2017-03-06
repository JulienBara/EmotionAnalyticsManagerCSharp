using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
