using System.Collections;
using UnityEngine.Networking;

namespace Fallen_LE_Mods.Shared.UI
{

    public static class VersionChecker
    {
        public static bool UpdateAvailable;
        public static string? LatestVersion;
        public static IEnumerator CheckGitHubBuildInfo()
        {
            string url = "https://raw.githubusercontent.com/FallenStar08/Fallen_LE_Mods/refs/heads/master/BuildInfo.cs";

            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            try
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string sourceCode = webRequest.downloadHandler.text;

                    //Always keep version under name in build info for this to work, and make sure to update it with every release...
                    string pattern = $@"""{BuildInfo.Name}"".*?Version\s*=\s*""([^""]+)""";

                    var match = System.Text.RegularExpressions.Regex.Match(
                        sourceCode,
                        pattern,
                        System.Text.RegularExpressions.RegexOptions.Singleline
                    );

                    if (match.Success)
                    {
                        string remoteVersion = match.Groups[1].Value;
                        if (remoteVersion != BuildInfo.Version)
                        {
                            VersionChecker.UpdateAvailable = true;
                            VersionChecker.LatestVersion = remoteVersion;
                        }
                    }
                }
            }
            finally
            {
                webRequest?.Dispose();
            }

        }
    }
}
