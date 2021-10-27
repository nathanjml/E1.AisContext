using E1Translator.Core.Builders;
using System.Threading;
using System.Threading.Tasks;
using TurnerTablet.Core.Scaffolding.Features.Ais;
using UnstableSort.Crudless.Integration.EntityFrameworkCore.Transactions;
using UnstableSort.Crudless.Mediator;
using static E1Translator.Core.AIS.E1;

namespace E1Translator.Core.Common
{
    [NoTransaction]
    [DoNotValidate]
    public class CloseAppRequest : IRequest<AisResponse<NoResponse>>
    {
        public string Form { get; set; }
        public string Version { get; set; }
        public string FormOid { get; set; }
        public int StackId { get; set; }
        public int StateId { get; set; }
        public string Rid { get; set; }

        public CloseAppRequest(string form, string version, string formOid
            , int stackId, int stateId, string rid)
        {
            Form = form;
            Version = version;
            FormOid = formOid;
            StackId = stackId;
            StateId = stateId;
            Rid = rid;
        }
    }

    public class CloseAppRequestHandler : IRequestHandler<CloseAppRequest, AisResponse<NoResponse>>
    {
        private readonly IMediator _mediator;

        public CloseAppRequestHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Response<AisResponse<NoResponse>>> HandleAsync(CloseAppRequest request, CancellationToken ct)
        {
            var builder = new AppStackBuilder(request.Form,
                request.Version,
                Actions.Close,
                request.StackId,
                request.StateId,
                request.Rid);

            var aisRequest = builder
                .ActionRequest(request.FormOid)
                .Build();

            var response = await _mediator.HandleAsync(
                new AppStackRequest<NoResponse>
                {
                    AisRequest = aisRequest
                });

            return response;
        }
    }
}
