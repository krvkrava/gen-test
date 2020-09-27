using System;
using System.Text;
using System.Threading.Tasks;
using Modules.CoroutineRunner;
using Modules.RequestProcessing.Abstractions;
using Modules.RequestProcessing.Data;
using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using Smooth.Slinq;
using Strx.Expansions.Extensions.Algebraic;
using Strx.Expansions.Modules.RequestProcessing.Data;
using Strx.Expansions.Modules.RequestProcessing.Manifests;
using Strx.Expansions.Modules.RequestProcessing.Middlewares;
using UnityEditor.CrashReporting;
using UnityEngine;
using UnityEngine.Networking;

namespace Modules.RequestProcessing.Middlewares
{
    /// <summary>
    /// Unity's implementation of request processor.
    /// TheMust: all the UnityWebRequests or download handlers must be disposed 
    /// </summary>
    public class UnityWebRequestProcessorMiddleware<T> : RequestProcessorMiddleware<T>,
        IUnityNetworkMiddlewareSchema where T : IUnityNetworkMiddlewareSchema
    {
        private ICoroutineRunner _coroutineRunner;

        public UnityWebRequestProcessorMiddleware(Option<T> nextMiddleware, RequestProcessorMiddlewareManifest manifest)
            : base(nextMiddleware, manifest)
        {
        }

        public static UnityWebRequestProcessorMiddleware<T> Create(Option<T> nextMiddleware,
            ICoroutineRunner runner, RequestProcessorMiddlewareManifest manifest)
        {
            var middleware = new UnityWebRequestProcessorMiddleware<T>(nextMiddleware, manifest);
            middleware._coroutineRunner = runner;

            return middleware;
        }

        public override async Task<Result<ResponseData>> Request(RequestData requestData)
        {
            var webRequestResult = CreateRequestWithBasicData(requestData);
            if (webRequestResult.IsError)
                return webRequestResult.ConvertErrorTo<ResponseData>("Cannot create request");

            var webRequest = webRequestResult.Value;
            
            CreateFilledUploadHandlerIfNeed(webRequest, requestData);
            var buffer = new DownloadHandlerBuffer();
            webRequest.downloadHandler = buffer;

            var responseDataResult = await ProcessRequest<ResponseData>(webRequest, requestData)
                .SpecifyAsync(() => "Cannot process request");

            if (responseDataResult.IsError)
                return responseDataResult.ConvertErrorTo<ResponseData>("Cannot process request");

            responseDataResult.Value
                .SetRawText(webRequest.downloadHandler.text);

            webRequest.Dispose();

            return responseDataResult;
        }

        public async Task<Result<ResponseBundleData>> RequestBundle(RequestData requestData)
        {
            var webRequestResult = CreateRequestWithBasicData(requestData);
            if (webRequestResult.IsError)
                return webRequestResult.ConvertErrorTo<ResponseBundleData>("Cannot create reqeust");

            var webRequest = webRequestResult.Value;
            
            CreateFilledUploadHandlerIfNeed(webRequest, requestData);
            
            //Using url from web request as url was created above at CreateRequestWithBasicData 
            var handler = new DownloadHandlerAssetBundle(webRequest.url, 0); //0 - ignore check sum validation
            webRequest.downloadHandler = handler;

            var responseDataResult = await ProcessRequest<ResponseBundleData>(webRequest, requestData);
            if (responseDataResult.IsError)
                return responseDataResult.ConvertErrorTo<ResponseBundleData>("Cannot process request");
            
            responseDataResult.Value
                .SetAssetBundle(handler.assetBundle)
                .SetRawData(handler.data);

            webRequest.Dispose();

            return responseDataResult;
        }

        private Result<Unit> ChangeCertificateHandlerIfNeed(UnityWebRequest webRequest)
        {
            if(Manifest.Protocol != "http" && Manifest.Protocol != "https")
                return Result<Unit>.FromError($"Not supported protocol: {Manifest.Protocol}");
            
            if (Manifest.Protocol == "http")
                webRequest.certificateHandler = new HttpMockCertificateHandler();

            return Unit.Default.ToValue();
        }

        private Result<UnityWebRequest> CreateRequestWithBasicData(RequestData requestData)
        {
            var webRequest = new UnityWebRequest();
            var urlResult = CreateUrl(requestData);
            if (urlResult.IsError)
                return urlResult.ConvertErrorTo<UnityWebRequest>();
            
            webRequest.uri = urlResult.Value;
            webRequest.method = requestData.Method.ToString();
            requestData.Timeout.Slinq().ForEach(webRequest, (time, request) => request.timeout = time);
            requestData.Headers.Slinq().ForEach(webRequest, (headers, request)
                => headers.Slinq().ForEach(request, (header, _request)
                    => _request.SetRequestHeader(header.Key, header.Value))); //same as foreach many

            var certificateChangeResult = ChangeCertificateHandlerIfNeed(webRequest);
            if (certificateChangeResult.IsError)
                return certificateChangeResult.ConvertErrorTo<UnityWebRequest>();
            
            return webRequest.ToValue();
        }

        private Result<Uri> CreateUrl(RequestData requestData)
        {
            if(requestData.UriOrLink.isNone)
             return Result<Uri>.FromError($"Cannot get url from: {requestData} as it's empty at {nameof(UnityWebRequestProcessorMiddleware<T>)}");

            var urlOrLink = requestData.UriOrLink.value;
            if (urlOrLink.isLeft)
                return urlOrLink.leftValue.ToValue();
            
            var url = $"{Manifest.Protocol}://{Manifest.HostName}/{urlOrLink.rightValue}";

            try
            {
                return new Uri(url).ToValue();
            }
            catch (Exception e)
            {
                return Result<Uri>.FromError($"Cannot create Url instance from url string: {url}");
            }
        }

        private void CreateFilledUploadHandlerIfNeed(UnityWebRequest webRequest, RequestData data)
        {
            if (data.Body.isNone)
                return;

            byte[] bytes = Encoding.ASCII.GetBytes(data.Body.value);
            var uploadHandler = new UploadHandlerRaw(bytes);

            webRequest.uploadHandler = uploadHandler;
        }

        private async Task<Result<T>> ProcessRequest<T>(UnityWebRequest webRequest, RequestData requestData)
            where T : ResponseData, new()
        {
            var asyncOperation = (AsyncOperation) webRequest.SendWebRequest();

            await _coroutineRunner.RunWithProgressAsync(asyncOperation,
                () => webRequest.downloadProgress,
                progress => requestData.ProgressCallback.ForEach(callback => callback(progress)));

            var validationResult = ValidateProcessedRequest(webRequest);
            if (validationResult.IsError)
                return validationResult.ConvertErrorTo<T>("Unable to validate response result");

            var responseData = new T();
            var filledResponseResult = FillResponseWithData(webRequest, responseData);
            if (filledResponseResult.IsError)
                return filledResponseResult.ConvertErrorTo<T>("Cannot fill response data");

            return filledResponseResult.Value.ToValue();
        }

        private Result<Unit> ValidateProcessedRequest(UnityWebRequest webRequest)
        {
            //TODO add to webRequest data boolean variable - ProcessStatusCode - if true, then status code validation will be processed

            //isNetworkError - Examples of system errors include failure to resolve a DNS entry,
            //a socket error or a redirect limit being exceeded
            if (webRequest.isNetworkError)
            {
                return Result<Unit>.FromError($"Network error was received. Error: {webRequest.error}." +
                                              $" StatusCode: {webRequest.responseCode}");
            }

            //NO NEED to check status code here as it is processed on StatusCodeValidationMiddleware

            if (!webRequest.isDone)
                return Result<Unit>.FromError("Unexpected behaviour. Request is not processed");

            return Unit.Default.ToValue();
        }

        private Result<T> FillResponseWithData<T>(UnityWebRequest request, T objectToFill) where T : ResponseData
        {
            objectToFill.SetStatusCode((int) request.responseCode);
            if (!string.IsNullOrEmpty(request.error))
                objectToFill.SetError(request.error);

            return objectToFill.ToValue();
        }
    }
}