﻿using E1Translator;
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
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Integration.EntityFrameworkCore.Transactions;
using UnstableSort.Crudless.Mediator;

namespace TurnerTablet.Core.Scaffolding.Features.Ais
{
    [DoNotValidate]
    [NoTransaction]
    public class AppStackRequest<TAisResponse>
        : IRequest<AisResponse<TAisResponse>>
    {
        public AisAppStackRequest AisRequest { get; set; }
        public bool LoadAllResults { get; set; } = true;

        public string Version { get; set; } = "v2";
    }

    public class AisAppStackRequestHandler<TAisResponse>
        : IRequestHandler<AppStackRequest<TAisResponse>, AisResponse<TAisResponse>>
    {
        private readonly IAISConfiguration _settings;
        private readonly HttpClient _http;
        private readonly IAisSessionProvider _tokenProvider;


        public AisAppStackRequestHandler(
            IAISConfiguration settings
            , IHttpClientFactory httpClient
            , IAisSessionProvider tokenProvider)
        {
            _settings = settings;
            _http = httpClient.CreateClient("ais");
            _tokenProvider = tokenProvider;
        }

        public async Task<Response<AisResponse<TAisResponse>>> HandleAsync(
            AppStackRequest<TAisResponse> request, CancellationToken ct)
        {
            var session = await _tokenProvider.GetSession();

            if(session == null)
            {
                return new Error { ErrorMessage = Errors.AuthError }.AsResponse<AisResponse<TAisResponse>>();
            }

            PopulateSessionData(request, session);

            _http.BaseAddress = new Uri(_settings.AisBaseUrl);

            var aisRequest = SerializeRequest(request.AisRequest);

            var payload = new StringContent(aisRequest, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(E1.Endpoints.AppStack(request.Version)
                , payload, ct);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) throw new AisErrorException(getErrorMessage(responseContent));

            var result = ParseContent(responseContent);

            if (result == null)
            {
                return (new AisResponse<TAisResponse>()).AsResponse();
            }

            if (result.HasErrors)
            {
                var error = new AisErrorException(GetErrorMessage(result))
                {
                    StateId = result.StateId,
                    StackId = result.StackId,
                    Rid = result.Rid
                };

                throw error;
            }

            if (!request.LoadAllResults)
            {
                return result.AsResponse();
            }

            while (!result.HasErrors && result.DataBrowser.Data.GridData.Summary.MoreRecords)
            {
                var nextResult = await LoadNext(_http, result.Links.First(x => x.Rel == "next").Href);
                if (nextResult == null) break;
                result = CombineResults(result.DataBrowser.Data.GridData
                    , nextResult);
            }

            if (result.HasErrors)
            {
                var error = new AisErrorException(GetErrorMessage(result))
                {
                    StateId = result.StateId,
                    StackId = result.StackId,
                    Rid = result.Rid
                };

                throw error;
            }

            return result.AsResponse();

        }

        private static AisResponse<TAisResponse> CombineResults(AisGridData<TAisResponse> a
            , AisResponse<TAisResponse> b)
        {
            b.DataBrowser.Data.GridData.Summary.Records += a.Summary.Records;
            b.DataBrowser.Data.GridData.RowSet = Enumerable.Concat(a.RowSet
                , b.DataBrowser.Data.GridData.RowSet).ToList();
            return b;
        }

        private static async Task<AisResponse<TAisResponse>?> LoadNext(HttpClient client, string nextUri)
        {
            var nextResponse = await client.GetAsync(nextUri);
            var content = await nextResponse.Content.ReadAsStringAsync();
            return ParseContent(content ?? "");
        }

        private static AisResponse<TAisResponse>? ParseContent(string content)
        {
            return JsonConvert.DeserializeObject<AisResponse<TAisResponse>>(content
                , new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new List<JsonConverter> { new AisJsonConverter<TAisResponse>(), new AisDataJsonConverter<TAisResponse>() }
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

        private string GetErrorMessage(AisResponse<TAisResponse> result)
        {
            return string.Join(" ",
                Enumerable.Concat(
                    result.SysErrors?.Select(x => x.Title + ": " + x.Desc) ?? new string[] { },
                    result.DataBrowser?.Errors?.Select(x => x.Mobile) ?? new string[] { }));
        }

        private static void PopulateSessionData(AppStackRequest<TAisResponse> request, AisSessionInfo session)
        {
            request.AisRequest.Token = session.Token;
            request.AisRequest.DeviceName = session.DeviceName;
        }

        private static string SerializeRequest(AisAppStackRequest aisAppStackRequest)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(aisAppStackRequest, Formatting.Indented, settings);
        }
    }

    public class AisAppStackRequest : AisRequest
    {
        public string Token { get; set; }
        public string DeviceName { get; set; }
        public string FormName { get; set; }
        public string Version { get; set; }
        public string Action { get; set; }
        public string OutputType { get; set; }
        public AisFormRequest FormRequest { get; set; }
        public AisActionRequest ActionRequest { get; set; }
        public int StackId { get; set; }
        public int StateId { get; set; }
        public string Rid { get; set; }
        public bool AllowCache { get; set; }
        public bool ShowActionControls { get; set; }

    }

    public class AisFormRequest
    {
        public AisQuery Query { get; set; }
        public List<AisFormAction> FormActions { get; set; }
        public string MaxPageSize { get; set; } = "1000";
        public string FormServiceAction { get; set; } // C/R/U/D
        public string FormName { get; set; }
        public string Version { get; set; }
        public string FormOID { get; set; }
    }

    public class AisFormAction : IAisFormAction
    {
        /* OPTIONAL
           Allowed Values: [ "SetQBEValue", "SetControlValue", 
               "SetRadioButton", "SetComboValue", "DoAction", 
               "SetCheckboxValue", "SelectRow", "UnSelectRow", 
               "UnSelectAllRows", "SelectAllRows", "ClickGridCell", 
               "ClickGridColumnAggregate", "NextGrid" ]
         */
        public string Command { get; set; }
        public string Value { get; set; }
        public string ControlID { get; set; }
    }

    public class AisActionRequest
    {
        public object[] FormActions { get; set; }
        public string FormOID { get; set; }
        public string ReturnControlIDs { get; set; }
    }

    public interface IAisFormAction { }

    public class AisGridAction : IAisFormAction
    {
        public AisGrid GridAction { get; set; }
    }

    public class AisGrid
    {
        public string GridID { get; set; }
        public List<AisRowUpdate> GridRowUpdateEvents { get; set; }
    }

    public class AisRowUpdate
    {
        public int RowNumber { get; set; }
        public List<AisColumnEvents> GridColumnEvents { get; set; }
    }

    public class AisColumnEvents
    {
        public string Value { get; set; }
        public string Command { get; set; }
        public string ColumnID { get; set; }
    }

    public class AisControl<T>
    {
        public T InternalValue { get; set; }
        public string Value { get; set; }
        public bool Editable { get; set; }
    }

    public class AisErrorException : Exception
    {
        private readonly string _errMessage;
        public string Rid { get; set; }
        public int? StackId { get; set; }
        public int? StateId { get; set; }

        public AisErrorException(string errMessage)
        {
            _errMessage = errMessage;
        }

        public override string Message => _errMessage;
    }
}
