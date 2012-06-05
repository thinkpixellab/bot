using System;
using System.Net;
using System.Diagnostics;
#if WP7
using Microsoft.Phone.Reactive;
#else
using System.Linq;
#endif

namespace PixelLab.Common
{
    public static class WebHelpers
    {
        public static IObservable<WebResponse> GetResponseAsync(this Uri requestUri, string userAgent = null)
        {
            var webRequest = WebRequest.Create(requestUri);
            if (userAgent != null)
            {
                var httpRequest = webRequest as HttpWebRequest;
                if (httpRequest == null)
                {
                    throw new ArgumentException("Cannot set 'userAgent' for non-http requests.");
                }
                else
                {
                    httpRequest.UserAgent = userAgent;
                }
            }

            var wrappedBeginGetResponse = new Func<AsyncCallback, Object, IAsyncResult>((callback, state) =>
            {
                Debug.WriteLine("Requesting   : {0}", requestUri);
                return webRequest.BeginGetResponse(callback, state);
            });

            var wrappedEndGetResponse = new Func<IAsyncResult, WebResponse>(result =>
            {
                try
                {
                    var response = webRequest.EndGetResponse(result);
                    Debug.WriteLine("Response from: {0}", response.ResponseUri);
                    return response;
                }
                catch (WebException webException)
                {
                    Debug.WriteLine("Error requesting '{0}'. Error: {1}", webRequest.RequestUri, webException);
                    return null;
                }
            });
            return Observable
              .Defer(Observable.FromAsyncPattern<WebResponse>(wrappedBeginGetResponse, wrappedEndGetResponse))
              .Where(response => response != null);
        }

        public static IObservable<string> GetResponseAsStringAsync(this Uri requestUri, string userAgent = null)
        {
            return from response in requestUri.GetResponseAsync(userAgent)
                   select response.UseAndDispose(ReadAsString);
        }

        public static IObservable<byte[]> GetResponseAsBytesAsync(this Uri requestUri, string userAgent = null)
        {
            return from response in requestUri.GetResponseAsync(userAgent)
                   select response.UseAndDispose(ReadAsBytes);
        }

        public static string ReadAsString(this WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                return stream.ReadAllAsString();
            }
        }

        public static byte[] ReadAsBytes(this WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                return stream.ReadAllAsBytes();
            }
        }
    }
}
