﻿using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CShocker.Devices.Additional;

public static class ApiHttpClient
{
    internal static HttpResponseMessage MakeAPICall(HttpMethod method, string uri, string? jsonContent, ILogger? logger = null, params ValueTuple<string, string>[] customHeaders)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        ProductInfoHeaderValue userAgent = new (fvi.ProductName ?? fvi.FileName, fvi.ProductVersion);
        HttpRequestMessage request = new (method, uri)
        {
            Headers =
            {
                UserAgent = { userAgent },
                Accept = { new MediaTypeWithQualityHeaderValue("application/json") },
                
            }
        };
        if (jsonContent is not null && jsonContent.Length > 0)
            request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonContent))
            {
                Headers = { ContentType = MediaTypeHeaderValue.Parse("application/json") }
            };
        foreach ((string, string) customHeader in customHeaders)
            request.Headers.Add(customHeader.Item1, customHeader.Item2);
        logger?.Log(LogLevel.Debug, string.Join("\n\t",
                "Request:",
                $"\u251c\u2500\u2500 URI: {request.RequestUri}",
                $"\u251c\u2500\u2510 Headers: {string.Concat(request.Headers.Select(h => $"\n\t\u2502 {(request.Headers.Last().Key.Equals(h.Key) ? "\u2514" : "\u251c")} {h.Key}: {string.Join(", ", h.Value)}"))}", 
                $"\u2514\u2500\u2500 Content: {request.Content?.ReadAsStringAsync().Result}"));

        HttpClient httpClient = new();
        HttpResponseMessage response = httpClient.Send(request);
        logger?.Log(!response.IsSuccessStatusCode ? LogLevel.Error : LogLevel.Debug, string.Join("\n\t",
            "Request:",
            $"\u251c\u2500\u2500 URI: {request.RequestUri}",
            $"\u251c\u2500\u2510 Headers: {string.Concat(response.Headers.Select(h => $"\n\t\u2502 {(response.Headers.Last().Key.Equals(h.Key) ? "\u2514" : "\u251c")} {h.Key}: {string.Join(", ", h.Value)}"))}", 
            $"\u2514\u2500\u2500 Content: {response.Content?.ReadAsStringAsync().Result}"));
        httpClient.Dispose();
        return response;
    }
}