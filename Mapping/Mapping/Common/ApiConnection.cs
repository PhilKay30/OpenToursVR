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
        public string GetPointsIDs => BaseUrl + "getpointid/";
        // The following one is to be followed with an ID to get the data.
        public string GetPointData => BaseUrl + "getpointid/";

        public string AddBoundsUrl => BaseUrl + "addbounds/";
        public string GetBoundsUrl => BaseUrl + "getbounds/";

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
