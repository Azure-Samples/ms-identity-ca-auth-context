using System.Linq;
using TodoListClient.Services;

namespace TodoListClient;

internal class ExtractAuthenticationHeader
{
    /// <summary>
    /// Extract claims from WwwAuthenticate header and returns the value.
    /// </summary>
    internal static string ExtractHeaderValues(WebApiMsalUiRequiredException response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && response.Headers.WwwAuthenticate.Any())
        {
            return AuthenticationHeaderHelper.ExtractClaimChallengeFromHttpHeader(response.Headers);
        }

        return null;
    }
}