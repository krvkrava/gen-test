using System.Threading.Tasks;
using Modules.RequestProcessing.Abstractions;
using Modules.RequestProcessing.Data;
using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using Strx.Expansions.Extensions.Algebraic;
using Strx.Expansions.Modules.RequestProcessing.Data;
using Strx.Expansions.Patterns.Structural.Middleware;

namespace Modules.RequestProcessing.Middlewares
{
    public class StatusCodeValidationMiddleware<T> : Middleware<T>,
        IUnityNetworkMiddlewareSchema where T : IUnityNetworkMiddlewareSchema
    {
        public static StatusCodeValidationMiddleware<T> Create(Option<T> nextMiddleware)
        {
            var middleware = new StatusCodeValidationMiddleware<T>
            {
                NextMiddleware = nextMiddleware
            };

            return middleware;
        }

        public async Task<Result<ResponseData>> Request(RequestData data)
        {
            var result = await NextMiddleware.value.Request(data);
            return result.ThenProceed((data, this),
                    (response, context) => context.Item2.ValidateIfHttpError(context.data, response));
        }

        public async Task<Result<ResponseBundleData>> RequestBundle(RequestData requestData)
        {
            var result = await NextMiddleware.value.RequestBundle(requestData);
            return result.ThenProceed((requestData, this),
                (response, context) => context.Item2.ValidateIfHttpError(context.requestData, response));
        }

        private Result<Unit> ValidateIfHttpError(RequestData requestData, ResponseData response)
        {
            var statusCode = response.StatusCode;
            if (statusCode < 100 || statusCode > 500)
                return Result.FromError($"Unexpected status code: {statusCode} at :{requestData.Path}");

            if (response.IsClientError || response.IsServerError)
                return Result.FromError($"An HTTP error was received for: {requestData.Path}. " +
                                        $"Error: {response.Error.ValueOr((string) null)}. " +
                                        $"StatusCode: {statusCode}");

            return Unit.Default.ToValue();
        }
    }
}