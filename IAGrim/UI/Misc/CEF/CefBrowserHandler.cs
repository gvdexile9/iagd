﻿using System;
using System.IO;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using IAGrim.Backup.Azure.CefSharp;
using IAGrim.Utilities;
using log4net;

namespace IAGrim.UI.Misc.CEF {
    public class CefBrowserHandler : IDisposable, ICefBackupAuthentication, IUserFeedbackHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CefBrowserHandler));
        private const string Dispatch = "data.globalStore.dispatch";
        private ChromiumWebBrowser _browser;
        public event EventHandler TransferSingleRequested;
        public event EventHandler TransferAllRequested;

        public ChromiumWebBrowser BrowserControl {
            get {
                return _browser;
            }
        }

        private object lockObj = new object();

        ~CefBrowserHandler() {
            Dispose();
        }
        public void Dispose() {
            try {
                lock (lockObj) {
                    if (_browser != null) {
                        CefSharpSettings.WcfTimeout = TimeSpan.Zero;
                        _browser.Dispose();

                        Cef.Shutdown();
                        _browser = null;
                    }
                }
            }
            catch (Exception ex) {
                // We're shutting down, doesn't matter. -- Rather not have these reported online.
                Logger.Warn(ex.Message, ex);
            }
        }

        public void ShowDevTools() {
            _browser.ShowDevTools();
        }

        public void ShowLoadingAnimation() {
            if (_browser.IsBrowserInitialized) {
                _browser.ExecuteScriptAsync("data.globalStore.dispatch(data.globalSetIsLoading(true));");
            }
        }
        public void RefreshItems() {
            if (_browser.IsBrowserInitialized) {
                _browser.ExecuteScriptAsync("setItemsFromGlobalItems();");
            }
            else {
                Logger.Warn("Attempted to update items but CEF not yet initialized.");
            }
        }

        public void JsCallback(string method, string json) {
            if (_browser.IsBrowserInitialized) {
                _browser.ExecuteScriptAsync($"{method}({json});");
            }
            else {
                Logger.Warn("Attempted to execute a callback but CEF not yet initialized.");
            }
        }

        public void AddItems() {
            if (_browser.IsBrowserInitialized) {
                // TODO: This may not be available yet, better the .js ask us for data, and then we deliver it.
                _browser.ExecuteScriptAsync($"addItemsFromGlobalItems();");
            }
            else {
                Logger.Warn("Attempted to update items but CEF not yet initialized.");
            }
        }


        public void ShowMessage(string message, UserFeedbackLevel level, string helpUrl = null) {
            string levelLowercased = level.ToString().ToLowerInvariant();
            var m = message.Replace("\n", "\\n").Replace("'", "\\'");
            if (!string.IsNullOrEmpty(message)) {
                if (_browser.IsBrowserInitialized) {
                    if (!string.IsNullOrEmpty(helpUrl)) {
                        _browser.ExecuteScriptAsync($"{Dispatch}(data.showMessage('{m}', '{levelLowercased}', '{helpUrl}'));");
                    }
                    else {
                        _browser.ExecuteScriptAsync($"{Dispatch}(data.showMessage('{m}', '{levelLowercased}', undefined));");
                    }
                }
                else {
                    Logger.Warn($"Attempted to display message \"{message}\" but browser is not yet initialized");
                }
            }
        }

        public void ShowMessage(string message) {
            ShowMessage(message, UserFeedbackLevel.Info);
        }


        public void InitializeChromium(object bindeable, EventHandler<IsBrowserInitializedChangedEventArgs> Browser_IsBrowserInitializedChanged) {
            try {
                Logger.Info("Creating Chromium instance..");
                Cef.EnableHighDPISupport();

                //var settings = new CefSettings();
                //settings.CefCommandLineArgs.Add("disable-gpu", "1");
                if (!Cef.Initialize()) {
                    Logger.Fatal("Could not initialize Chromium");
                    MessageBox.Show("Fatal error - Could not initialize chromium", "Fatal error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                _browser = new ChromiumWebBrowser(GlobalPaths.ItemsHtmlFile);

                // TODO: Read and analyze https://github.com/cefsharp/CefSharp/issues/2246 -- Is this the correct way to do things in the future?
                CefSharpSettings.LegacyJavascriptBindingEnabled = true;
                _browser.RegisterJsObject("data", bindeable, BindingOptions.DefaultBinder);
                _browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;

                var requestHandler = new CefRequestHandler();
                requestHandler.TransferSingleRequested += (sender, args) => this.TransferSingleRequested?.Invoke(sender, args);
                requestHandler.TransferAllRequested += (sender, args) => this.TransferAllRequested?.Invoke(sender, args);
                requestHandler.OnAuthentication += (sender, args) => OnSuccess?.Invoke(sender, args);
                _browser.RequestHandler = requestHandler;
                
                _browser.LifeSpanHandler = new AzureOnClosePopupHijack();

                Logger.Info("Chromium created..");
            } catch (System.IO.FileNotFoundException ex) {
                MessageBox.Show("Error \"File Not Found\" loading Chromium, did you forget to install Visual C++ runtimes?\n\nvc_redist86 in the IA folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                throw;
            } catch (IOException ex) {
                MessageBox.Show("Error loading Chromium, did you forget to install Visual C++ runtimes?\n\nvc_redist86 in the IA folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                throw;
            } catch (Exception ex) {
                MessageBox.Show("Unknown error loading Chromium, please see log file for more information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                throw;
            }
        }

        /* Start CefBackupAuthentication Start */
        public void Open(string url) {
            _browser.ExecuteScriptAsync($"window.open('{url}');");
        }

        public event EventHandler OnSuccess;
        /* End CefBackupAuthentication End */
    }
}
