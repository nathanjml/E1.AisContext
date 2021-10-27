using E1Translator.Core.Config;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Integration.EntityFrameworkCore.Transactions;
using UnstableSort.Crudless.Mediator;

namespace E1Translator.Core.AIS
{
    [NoTransaction]
    public class DataServiceRequest<TAisResponse>
            : IRequest<AisResponse<TAisResponse>>
    {
        public bool LoadAllResults { get; set; } = true;
        public AisDataServiceRequest AisRequest { get; set; }
    }


    public class AisDataServiceRequestHandler<TAisResponse>
        : IRequestHandler<DataServiceRequest<TAisResponse>, AisResponse<TAisResponse>>
    {
        private readonly IAISConfiguration _settings;
        private readonly HttpClient _http;
        private readonly IAisSessionProvider _tokenProvider;

        public AisDataServiceRequestHandler(
            IAISConfiguration settings
            , IHttpClientFactory httpClient
            , IAisSessionProvider tokenProvider)
        {
            _settings = settings;
            _http = httpClient.CreateClient("ais");
            _tokenProvider = tokenProvider;
        }

        public async Task<Response<AisResponse<TAisResponse>>> HandleAsync(
            DataServiceRequest<TAisResponse> request, CancellationToken cancellationToken)
        {
            var session = await _tokenProvider.GetSession();
            request.AisRequest.Token = session.Token;
            request.AisRequest.DeviceName = session.DeviceName;

            _http.BaseAddress = new Uri(_settings.AisBaseUrl);

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            var aisRequest = JsonConvert.SerializeObject(request.AisRequest
                , Formatting.Indented
                , settings);

            var payload = new StringContent(aisRequest, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(E1.Endpoints.DataService(), payload);

            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = ParseContent(responseContent);
                if (result.HasErrors || !request.LoadAllResults)
                {
                    if (result.HasErrors)
                    {
                        LogErrors(result);
                    }

                    return result.AsResponse();
                }
                else
                {
                    while (!result.HasErrors && result.DataBrowser.Data.GridData.Summary.MoreRecords)
                    {
                        result = CombineResults(result.DataBrowser.Data.GridData
                            , await LoadNext(_http
                                , result.Links.First(x => x.Rel == "next").Href));
                    }

                    if (result.HasErrors)
                    {
                        LogErrors(result);
                    }

                    return result.AsResponse();
                }
            }
            var error = new Error { ErrorMessage = getErrorMessage(responseContent) };
            return error.AsResponse<AisResponse<TAisResponse>>();
        }

        private AisResponse<TAisResponse> CombineResults(AisGridData<TAisResponse> a
            , AisResponse<TAisResponse> b)
        {
            b.DataBrowser.Data.GridData.Summary.Records += a.Summary.Records;
            b.DataBrowser.Data.GridData.RowSet = Enumerable.Concat(a.RowSet
                , b.DataBrowser.Data.GridData.RowSet).ToList();
            return b;
        }

        private async Task<AisResponse<TAisResponse>> LoadNext(HttpClient client, string nextUri)
        {
            var nextResponse = await client.GetAsync(nextUri);
            var content = await nextResponse.Content.ReadAsStringAsync();
            return ParseContent(content);
        }

        private AisResponse<TAisResponse> ParseContent(string content)
        {
            return JsonConvert.DeserializeObject<AisResponse<TAisResponse>>(content
                , new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new List<JsonConverter> { new AisJsonConverter<TAisResponse>() }
                });
        }

        private string getErrorMessage(string errorContent)
        {
            //Log.Warning(errorContent);

            if (errorContent[0] == '{')
            {
                var msg = JsonConvert.DeserializeObject<AisException>(errorContent);
                return msg.Message;
            }

            return "Unknown error during request";
        }

        private void LogErrors(AisResponse<TAisResponse> result)
        {
            //result.SysErrors?.ForEach(x => Log.Warning(x.Title + ": " + x.Desc));
            //result.DataBrowser?.Errors?.ForEach(x => Log.Warning(x.Mobile));
        }
    }

    public class AisDataServiceRequestValidator<TAisResponse>
        : AbstractValidator<DataServiceRequest<TAisResponse>>
    {
        public AisDataServiceRequestValidator()
        {
            RuleFor(x => x.AisRequest).NotNull();
            RuleFor(x => x.AisRequest.TargetName).NotEmpty();
            RuleFor(x => x.AisRequest.TargetType).Must(t => t == "view" || t == "table");
        }
    }
}
