using IBM.Cloud.SDK.Core.Http;

namespace EmotionAnalyticsManagerCoreStandard.Helpers
{
    public static class DetailedResponseExtension
    {
        public static bool IsSuccessStatusCode<T>(this DetailedResponse<T> response)
        {
            return 
                200 <= response.StatusCode
                && response.StatusCode <= 299;
        }
    }
}
