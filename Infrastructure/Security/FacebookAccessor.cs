using Application.Interfaces;
using Application.User;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Infrastructure.Security
{
    public class FacebookAccessor : IFacebookAccessor
    {
        private readonly HttpClient HttpClient;
        private readonly IOptions<FacebookAppSettings> Config;

        public FacebookAccessor(IOptions<FacebookAppSettings> config)
        {
            Config = config;
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/")
            };
            HttpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<FacebookUserInfo> FacebookLogin(string accessToken)
        {
            var verifyToken = await HttpClient.GetAsync($"debug_token?input_token=" +
                $"{accessToken}&access_token={Config.Value.AppId}|{Config.Value.AppSecret}");

            if (!verifyToken.IsSuccessStatusCode)
                return null;

            var result = await GetAsync<FacebookUserInfo>(
                accessToken, "me", "fields=name,email,picture.width(100).height(100)");

            return result;
        }

        private async Task<T> GetAsync<T>(string accessToken, string endpoint, string args)
        {
            var response = await HttpClient.GetAsync($"{endpoint}?access_token={accessToken}&{args}");

            if (!response.IsSuccessStatusCode)
                return default(T);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}