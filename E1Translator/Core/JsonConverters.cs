using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace E1Translator.Core
{
    public class AisJsonConverter<T> : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType
                ? objectType.GetGenericTypeDefinition() == typeof(AisResponse<>)
                : false;
        }

        public override object ReadJson(JsonReader reader
            , Type objectType
            , object existingValue
            , JsonSerializer serializer)
        {
            var json = JObject.Load(reader);
            var sysErrors = json.Value<JArray>("sysErrors");
            if (sysErrors != null && sysErrors.HasValues)
            {
                return new AisResponse<T>
                {
                    SysErrors = sysErrors.ToObject<List<AisErrorMessage>>()
                };
            }

            var currentApp = json.Value<string>("currentApp");
            var links = json.Value<JArray>("links")?
                .ToObject<List<AisLink>>();

            var property = json.Properties().First(p => p.Name.Contains("fs_"));
            var dataBrowser = serializer.Deserialize<AisDataBrowser<T>>(
                json[property.Name].CreateReader());

            return new AisResponse<T>
            {
                DataBrowser = dataBrowser,
                CurrentApp = currentApp,
                Links = links,
                StackId = json.Value<int>("stackId"),
                StateId = json.Value<int>("stateId"),
                TimeStamp = json.Value<string>("timeStamp"),
                Rid = json.Value<string>("rid"),
                SysErrors = new List<AisErrorMessage>()
            };
        }

        public override void WriteJson(JsonWriter writer
            , object value
            , JsonSerializer serializer)
        {
            throw new InvalidOperationException("This converter currently only supports deserializing requests");
        }
    }

    public class AisDataJsonConverter<T> : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType
                ? objectType.GetGenericTypeDefinition() == typeof(AisData<>)
                : false;
        }

        public override object ReadJson(JsonReader reader
            , Type objectType
            , object existingValue
            , JsonSerializer serializer)
        {

            var json = JObject.Load(reader);
            var relatedData = new Dictionary<string, AisDataItem>();

            foreach (var o in json)
            {
                if (o.Key.ToUpper() != "GRIDDATA")
                {
                    var val = serializer.Deserialize<AisDataItem>(o.Value.CreateReader());
                    relatedData.Add(o.Key, val);
                }
            }

            return new AisData<T>
            {
                GridData = json.Value<JObject>("gridData") != null
                    ? serializer.Deserialize<AisGridData<T>>(json["gridData"].CreateReader())
                    : new AisGridData<T>(),
                RelatedData = relatedData
            };
        }

        public override void WriteJson(JsonWriter writer
            , object value
            , JsonSerializer serializer)
        {
            throw new InvalidOperationException("This converter currently only supports deserializing requests");
        }
    }

    public class OrchestrationJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(OrchestrationResponse);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var json = JObject.Load(reader);
            var data = new Dictionary<string, string>();

            JArray errors = null;
            IList<IDictionary<string, string>> rows = null;

            foreach (var o in json)
            {
                switch (o.Key.ToUpper())
                {
                    case "ERRORS/WARNINGS":
                        errors = json.Value<JArray>("Errors/Warnings");
                        break;
                    case "GRIDS":
                        rows = json
                            .Value<JArray>("Grids")[0]["Row Set"]
                            .ToObject<IList<IDictionary<string, string>>>();
                        break;
                    default:
                        data.Add(o.Key, o.Value.ToString());
                        break;
                }

            }

            return new OrchestrationResponse
            {
                Data = data,
                GridRows = rows,
                Errors = errors?.ToObject<List<OrchestrationErrorResponse>>().FirstOrDefault()
            };
        }

        public override void WriteJson(JsonWriter writer
            , object value
            , JsonSerializer serializer)
        {
            throw new InvalidOperationException("This converter currently only supports deserializing requests");
        }
    }
}
