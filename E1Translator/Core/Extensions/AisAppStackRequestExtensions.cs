using System;
using System.Collections.Generic;
using System.Text;
using TurnerTablet.Core.Scaffolding.Features.Ais;

namespace E1Translator.Core.Extensions
{
    public static class AisAppStackRequestExtensions
    {
        public static AppStackRequest<T> ToAppStackRequest<T>(this AisAppStackRequest request)
        {
            return new AppStackRequest<T> { AisRequest = request };
        }
    }
}
