using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AntDesign;
using Demo.Models.Dto;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Newtonsoft.Json;

namespace Demo.SPA.Services {
    public class RestService {
        private readonly HttpClient httpClient;
        private readonly NotificationService notice;

        public RestService(HttpClient httpClient, NotificationService notice) {
            this.httpClient = httpClient;
            this.notice = notice;
        }

        public async Task ExecuteGet(string apiPath, CancellationToken cancellationToken) {
            try {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiPath);
                requestMessage.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                await httpClient.SendAsync(requestMessage, cancellationToken);
            } catch (Exception e) {
                await notice.Error(new NotificationConfig {Message = "Ошибка!", Description = e.Message, Duration = 5});
            }
        }

        public async Task<TResult>
            ExecutePost<TRequest, TResult>(string apiPath, TRequest request, CancellationToken cancellationToken)
            where TResult : class {
            try {
                var jsonRequest = JsonConvert.SerializeObject(request);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiPath) {
                    Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
                };

                requestMessage.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                var response = await httpClient.SendAsync(requestMessage, cancellationToken);
                var jsonResult = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<Response<TResult>>(jsonResult);
                if (response.IsSuccessStatusCode)
                    if (result != null)
                        return result.Data;

                if (result == null) return null;
                var errorMessage = result.Errors.Aggregate(string.Empty,
                    (current, error) => current + $"{error.ErrorMessage}\n");
                await notice.Error(new NotificationConfig {
                    Message = "Ошибка!", Description = errorMessage, Duration = 5
                });

                return null;
            } catch (Exception e) {
                await notice.Error(new NotificationConfig {
                    Message = "Ошибка!", Description = e.Message + "\n" + e.StackTrace, Duration = 5
                });
                return null;
            }
        }
    }
}