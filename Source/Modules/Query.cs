namespace DiscordStatus
{
    internal class Query : IQuery
    {
        public async Task<string> GetCountryCodeAsync(string ipAddress)
        {
            using var client = CreateHttpClient();
            try
            {
                string requestUri = $"http://ip-api.com/json/{ipAddress}";

                HttpResponseMessage response = await client.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
                    return data.countryCode ?? "CC Error";
                }
                else
                {
                    DSLog.Log(2, $"Error getting country code. Status code: {response.StatusCode}");
                    return "CC Error";
                }
            }
            catch (Exception ex)
            {
                DSLog.Log(2, $"Exception in GetCountryCodeAsync: {ex.Message}");
                return "CC Error";
            }
        }

        public async Task<string> IPQueryAsync(string ipAddress, string endpoint)
        {
            using var client = CreateHttpClient();
            try
            {
                string apiUrl = $"https://ipapi.co/{ipAddress}/{endpoint}/";
                string response = await client.GetStringAsync(apiUrl).ConfigureAwait(false);
                return response.Trim();
            }
            catch (HttpRequestException ex)
            {
                DSLog.Log(2, $"HttpRequestException in IPQueryAsync: {ex.Message}");
                return "Error";
            }
            catch (Exception ex)
            {
                DSLog.Log(2, $"Exception in IPQueryAsync: {ex.Message}");
                return "Error";
            }
        }

        private static HttpClient CreateHttpClient()
        {
            return new HttpClient();
        }

        /*  public async Task<string> GetIPAsync()
          {
              using var client = CreateHttpClient();
              try
              {
                  string apiUrl = "https://api.ipify.org";
                  HttpResponseMessage response = await client.GetAsync(apiUrl).ConfigureAwait(false);
                  response.EnsureSuccessStatusCode();
                  string serverip = await response.Content.ReadAsStringAsync();
                  DSLog.Log(0, $"Finished getting IP Address: {serverip}");
                  return serverip;
              }
              catch (Exception ex)
              {
                  DSLog.Log(2, $"Exception in GetIPAsync: {ex.Message}");
                  return "IP Error";
              }
          }*/
    }
}