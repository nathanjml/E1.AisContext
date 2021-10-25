using E1Translator.Core.AIS;
using E1Translator.Core.Builders;
using NSubstitute;
using NUnit.Framework;
using System.Threading.Tasks;
using TurnerTablet.Core.Scaffolding.Features.Ais;
using UnstableSort.Crudless.Mediator;
using static E1Translator.Core.AIS.E1;

namespace E1Translator.UnitTests
{
    [TestFixture]
    public class AisAppStackContextTests : BaseUnitTest
    {
        private IMediator _mediator;

        [SetUp]
        public void Setup()
        {
            _mediator = Substitute.For<IMediator>();
            _mediator.HandleAsync<AisResponse<string>>(Arg.Any<AppStackRequest<string>>()).Returns(x => new AisResponse<string>
            {
                StackId = x.Arg<AppStackRequest<string>>().AisRequest.StackId + 1
                ,
                StateId = x.Arg<AppStackRequest<string>>().AisRequest.StateId + 1
                ,
                Rid = x.Arg<AppStackRequest<string>>().AisRequest.Rid
            }.AsResponse());
        }

        [Test]
        public async Task ContextExecutesSuccessfully()
        {
            AisResponse<string> testResult = null;

            var context = new AisContext
                <AppStackRequest<string>, string>(_mediator);

            var builder = new AppStackBuilder("F_243");
            var aisRequest = builder.ActionRequest("Program", new AisFormAction
            {
                Command = Commands.DoAction,
                ControlID = "13"
            }).Build<string>();

            context.SetRequest(aisRequest);
            await context.Then((result) =>
            {
                testResult = result;
                return Task.CompletedTask;
            });
        }

        [Test]
        public async Task Context_UpdatesAppStackStateVars()
        {
            AisResponse<string> testResult = null;

            var context = new AisContext<AppStackRequest<string>, string>(_mediator);

            var builder = new AppStackBuilder("F_342");
            var aisRequest = builder.ActionRequest("Program", new AisFormAction
            {
                Command = Commands.DoAction,
                ControlID = "13"
            }).Build<string>();
            aisRequest.AisRequest.StackId = 1;
            aisRequest.AisRequest.StateId = 1;
            aisRequest.AisRequest.Rid = "Test1";

            context.SetRequest(aisRequest);
            await context.Then((result) =>
            {
                testResult = result;
                return Task.CompletedTask;
            });

            Assert.IsTrue(testResult.StackId == 2);
            Assert.IsTrue(testResult.StateId == 2);
        }

        [Test]
        public async Task Context_Automatically_CloseRequest()
        {
            AisResponse<string> testResult = null;

            var context = new AisContext<AppStackRequest<string>, string>(_mediator);

            var builder = new AppStackBuilder("F_342");
            var aisRequest = builder.ActionRequest("Program", new AisFormAction
            {
                Command = Commands.DoAction,
                ControlID = "13"
            }).Build<string>();
            aisRequest.AisRequest.StackId = 1;
            aisRequest.AisRequest.StateId = 1;
            aisRequest.AisRequest.Rid = "Test1";

            context.SetRequest(aisRequest);
            var newcontext = await context.Then((result) =>
            {
                testResult = result;
                return Task.CompletedTask;
            });


            Assert.IsTrue(testResult.StackId == 2);
            Assert.IsTrue(testResult.StateId == 2);
            _mediator.Received().HandleAsync<AisResponse<NoResponse>>(Arg.Any<IRequest<AisResponse<NoResponse>>>());
        }

        [Test]
        public async Task Context_ChainMultipleRequests_DoesNotClose()
        {
            AisResponse<string> testResult = null;

            var context = new AisContext<AppStackRequest<string>, string>(_mediator);

            var builder = new AppStackBuilder("F_342");
            var aisRequest = builder.ActionRequest("Program", new AisFormAction
            {
                Command = Commands.DoAction,
                ControlID = "13"
            }).Build<string>();
            aisRequest.AisRequest.StackId = 1;
            aisRequest.AisRequest.StateId = 1;
            aisRequest.AisRequest.Rid = "Test1";

            context.SetRequest(aisRequest);
            var newContext = await context.Then<AppStackRequest<string>, string>(async (result) =>
            {
                testResult = result;
                var request = new AppStackBuilder("P_343").ActionRequest("Program", new AisFormAction
                {
                    Command = Commands.DoAction,
                    ControlID = "100"
                }).Build<string>();

                return request;
            });

            _mediator.DidNotReceive().HandleAsync<AisResponse<NoResponse>>(Arg.Any<IRequest<AisResponse<NoResponse>>>());

            await newContext.Then((res) => Task.CompletedTask );

            _mediator.Received().HandleAsync<AisResponse<NoResponse>>(Arg.Any<IRequest<AisResponse<NoResponse>>>());
        }

        [Test]
        public async Task SimpleInjector_Resolves_AisContext()
        {
            var instance = Container.GetInstance<AisContext<AppStackRequest<string>, string>>();
            Assert.NotNull(instance);
        }
    }
}
