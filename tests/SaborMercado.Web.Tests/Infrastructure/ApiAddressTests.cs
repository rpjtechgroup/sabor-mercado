using SaborMercado.Web.Infrastructure;

namespace SaborMercado.Web.Tests.Infrastructure;

public class ApiAddressTests
{
    [Theory]
    [InlineData("https://rpjtechgroup.ddns.net/mercado", "/api/v1/auth/google", "https://rpjtechgroup.ddns.net/mercado/api/v1/auth/google")]
    [InlineData("http://localhost:5280", "/api/v1/starter-catalog", "http://localhost:5280/api/v1/starter-catalog")]
    public void CombineBaseAndPath_ResolvesUnderMercadoPrefix(string baseUrl, string path, string expected)
    {
        var client = new HttpClient { BaseAddress = ApiAddress.NormalizeBase(baseUrl) };
        var request = new HttpRequestMessage(HttpMethod.Post, ApiAddress.ToRelativePath(path));
        var resolved = new Uri(client.BaseAddress!, request.RequestUri!);
        Assert.Equal(expected, resolved.ToString());
    }
}
