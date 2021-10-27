using E1Translator.Core.Config;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Integration.EntityFrameworkCore.Transactions;
using UnstableSort.Crudless.Mediator;

namespace E1Translator.Core.AIS.Auth
{
    [NoTransaction]
    public class AisTokenValidationRequest : IRequest<bool>
    {
        public string Token { get; set; }
    }

    public class AisTokenValidationRequestValidator : AbstractValidator<AisTokenValidationRequest>
    {
        public AisTokenValidationRequestValidator()
        {
            RuleFor(x => x.Token).NotNull();
        }
    }

    public class AisTokenValidationRequestHandler : IRequestHandler<AisTokenValidationRequest, bool>
    {
        private readonly HttpClient _http;

        //private readonly ILogger _logger;

        public AisTokenValidationRequestHandler(IAISConfiguration settings
            , IHttpClientFactory httpClient
            //, ILogger logger)
            )
        {
            //_logger = logger;
            _http = httpClient.CreateClient("ais");
            _http.BaseAddress = new Uri(settings.AisBaseUrl);
        }

        public async Task<Response<bool>> HandleAsync(AisTokenValidationRequest request, CancellationToken ct)
        {
            if (request.Token == string.Empty)
                return false.AsResponse();

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var content = new StringContent(JsonConvert.SerializeObject(request, settings)
                , Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("/jderest/v2/tokenrequest/validate", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JObject.Parse(responseContent).Value<bool>("isValidSession").AsResponse();
            }

            //_logger.Warning(responseContent);

            return false.AsResponse();
        }
    }
}
