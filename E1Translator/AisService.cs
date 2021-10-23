using System.Threading.Tasks;

namespace E1Translator
{
    public interface E1Interface
    {
        public Task<AisResponse<T>> Query<T>(string e1Endpoint, params object[] parameters);

    }

    public class AisService : E1Interface
    {
        public Task<AisResponse<T>> Query<T>(string e1Endpoint, params object[] parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}
