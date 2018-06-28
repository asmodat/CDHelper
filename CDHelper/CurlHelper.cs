using System;
using System.Threading.Tasks;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using System.Net.Http;
using AsmodatStandard.Types;

namespace CDHelper
{
    public static class CurlHelper
    {
        public static async Task AwaitSuccessCurlGET(string uri, int timeout, int intensity = 1000)
        {
            if (uri.IsNullOrEmpty())
                throw new ArgumentException($"{nameof(uri)} can't be null or empty.");

            var tt = new TickTimeout(timeout, TickTime.Unit.ms);
            HttpResponseMessage lastResponse = null;
            Exception lastException = null;
            do
            {
                try
                {
                    var result = (await HttpHelper.CURL(HttpMethod.Get, uri, null));
                    lastResponse = result.Response;

                    if (lastResponse.StatusCode == System.Net.HttpStatusCode.OK)
                        return;
                }
                catch(Exception ex)
                {
                    lastException = ex;
                }

                if (!tt.IsTriggered)
                    await Task.Delay(intensity);

            } while (!tt.IsTriggered);

            throw new Exception($"AwaitSuccessCurlGET, span: {(int)tt.Span}/{timeout} [ms], status code: '{lastResponse?.StatusCode}', response: '{lastResponse?.Content?.ReadAsStringAsync()}'", lastException);
        }
    }
}
