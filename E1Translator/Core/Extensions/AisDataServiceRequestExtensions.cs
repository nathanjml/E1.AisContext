using E1Translator;
using E1Translator.Core.AIS;

namespace E1AisSender.Core.Extensions
{
    public static class AisDataServiceRequestExtensions
    {
        public static DataServiceRequest<T> ToDataServiceRequest<T>(this AisDataServiceRequest dataServiceRequest)
            => new DataServiceRequest<T> {AisRequest = dataServiceRequest};
    }
}
