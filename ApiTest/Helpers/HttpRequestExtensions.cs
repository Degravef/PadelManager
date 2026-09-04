namespace ApiTest.Helpers;

internal static class HttpRequestExtensions
{
    public static HttpRequestMessage WithAdmin(this HttpRequestMessage request, int adminId)
    {
        request.Headers.Add("X-User-Role", "Admin");
        request.Headers.Add("X-User-Id", adminId.ToString());
        return request;
    }

    public static HttpRequestMessage WithMember(this HttpRequestMessage request, string matricule)
    {
        request.Headers.Add("X-User-Role", "Member");
        request.Headers.Add("X-User-Id", matricule);
        return request;
    }
}