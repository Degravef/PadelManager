namespace ApiTest.Helpers;

internal static class HttpRequestExtensions
{
    public static HttpRequestMessage WithAdmin(this HttpRequestMessage request, int adminId)
    {
        request.Headers.Add("X-User-Role", "Admin");
        request.Headers.Add("X-User-Id", adminId.ToString());
        return request;
    }
}