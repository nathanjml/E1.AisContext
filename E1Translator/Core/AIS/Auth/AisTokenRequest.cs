using E1Translator.Core.Config;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Mediator;

namespace E1Translator.Core.AIS.Auth
{
    public class AisTokenRequest : IRequest<AisSessionInfo>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Device { get; set; }
        public string Version { get; set; } = "v2";
    }

    public class AisTokenRequestValidator : AbstractValidator<AisTokenRequest>
    {
        public AisTokenRequestValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class AisTokenRequestHandler
       : IRequestHandler<AisTokenRequest, AisSessionInfo>
    {
        private readonly IAISConfiguration _settings;
        private readonly HttpClient _http;
        //private readonly ILogger _logger;

        public AisTokenRequestHandler(IAISConfiguration settings,
            IHttpClientFactory httpClient
            //ILogger logger,
            )
        {
            _settings = settings;
            _http = httpClient.CreateClient("ais");
            _http.BaseAddress = new Uri(_settings.AisBaseUrl);
            //_logger = logger;
        }

        public async Task<Response<AisSessionInfo>> HandleAsync(AisTokenRequest request, CancellationToken ct)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var json = JsonConvert.SerializeObject(new AisAuthInfo
                {
                    Username = request.Username,
                    Password = request.Password,
                    Environment = _settings.AisEnvironment,
                    Role = _settings.AisRole,
                    DeviceName = request.Device,
                }, settings);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync(E1.Endpoints.TokenRequest(request.Version), content);

                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var authResponse = JsonConvert.DeserializeObject<AisAuthResponse>(responseContent)
                        .AsResponse();

                    var session = new AisSessionInfo
                    {
                        Token = authResponse.Result.UserInfo.Token,
                        DeviceName = request.Device
                    };

                    return session.AsResponse();
                }
                return new Error {ErrorMessage = GetErrorMessage(responseContent)}.AsResponse<AisSessionInfo>();
            }
            catch (Exception e)
            {
                //_logger.Fatal(e, "{Message:l}", e.Message);
                return new Error { ErrorMessage = "AIS Server currently unavailable. Please contact the help desk if problem persists." }.AsResponse<AisSessionInfo>(); ;
            }
        }

        private string GetErrorMessage(string errorContent)
        {
            if (errorContent[0] == '{')
            {
                var msg = JsonConvert.DeserializeObject<AisException>(errorContent);

                if (msg.Message.Contains("Connection refused"))
                {
                    //_logger.Fatal(errorContent);
                    return "AIS Server currently unavailable. Please contact the help desk if problem persists.";
                }
                else
                {
                    //_logger.Error(errorContent);
                    return msg.Message;
                }
            }

            //_logger.Error(errorContent);
            return "Unknown error during token request";
        }
    }
}
