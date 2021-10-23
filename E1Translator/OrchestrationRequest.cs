using E1Translator.Core;
using E1Translator.Core.AIS;
using E1Translator.Core.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

namespace E1Translator
{
    [DoNotValidate]
    public class OrchestrationRequest
            : IRequest<OrchestrationResponse>
    {
        public OrchRequest Request { get; set; }
        public string OrchestrationName { get; set; }
        public string Version { get; set; } = "v2";
    }

    public class OrchRequest
    {
        public List<Input> Inputs { get; set; } = new List<Input>();

        public List<DetailInput> DetailInputs { get; set; } = new List<DetailInput>();

        public string Token { get; set; }
        public string DeviceName { get; set; }
    }

    public class Input
    {
        public Input() { }

        public Input(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class DetailInput
    {
        public string Name { get; set; }
        public List<RepeatingInput> RepeatingInputs { get; set; } = new List<RepeatingInput>();
    }

    public class RepeatingInput
    {
        public List<Input> Inputs { get; set; }
    }


    public class OrchestrationResponse
    {
        public IDictionary<string, string> Data { get; set; }
        public IList<IDictionary<string, string>> GridRows { get; set; }
        public OrchestrationErrorResponse Errors { get; set; }
        public bool HasErrors => Errors?.Errors?.Any() ?? false;
    }

    public class OrchestrationErrorResponse
    {
        public string ApplicationID { get; set; }
        public string Title { get; set; }
        public IEnumerable<OrchestrationMessage> Errors { get; set; }
        public IEnumerable<OrchestrationMessage> Warnings { get; set; }
    }

    public class OrchestrationMessage
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string ErrorControl { get; set; }
        public string Desc { get; set; }
        public string Mobile { get; set; }
    }

    public class OrchestrationRequestHandler
        : IRequestHandler<OrchestrationRequest, OrchestrationResponse>
    {
        private readonly HttpClient _http;
        private readonly IAisSessionProvider _tokenProvider;
        private readonly IAISConfiguration _settings;


        public OrchestrationRequestHandler(
            IAISConfiguration settings,
            IHttpClientFactory httpClient,
            IAisSessionProvider tokenProvider)
        {
            _settings = settings;
            _http = httpClient.CreateClient("orchestration");
            _tokenProvider = tokenProvider;
        }

        public async Task<Response<OrchestrationResponse>> HandleAsync(OrchestrationRequest request)
        {
            _http.BaseAddress = new Uri(_settings.AisBaseUrl);

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var session = await _tokenProvider.GetSession();
            request.Request.Token = session.Token;
            request.Request.DeviceName = session.DeviceName;

            var orchRequest = JsonConvert.SerializeObject(request.Request
                , Formatting.Indented
                , settings);

            var baseUri = E1.Endpoints.Orchestrator(request.Version);

            var payload = new StringContent(orchRequest, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{baseUri}/{request.OrchestrationName}"
                , payload);

            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = ParseContent(responseContent);
                return result.AsResponse();
            }

            return Error.AsResponse<OrchestrationResponse>(GetErrorMessage(responseContent));
        }

        private OrchestrationResponse ParseContent(string content)
        {
            return JsonConvert.DeserializeObject<OrchestrationResponse>(content
                , new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new List<JsonConverter> { new OrchestrationJsonConverter() }
                });
        }

        private string GetErrorMessage(string errorContent)
        {
            if (errorContent[0] == '{')
            {
                var msg = JsonConvert.DeserializeObject<AisException>(errorContent);
                return msg.Message;
            }

            return "Unknown error during request";
        }
    }

    public static class OrchestrationResponseHelpers
    {
        //public static string GetAllErrors(Response<OrchestrationResponse> response)
        //{
        //    response.Errors.ForEach(x => Log.Warning(x.ErrorMessage));
        //    response.Data?.Errors?.Errors.ForEach(x => Log.Warning(x.Mobile));
        //    return String.Join(" ",
        //        Enumerable.Concat(
        //            response.Errors.Select(x => ExtractMobileMessage(x.ErrorMessage)),
        //            response.Data?.Errors?.Errors?.Select(x => x.Mobile) ?? new string[] { }));
        //}

        public static bool HasErrors(Response<OrchestrationResponse> response)
        {
            return response.HasErrors || response.Data.HasErrors;
        }

        private static string ExtractMobileMessage(string error)
        {
            if (error.Contains("MOBILE"))
            {
                var mobileIndex = error.IndexOf("MOBILE");
                var startIndex = error.IndexOf("\"", error.IndexOf(":", mobileIndex)) + 1;
                var endIndex = error.IndexOf("\"", startIndex);

                return error.Substring(startIndex, endIndex - startIndex);
            }

            return error;
        }
    }
}
