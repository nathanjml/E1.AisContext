using E1Translator.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TurnerTablet.Core.Scaffolding.Features.Ais;
using static E1Translator.Core.AIS.E1;

namespace E1Translator.Core.Builders
{

    public enum Conjunction
    {
        And,
        Or
    }

    public abstract class AisRequestBuilder<TBuilder, TReq>
        where TReq : AisRequest, new()
        where TBuilder : AisRequestBuilder<TBuilder, TReq>
    {
        protected readonly ImmutableList<Action<TReq>> _builders
            = ImmutableList<Action<TReq>>.Empty;

        protected abstract TBuilder Add(Action<TReq> fn);

        protected AisRequestBuilder() { }

        protected AisRequestBuilder(ImmutableList<Action<TReq>> builders)
        {
            _builders = builders;
        }

        protected AisRequestBuilder(Action<TReq> init)
        {
            _builders = _builders.Add(init);
        }

        public TReq Build()
        {
            var result = new TReq();
            foreach (var fn in _builders)
            {
                fn(result);
            }

            return result;
        }
    }

    public class DataServiceBuilder
        : AisRequestBuilder<DataServiceBuilder, AisDataServiceRequest>
    {
        public DataServiceBuilder(string targetName
            , string targetType = null
            , string dataServiceType = "BROWSE")
            : base(x =>
            {
                x.TargetName = targetName;
                x.TargetType = targetType ?? GetTargetType(targetName);
                x.DataServiceType = dataServiceType;
                x.MaxPageSize = "1000";
                x.EnableNextPageProcessing = true;
            })
        {
        }

        protected DataServiceBuilder(ImmutableList<Action<AisDataServiceRequest>> builders)
            : base(builders)
        {
        }

        protected override DataServiceBuilder Add(Action<AisDataServiceRequest> ac)
        {
            return new DataServiceBuilder(_builders.Add(ac));
        }

        public DataServiceBuilder MaxPageSize(int maxPageSize)
        {
            return Add(x => x.MaxPageSize = maxPageSize.ToString());
        }

        public DataServiceBuilder EnableNextPageProcessing(bool enabled)
        {
            return Add(x => x.EnableNextPageProcessing = enabled);
        }

        public DataServiceBuilder ReturnsControlIDS(params string[] ctrls)
        {
            return Add(x => x.ReturnControlIDs = String.Join("|", ctrls));
        }

        public DataServiceBuilder Query(bool autoFind
            , params AisCondition[] conditions)
        {
            return Add(x => x.Query = new AisQuery
            {
                AutoFind = autoFind,
                Condition = conditions
            });
        }

        public DataServiceBuilder Query(bool autoFind
            , Action<AisQuery> builder)
        {
            var query = new AisQuery
            {
                AutoFind = autoFind,
            };

            builder(query);

            return Add(x => x.Query = query);
        }

        public DataServiceBuilder OrderBy(params AisOrderBy[] orderings)
        {
            return Add(x => x.Aggregation = new AisAggregation
            {
                OrderBy = orderings
            });
        }

        private static string GetTargetType(string targetName)
        {
            switch (targetName[0])
            {
                case 'V':
                    return "view";
                default:
                case 'F':
                    return "table";
            }
        }
    }

    public enum FormActionType
    {
        Create,
        Read,
        Update,
        Delete
    }

    public class AppStackBuilder
        : AisRequestBuilder<AppStackBuilder, AisAppStackRequest>
    {
        private readonly string _formName;
        private readonly string _formVersion;

        public AppStackBuilder(string formName = ""
            , string version = ""
            , string action = Actions.Open
            , int stackId = 0
            , int stateId = 0
            , string rid = ""
            , string outputType = "VERSION2")

            : base(x =>
            {
                x.FormName = formName;
                x.Version = version;
                x.Action = action;
                x.OutputType = outputType;
                x.StackId = stackId;
                x.StateId = stateId;
                x.Rid = rid;
            })
        {
            _formName = formName;
            _formVersion = version;
        }

        protected AppStackBuilder(string formName
            , string version
            , ImmutableList<Action<AisAppStackRequest>> builders)
            : base(builders)
        {
            _formName = formName;
            _formVersion = version;
        }

        public AppStackBuilder AllowCache(bool cache = true)
        {
            return Add(x => x.AllowCache = cache);
        }

        protected override AppStackBuilder Add(Action<AisAppStackRequest> ac)
        {
            return new AppStackBuilder(_formName, _formVersion, _builders.Add(ac));
        }

        public AppStackBuilder ReturnsControlIDS(params string[] ctrls)
        {
            return Add(x => x.ReturnControlIDs = String.Join("|", ctrls));
        }

        public AppStackBuilder FormRequest(FormActionType actionType
            , params AisFormAction[] formActions)
        {
            return Add(x => x.FormRequest = new AisFormRequest
            {
                FormName = _formName,
                FormActions = formActions.ToList(),
                FormServiceAction = GetFormAction(actionType),
                Version = _formVersion
            });
        }

        public AppStackBuilder ActionRequest(string formOID, params IAisFormAction[] formActions)
        {
            return Add(x => x.ActionRequest = new AisActionRequest
            {
                FormActions = formActions,
                FormOID = formOID
            });
        }

        public AppStackBuilder ActionRequest(string formOID
            , string returnControlIDs
            , params object[] formActions)
        {
            return Add(x => x.ActionRequest = new AisActionRequest
            {
                FormOID = formOID,
                ReturnControlIDs = returnControlIDs,
                FormActions = formActions,
            });
        }

        public AppStackBuilder ShowActionControls(bool showActionControls = true)
        {
            return Add(x => x.ShowActionControls = showActionControls);
        }

        private static string GetFormAction(FormActionType action)
        {
            switch (action)
            {
                case FormActionType.Create: return "C";
                case FormActionType.Read: return "R";
                case FormActionType.Update: return "U";
                case FormActionType.Delete: return "D";
                default: throw new ArgumentException("Invalid form action type");
            }
        }

        private static string GetTargetType(string targetName)
        {
            switch (targetName[0])
            {
                case 'V':
                    return "view";
                default:
                case 'F':
                    return "table";
            }
        }

        public AppStackRequest<T> Build<T>()
        {
            var result = base.Build();
            return result.ToAppStackRequest<T>();
        }
    }

    public class OrchestrationBuilder
    {
        protected readonly ImmutableList<Action<OrchRequest>> _builders
            = ImmutableList<Action<OrchRequest>>.Empty;

        protected OrchestrationBuilder(ImmutableList<Action<OrchRequest>> builders)
        {
            _builders = builders;
        }

        public OrchestrationBuilder()
        {
        }

        public OrchestrationBuilder AddInput(string name, string value)
        {
            if (value == null) return this;

            return new OrchestrationBuilder(_builders.Add(x => x.Inputs.Add(new Input
            {
                Name = name,
                Value = value
            })));
        }

        public OrchestrationBuilder AddDetailInput(string name, List<RepeatingInput> repeatingInputs)
        {
            return new OrchestrationBuilder(_builders.Add(x => x.DetailInputs.Add(new DetailInput
            {
                Name = name,
                RepeatingInputs = repeatingInputs
            })));
        }

        public OrchRequest Build()
        {
            var result = new OrchRequest();
            foreach (var fn in _builders)
            {
                fn(result);
            }

            return result;
        }
    }
}
