using Beaversims.Core.Common;
using Beaversims.Core.Shadow.ImportLogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Beaversims.Core.Shadow.WclClient
{
    internal class WclClient
    {
        //private const string AUTHORIZE_URI = "https://www.warcraftlogs.com/oauth/authorize";
        //private const string TOKEN_URI = "https://www.warcraftlogs.com/oauth/token";
        private const string API_URL = "https://www.warcraftlogs.com/api/v2/client";
        private static string accessToken;


        static async Task<string> ImportLogs(string query)
        {
            string API_URL = "https://www.warcraftlogs.com/api/v2/client";

            // Read access token from JSON
            string projectRoot = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Beaversims.Core")
            ); var tokenPath = Path.Combine(projectRoot, "Shadow", "ImportLogs", "access_token.json");

            var tokenJson = await File.ReadAllTextAsync(tokenPath);
            using var doc = JsonDocument.Parse(tokenJson);
            string accessToken = doc.RootElement.GetProperty("access_token").GetString();

            // Example GraphQL query
     

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var body = new { query = query };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(API_URL, content);

            string result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static string CachePath(List<string> linkElements)
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Beaversims.Core")
            );

            string cacheDir = Path.Combine(projectRoot, "Shadow", "ImportLogs", "Cache");
            string fileName = string.Join("-", linkElements) + ".json";
            return Path.Combine(cacheDir, fileName);
        }

        public static List<string> ParseLogLink(string logLink)
        {
            var uri = new Uri(logLink);

            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string reportCode = segments.Length > 1 ? segments[1] : string.Empty;

            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            string fightId = queryParams["fight"];
            string userId = queryParams["source"];
            return [reportCode, fightId, userId];
        }
        public static async Task<JsonDocument> GetLogs(string reportCode, int fightId, int userId)
        {
            var linkElements = new List<string>
            {
                reportCode,
                fightId.ToString(),
                userId.ToString()
            };
            string path = CachePath(linkElements);

            if (File.Exists(path))
            {
                string json = await File.ReadAllTextAsync(path);
                return JsonDocument.Parse(json);
            }

            string query = Queries.StandardSimQuery(reportCode, fightId, userId);
            string jsonResponse = await ImportLogs(query);

            await File.WriteAllTextAsync(path, jsonResponse);
            return JsonDocument.Parse(jsonResponse);
        }
        public static async Task<JsonDocument> GetFights(string reportCode)
        {
            var linkElements = new List<string>
            {
                reportCode,
            };
            string path = CachePath(linkElements);

            if (File.Exists(path))
            {
                string json = await File.ReadAllTextAsync(path);
                return JsonDocument.Parse(json);
            }
            string query = Queries.FightsQuery(reportCode);
            string jsonResponse = await ImportLogs(query);

            await File.WriteAllTextAsync(path, jsonResponse);
            return JsonDocument.Parse(jsonResponse);
        }
    }
}
