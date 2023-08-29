using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Infra.Messaging.JsonPolicies;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace FC.CodeFlix.Catalog.EndToEndTests.Base;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakAuthenticationOptions _keycloakOption;
    private readonly JsonSerializerOptions _defaultSerializeOptions;

    private const string _adminUser = "admin";
    private const string _adminPassword = "123456";

    public ApiClient(HttpClient httpClient, KeycloakAuthenticationOptions keycloakOption)
    {
        _httpClient = httpClient;
        _keycloakOption = keycloakOption;

        _defaultSerializeOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = new JsonSnakeCasePolicy(),
            PropertyNameCaseInsensitive = true
        };
        AddAuthorizationHeader();
    }

    public async Task<string> GetAccessTokenAsync(string user, string password)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_keycloakOption.KeycloakUrlRealm}/protocol/openid-connect/token");
        var collection = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("client_id", _keycloakOption.Resource),
            new("client_secret", _keycloakOption.Credentials.Secret),
            new("username", user),
            new("password", password)
        };
        var content = new FormUrlEncodedContent(collection);
        request.Content = content;
        var response = await client.SendAsync(request);

        var credentials = await GetOutput<Credentials>(response);

        return credentials!.AccessToken;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        => await _httpClient.SendAsync(request);

    public async Task<(HttpResponseMessage?, TOutput?)> Post<TOutput>(string route, object payload)
        where TOutput : class
    {
        var response = await _httpClient.PostAsync(route, new StringContent(
             JsonSerializer.Serialize(payload, _defaultSerializeOptions),
             Encoding.UTF8,
             "application/json"
            ));

        var output = await GetOutput<TOutput>(response);

        return (response, output);
    }


    public async Task<(HttpResponseMessage?, TOutput?)> Get<TOutput>(string route, object? queryStringParamObjects = null)
        where TOutput : class
    {
        var url = PrepareGetRoute(route, queryStringParamObjects);
        var response = await _httpClient.GetAsync(url);

        var output = await GetOutput<TOutput>(response);

        return (response, output);
    }

    public async Task<(HttpResponseMessage?, TOutput?)> Delete<TOutput>(string route)
        where TOutput : class
    {
        var response = await _httpClient.DeleteAsync(route);

        var output = await GetOutput<TOutput>(response);

        return (response, output);
    }


    public async Task<(HttpResponseMessage?, TOutput?)> Put<TOutput>(string route, object payload)
        where TOutput : class
    {
        var response = await _httpClient.PutAsync(route, new StringContent(
             JsonSerializer.Serialize(payload, _defaultSerializeOptions),
             Encoding.UTF8,
             "application/json"
            ));

        var output = await GetOutput<TOutput>(response);

        return (response, output);
    }

    public async Task<(HttpResponseMessage?, TOutput?)> PostFormData<TOutput>(string route, FileInput file)
    where TOutput : class
    {
        var fileContent = new StreamContent(file.FileStream);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        using var content = new MultipartFormDataContent
        {
            { fileContent, "media_file", $"media.{file.Extension}" }
        };

        var response = await _httpClient.PostAsync(route, content);

        var output = await GetOutput<TOutput>(response);

        return (response, output);
    }

    private async Task<TOutput?> GetOutput<TOutput>(HttpResponseMessage response)
        where TOutput : class
    {
        var outputString = await response.Content.ReadAsStringAsync();
        TOutput? output = null;
        if (!string.IsNullOrWhiteSpace(outputString))
        {
            output = JsonSerializer.Deserialize<TOutput>(outputString, _defaultSerializeOptions);
        }
        return output;
    }

    private void AddAuthorizationHeader()
    {
        var accessToken = GetAccessTokenAsync(_adminUser, _adminPassword)
            .GetAwaiter().GetResult();
        _httpClient.DefaultRequestHeaders
            .Authorization = new AuthenticationHeaderValue(
                "Bearer", accessToken);
    }


    private string PrepareGetRoute(string route, object? queryStringParamObjects)
    {
        if (queryStringParamObjects is null) return route;

        var parametersJson = JsonSerializer.Serialize(queryStringParamObjects, _defaultSerializeOptions);
        var parametersDictionary = Newtonsoft.Json.JsonConvert
                                            .DeserializeObject<Dictionary<string, string>>(parametersJson);
        return QueryHelpers.AddQueryString(route, parametersDictionary!);


    }

    
}