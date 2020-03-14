namespace Mapping.Common
{
    /// <summary>
    /// Storage class for connection data to handle connections to the API.
    /// Created by Timothy J Cowen.
    /// </summary>
    internal class ApiConnection
    {
        internal string BaseUrl { get; set; }
        public string AddImageUrl => BaseUrl + "addimg/";
        public string GetImageUrl => BaseUrl + "getimg/";
        public string AddPointUrl => BaseUrl + "addpoint/";
        public string GetPointsIDs => BaseUrl + "getpointids/";

        public ApiConnection(string baseUrl)
        {
            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += '/';
            }

            BaseUrl = baseUrl;
        }
    }
}
