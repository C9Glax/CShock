﻿using CShocker.Devices.Additional;
using CShocker.Ranges;
using CShocker.Shockers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace CShocker.Devices.Abstract;

public abstract class OpenShockApi : Api
{
    // ReSharper disable twice MemberCanBeProtected.Global -> Exposed
    public string Endpoint { get; init; }
    public string ApiKey { get; init; }
    public const string DefaultEndpoint = "https://api.openshock.app";

    // ReSharper disable once PublicConstructorInAbstractClass -> Exposed
    public OpenShockApi(DeviceApi apiType, string apiKey, string endpoint = DefaultEndpoint, ILogger? logger = null) : base(apiType, new IntegerRange(0, 100), new IntegerRange(300, 30000), logger)
    {
        this.Endpoint = endpoint;
        this.ApiKey = apiKey;
    }

    public override bool Equals(object? obj)
    {
        return obj is OpenShockApi osd && Equals(osd);
    }

    private bool Equals(OpenShockApi other)
    {
        return base.Equals(other) && Endpoint.Equals(other.Endpoint) && ApiKey.Equals(other.ApiKey);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Endpoint, ApiKey);
    }

    public IEnumerable<OpenShockShocker> GetShockers()
    {
        return GetShockers(this.ApiKey, this, this.Endpoint, this.Logger);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<OpenShockShocker> GetShockers(string apiKey, OpenShockApi api, string apiEndpoint = DefaultEndpoint, ILogger? logger = null)
    {
        List<OpenShockShocker> shockers = new();
        
        HttpResponseMessage ownResponse = ApiHttpClient.MakeAPICall(HttpMethod.Get, $"{apiEndpoint}/1/shockers/own", null, logger, new ValueTuple<string, string>("OpenShockToken", apiKey));
        if (!ownResponse.IsSuccessStatusCode)
            return shockers;
        
        StreamReader ownShockerStreamReader = new(ownResponse.Content.ReadAsStream());
        string ownShockerJson = ownShockerStreamReader.ReadToEnd();
        logger?.Log(LogLevel.Debug,ownShockerJson);
        JObject ownShockerListJObj = JObject.Parse(ownShockerJson);
        shockers.AddRange(ownShockerListJObj.SelectTokens("$.data..shockers[*]").Select(t =>
            new OpenShockShocker(
                api,
            t["name"]!.Value<string>()!, t["id"]!.Value<string>()!, t["rfId"]!.Value<short>(),
            Enum.Parse<OpenShockShocker.OpenShockModel>(t["model"]!.Value<string>()!),
            t["createdOn"]!.ToObject<DateTime>(), t["isPaused"]!.Value<bool>())));
        
        HttpResponseMessage sharedResponse = ApiHttpClient.MakeAPICall(HttpMethod.Get, $"{apiEndpoint}/1/shockers/shared", null, logger, new ValueTuple<string, string>("OpenShockToken", apiKey));
        if (!sharedResponse.IsSuccessStatusCode)
            return shockers;
        
        StreamReader sharedShockerStreamReader = new(sharedResponse.Content.ReadAsStream());
        string sharedShockerJson = sharedShockerStreamReader.ReadToEnd();
        logger?.Log(LogLevel.Debug, sharedShockerJson);
        JObject sharedShockerListJObj = JObject.Parse(sharedShockerJson);
        shockers.AddRange(sharedShockerListJObj.SelectTokens("$.data..shockers[*]").Select(t =>
            new OpenShockShocker(
                api, 
                t["name"]!.Value<string>()!, t["id"]!.Value<string>()!, t["rfId"]!.Value<short>(),
                Enum.Parse<OpenShockShocker.OpenShockModel>(t["model"]!.Value<string>()!),
                t["createdOn"]!.ToObject<DateTime>(), t["isPaused"]!.Value<bool>())));
        return shockers;
    }
}