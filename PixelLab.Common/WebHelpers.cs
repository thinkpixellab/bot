using System;
using System.Linq;
using System.Net;
#if WP7
using Microsoft.Phone.Reactive;
#endif

namespace PixelLab.Common {
  public static class WebHelpers {
    public static IObservable<string> GetResponseAsStringAsync(this WebRequest webRequest) {
      return from response in webRequest.GetResponseAsync() select response.ReadAsString();
    }

    public static IObservable<byte[]> GetResponseAsBytesAsync(this WebRequest webRequest) {
      return from response in webRequest.GetResponseAsync() select response.ReadAsBytes();
    }

    public static IObservable<WebResponse> GetResponseAsync(this WebRequest webRequest) {
      return Observable.FromAsyncPattern<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse)();
    }

    public static string ReadAsString(this WebResponse response) {
#if DEBUG
      System.Diagnostics.Debug.WriteLine("Reading URI: {0}", ((HttpWebResponse)response).ResponseUri);
#endif
      using (var stream = response.GetResponseStream()) {
        return stream.ReadAllAsString();
      }
    }

    public static byte[] ReadAsBytes(this WebResponse response) {
#if DEBUG
      System.Diagnostics.Debug.WriteLine("Reading URI: {0}", ((HttpWebResponse)response).ResponseUri);
#endif
      using (var stream = response.GetResponseStream()) {
        return stream.ReadAllAsBytes();
      }
    }


  }
}
