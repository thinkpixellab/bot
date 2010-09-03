using System;
using System.Net;
using System.Diagnostics;
#if WP7
using Microsoft.Phone.Reactive;
#else
using System.Linq;
#endif

namespace PixelLab.Common {
  public static class WebHelpers {
    public static IObservable<WebResponse> GetResponseAsync(this Uri requestUri, string userAgent = null) {
      var webRequest = WebRequest.Create(requestUri);
      if (userAgent != null) {
        var httpRequest = webRequest as HttpWebRequest;
        if (httpRequest == null) {
          throw new ArgumentException("Cannot set 'userAgent' for non-http requests.");
        }
        else {
          httpRequest.UserAgent = userAgent;
        }
      }

      var wrappedEndGetResponse = new Func<IAsyncResult, WebResponse>(result => {
        try {
          return webRequest.EndGetResponse(result);
        }
        catch (WebException webException) {
          Debug.WriteLine("Error requesting '{0}'. Error: {1}", webRequest.RequestUri, webException);
          return null;
        }
      });
      return Observable
        .Defer(Observable.FromAsyncPattern<WebResponse>(webRequest.BeginGetResponse, wrappedEndGetResponse))
        .Where(response => response != null);
    }

    public static IObservable<string> GetResponseAsStringAsync(this Uri requestUri, string userAgent = null) {
      return from response in requestUri.GetResponseAsync(userAgent)
             select response.UseAndDispose(ReadAsString);
    }

    public static IObservable<byte[]> GetResponseAsBytesAsync(this Uri requestUri, string userAgent = null) {
      return from response in requestUri.GetResponseAsync(userAgent)
             select response.UseAndDispose(ReadAsBytes);
    }

    public static string ReadAsString(this WebResponse response) {
#if DEBUG
      var http = response as HttpWebResponse;
      if (http != null) {
        Debug.WriteLine(http.ResponseUri);
      }
#endif
      using (var stream = response.GetResponseStream()) {
        return stream.ReadAllAsString();
      }
    }

    public static byte[] ReadAsBytes(this WebResponse response) {
      using (var stream = response.GetResponseStream()) {
        return stream.ReadAllAsBytes();
      }
    }

    public static TResponse UseAndDispose<T, TResponse>(this T source, Func<T, TResponse> func) where T : IDisposable {
      using (source) {
        return func(source);
      }
    }
  }
}
