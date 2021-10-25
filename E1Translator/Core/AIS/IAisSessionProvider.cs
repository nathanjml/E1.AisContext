using E1Translator.Core.AIS.Auth;
using E1Translator.Core.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnstableSort.Crudless.Mediator;

namespace E1Translator.Core.AIS
{
    public class AisSessionInfo
    {
        public string Token { get; set; }
        public string DeviceName { get; set; }
    }

    public interface IAisSessionProvider
    {
        Task<AisSessionInfo> GetSession();
    }

    public class DefaultAisSessionProvider : IAisSessionProvider
    {
        private readonly IAISConfiguration _settings;
        private readonly IMediator _mediator;
        private AisSessionInfo _session;

        public DefaultAisSessionProvider(IAISConfiguration settings, IMediator mediator)
        {
            _settings = settings;
            _mediator = mediator;
        }

        public async Task<AisSessionInfo> GetSession()
        {
            var isTokenValid = await ValidateToken();

            if(!isTokenValid)
            {
                var tokenResponse = await _mediator.HandleAsync(new AisTokenRequest
                {
                    Username = _settings.AisUsername,
                    Password = _settings.AisPassword
                });

                _session = tokenResponse.Result;
            }

            return _session;
        }

        private async Task<bool> ValidateToken()
        {
            if (_session == null) return false;

            var validationResponse = await _mediator.HandleAsync(
                new AisTokenValidationRequest
                {
                    Token = _session.Token
                });

            return (!validationResponse.HasErrors && validationResponse.Result);
        }
    }
}
