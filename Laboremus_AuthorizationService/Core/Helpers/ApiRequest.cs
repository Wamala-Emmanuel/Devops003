using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.Core.Helpers
{
    public static class ApiRequest
    {
        public static string UserId;
        public static string OrganisationId;

        public static void Init(string userId, string organisationId)
        {
            UserId = userId;
            OrganisationId = organisationId;
        }

        public static async Task<ApiResponse> GetAsync(string url, string token)
        {
            using (var client = new HttpClient())
            {
                var request = PrepareRequest(url, token);
                request.Method = HttpMethod.Get;

                var response = await client.SendAsync(request);
                return await ProcessResponse(response, token);
            }
        }

        public static async Task<ApiResponse> DeleteAsync(string url, string token)
        {
            using (var client = new HttpClient())
            {
                var request = PrepareRequest(url, token);
                request.Method = HttpMethod.Delete;

                var response = await client.SendAsync(request);
                return await ProcessResponse(response, token);
            }
        }

        public static async Task<ApiResponse> PostAsync(string url, string token, object obj)
        {

            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(obj);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var content = new ByteArrayContent(buffer);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var request = PrepareRequest(url, token);
                request.Method = HttpMethod.Post;
                request.Content = content;

                var response = await client.SendAsync(request);
                return await ProcessResponse(response, token);
            }
        }

        public static async Task<ApiResponse> UpdateAsync(string url, string token, object obj)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(obj);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var content = new ByteArrayContent(buffer);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var request = PrepareRequest(url, token);
                request.Method = HttpMethod.Put;
                request.Content = content;

                var response = await client.SendAsync(request);
                return await ProcessResponse(response, token);
            }
        }


        public static async Task<ApiResponse> ProcessResponse(HttpResponseMessage response, string token)
        {
            var apiResponse = new ApiResponse
            {
                StatusCode = response.StatusCode,
                Errors = new List<string>()
            };
            var data = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                apiResponse.Data = data;
                apiResponse.StatusCode = HttpStatusCode.OK;
                apiResponse.Message = "success";
            }
            else
            {
                if (!string.IsNullOrEmpty(data))
                {
                    var annonymousErrorObject = new
                    {
                        Message = "",
                        ModelState = new Dictionary<string, string[]>()
                    };
                    var deserializeErrorObject = JsonConvert.DeserializeAnonymousType(data, annonymousErrorObject);

                    if (deserializeErrorObject?.ModelState != null)
                    {
                        var errors = deserializeErrorObject.ModelState.Select(kvp => string.Join("\n", kvp.Value)).ToList();
                        for (var i = 0; i < errors.Count; i++)
                        {
                            apiResponse.Errors.Add(errors.ElementAt(i));
                        }
                    }
                    else
                    {
                        var error = JsonConvert.DeserializeObject<ErrorResponse>(data);
                        if (!string.IsNullOrEmpty(error?.Message))
                        {
                            var deserialisedErrors = JsonConvert.DeserializeObject<List<string>>(error.Message);
                            apiResponse.Errors.AddRange(deserialisedErrors);
                        }
                    }
                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            apiResponse.Errors.Add("Access denied: Login again");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return apiResponse;
        }

        private static HttpRequestMessage PrepareRequest(string url, string token)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            request.Headers.Add("UserId", UserId);
            request.Headers.Add("OrganisationId", OrganisationId);
            return request;
        }
    }
}
