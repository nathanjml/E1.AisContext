using E1Translator.Core.Builders;
using E1Translator.Core.Common;
using System;
using System.Threading.Tasks;
using TurnerTablet.Core.Scaffolding.Features.Ais;
using UnstableSort.Crudless.Mediator;
using static E1Translator.Core.AIS.E1;

namespace E1Translator.Core.AIS
{
    public class AisContext<TRequest, TAisResponse> : BaseAisContext<TRequest, AisResponse<TAisResponse>>
        where TRequest : IRequest<AisResponse<TAisResponse>>
    {
        public bool HandleCloseApp { get; set; } = true;

        public AppStackBuilder GetAppStackBuilder(string formName = ""
            , string version = ""
            , string action = Actions.Open
            , int stackId = 0
            , int stateId = 0
            , string rid = ""
            , string outputType = "VERSION2") => new AppStackBuilder(formName, version, action, stackId, stateId, rid, outputType);

        public DataServiceBuilder GetDataServiceBuilder(string targetName
            , string targetType = null
            , string dataServiceType = "BROWSE") => new DataServiceBuilder(targetName, targetType, dataServiceType);

        public OrchestrationBuilder GetOrchestrationBuilder() => new OrchestrationBuilder();

        public AisContext(IMediator mediator) : base(mediator)
        {

        }

        protected override BaseAisContext<T, AisResponse<R>> Next<T, R>(T request, AisResponse<TAisResponse> response)
        {
            var newContext = new AisContext<T, R>(Mediator);
            if(request.GetType() == typeof(AppStackRequest<>))
            {
                var appStackRequest = request as AppStackRequest<AisResponse<R>>;
                appStackRequest.AisRequest.StackId = response.StackId;
                appStackRequest.AisRequest.StateId = response.StateId;
                appStackRequest.AisRequest.Rid = response.Rid;
                newContext.SetRequest(appStackRequest as T);
            } else
            {
                newContext.SetRequest(request);
            }

            return newContext;
        }

        public async override Task<BaseAisContext<T, AisResponse<R>>> Then<T, R>(Func<AisResponse<TAisResponse>, Task<T>> action) 
        {
            var response = await Mediator.HandleAsync(Request);
            var result2 = await action(response.Result);
            return Next<T, R>(result2, response.Result);
        }

        public async override Task<BaseAisContext<TRequest, AisResponse<TAisResponse>>> Then(Func<AisResponse<TAisResponse>, Task> action)
        {
            var response = await Mediator.HandleAsync(Request);
            await action(response.Result);
            
            if(HandleCloseApp)
            {
                var type = Request.GetType();
                if (type.Name == typeof(AppStackRequest<>).Name)
                {
                    var appStackRequest = Request as AppStackRequest<TAisResponse>;
                    var closeRequest = new CloseAppRequest(appStackRequest.AisRequest.FormName
                        , appStackRequest.AisRequest.Version
                        , Utilities.GetFormOidFromForm(appStackRequest.AisRequest.FormName)
                        , appStackRequest.AisRequest.StackId
                        , appStackRequest.AisRequest.StateId
                        , appStackRequest.AisRequest.Rid);

                    await Mediator.HandleAsync(closeRequest);
                }
            }
            return this;
        }
    }

    public abstract class BaseAisContext<TAisResponse>
    {
        public IMediator Mediator { get; protected set; }

        public BaseAisContext(IMediator mediator)
        {
            Mediator = mediator;
        }
        public abstract Task<BaseAisContext<AisResponse<R>>> Then<T, R>(Func<TAisResponse, Task<T>> action)
            where T : class, IRequest<AisResponse<R>>;

        public abstract Task<BaseAisContext<TAisResponse>> Then(Func<TAisResponse, Task> action);

        protected abstract BaseAisContext<AisResponse<R>> Next<T, R>(T request, TAisResponse resp)
            where T : class, IRequest<AisResponse<R>>;
    }

    public abstract class BaseAisContext<TRequest, TAisResponse>
    {
        public IMediator Mediator { get; protected set; }

        public TRequest Request { get; protected set; }

        public BaseAisContext(IMediator mediator)
        {
            Mediator = mediator;
        }

        public BaseAisContext<TRequest, TAisResponse> SetRequest(TRequest request)
        {
            Request = request;
            return this;
        }

        public abstract Task<BaseAisContext<T, AisResponse<R>>> Then<T, R>(Func<TAisResponse, Task<T>> action)
            where T : class, IRequest<AisResponse<R>>;

        public abstract Task<BaseAisContext<TRequest, TAisResponse>> Then(Func<TAisResponse, Task> action);

        protected abstract BaseAisContext<T, AisResponse<R>> Next<T, R>(T request, TAisResponse resp)
            where T : class, IRequest<AisResponse<R>>;
    }
}
