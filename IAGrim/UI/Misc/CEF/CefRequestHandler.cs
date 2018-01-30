using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace IAGrim.UI.Misc.CEF {
    class CefRequestHandler : IRequestHandler {

        public event EventHandler TransferSingleRequested;
        public event EventHandler TransferAllRequested;
        public event EventHandler OnSetZoom;

        public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect) {
            if (request.Url.StartsWith("http://")) {
                Process.Start(request.Url);
                return true;
            }

            if (request.Url.StartsWith("relics://")) {
                Process.Start("http://items.dreamcrash.org/ComponentAssembler?record=d106_relic.dbr ");
                return true;
            }

            if (request.Url.StartsWith("setzoom://")) {
                double val;
                double.TryParse(request.Url.Replace("setzoom://", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out val);
                OnSetZoom?.Invoke(this, new SetZoomEvent {
                    ZoomLevel = val
                });
                return true;
            }

            if (request.Url.StartsWith("transfer://single/")) {
                TransferSingleRequested?.Invoke(this, new ItemTransferEvent {
                    Request = request.Url.Replace("transfer://single/", "")
                });
                return true;
            }

            if (request.Url.StartsWith("transfer://all/")) {
                TransferAllRequested?.Invoke(this, new ItemTransferEvent {
                    Request = request.Url.Replace("transfer://all/", "")
                });
                return true;
            }

            return request.Url.StartsWith("http:");
        }

        public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl,
            WindowOpenDisposition targetDisposition, bool userGesture) {
            return false;
        }

        public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl,
            ISslInfo sslInfo, IRequestCallback callback) {
            return false;
        }

        public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath) {
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            IRequestCallback callback) {
            return CefReturnValue.Continue;
        }

        public bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port,
            string realm, string scheme, IAuthCallback callback) {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status) {

        }

        public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize,
            IRequestCallback callback) {
            return false;
        }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            ref string newUrl) {
        }

        public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url) {
            return false;
        }

        public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser) {

        }

        public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            IResponse response) {
            return false;
        }

        public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            IResponse response) {
            return null;
        }

        public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            IResponse response, UrlRequestStatus status, long receivedContentLength) {
        }
    }
}
