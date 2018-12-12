using System;
using System.Threading.Tasks;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using System.Net.Http;
using System.Diagnostics;

namespace CDHelper
{
    public static class CurlHelper
    {
        public static async Task<HttpResponseMessage> AwaitSuccessCurlGET(
            string uri, 
            int timeout = 60 * 1000, 
            int intensity = 1000, 
            int requestTimeout = 60 * 1000)
        {
            if (uri.IsNullOrEmpty())
                throw new ArgumentException($"{nameof(uri)} can't be null or empty.");

            if (timeout < requestTimeout)
                throw new ArgumentException($"timeout {timeout} must be smaller then requestTimeout {requestTimeout}");

            if (requestTimeout <= 0)
                throw new ArgumentException($"requestTimeout must be greater then 0");

            var sw = Stopwatch.StartNew();
            HttpResponseMessage lastResponse = null;
            Exception lastException = null;
            do
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(Math.Min(requestTimeout, timeout));

                        var result = (await client.CURL(HttpMethod.Get, uri, null));
                        lastResponse = result.Response;

                        if (lastResponse.StatusCode == System.Net.HttpStatusCode.OK)
                            return lastResponse;
                    }
                }
                catch(Exception ex)
                {
                    lastException = ex;
                }

                if (sw.ElapsedMilliseconds < (timeout - intensity))
                    await Task.Delay(intensity);
                else
                    break;

            } while (true);

            throw new Exception($"AwaitSuccessCurlGET, span: {(int)sw.ElapsedMilliseconds}/{timeout} [ms], status code: '{lastResponse?.StatusCode}', response: '{lastResponse?.Content?.ReadAsStringAsync()}'", lastException);
        }
    }
}
