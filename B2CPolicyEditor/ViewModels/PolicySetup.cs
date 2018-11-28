using DataToolkit;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace B2CPolicyEditor.ViewModels
{
    class PolicySetup: ObservableObject
    {
        public PolicySetup()
        {
            MainWindow.Current.IsDisabled = false;
            VerifyB2CSetupCmd = new DelegateCommand(async () => await VerifyB2CSetup());
            Name = App.PolicySet.NamePrefix;
            IEFAppName = App.PolicySet.IEFAppName;
            IEFProxyAppName = App.PolicySet.IEFProxyAppName;
            IEFAppId = App.PolicySet.IEFAppId;
            IEFProxyAppId = App.PolicySet.IEFProxyAppId;
            ExtensionAppId = App.PolicySet.B2CExtensionAppId;
            ExtensionObjId = App.PolicySet.B2CExtensionObjId;
        }
        private async Task VerifyB2CSetup()
        {
            MainWindow.Current.IsDisabled = true;
            await LoginImplAsync();

            App.PolicySet.Domain = await GetDomainAsync();

            // Do keys exist, if not create

            // Do apps exist, if not create
            var app = await GetAppAsync(IEFAppName);
            if (app != null)
            {
                App.PolicySet.IEFAppId = (string)app["appId"];
                MainWindow.Trace.Add(new TraceItem() { Msg = $"{IEFAppName} assigned app id: {App.PolicySet.IEFAppId}." });
            }
            else
                App.PolicySet.IEFAppId = await CreateIEFApp(IEFAppName);
            IEFAppId = App.PolicySet.IEFAppId;
            app = await GetAppAsync(IEFProxyAppName);
            if (app != null)
            {
                App.PolicySet.IEFProxyAppId = (string)app["appId"];
                MainWindow.Trace.Add(new TraceItem() { Msg = $"{IEFProxyAppName} assigned app id: {App.PolicySet.IEFProxyAppId}." });
            }
            else
                App.PolicySet.IEFProxyAppId = await CreateIEFApp(IEFProxyAppName, App.PolicySet.IEFAppId);
            IEFProxyAppId = App.PolicySet.IEFProxyAppId;

            // b2c-extensions-app
            app = await GetAppAsync("b2c-extensions-app", true);
            if (app != null)
            {
                var id = (string)app["appId"];
                App.PolicySet.B2CExtensionAppId = id;
                ExtensionAppId = id;
                id = (string)app["objectId"];
                App.PolicySet.B2CExtensionObjId = id;
                ExtensionObjId = id;
                MainWindow.Trace.Add(new TraceItem() { Msg = $"B2C-Extensions-App app id is: {App.PolicySet.B2CExtensionObjId}." });
            }
            else
                MainWindow.Trace.Add(new TraceItem() { Msg = "B2C-Extensions-App was not found in the tenant. Please verify this is a B2C tenant." });
            MainWindow.Current.IsDisabled = false;
        }

        private async Task<string> CreateIEFApp(string appName, string IEFAppId = null)
        {
            var tokens = await SilentLoginImpl();
            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            var isProxy = (IEFAppId != null);
            var requiredAADAccess = new
            {
                resourceAppId = "00000002-0000-0000-c000-000000000000",
                resourceAccess = new List<object>()
                {
                    new {
                        id = "311a71cc-e848-46a1-bdf8-97ff7156d8e6",
                        type = "Scope"
                    }
                }
            };

            var app = new
            {
                displayName = appName,
                homepage = $"https://login.microsoftonline.com/{App.PolicySet.Domain}",
                identifierUris = new List<string>() { $"https://{appName}" },
                oauth2AllowIdTokenImplicitFlow = true,
                publicClient = isProxy,
                replyUrls = new List<string>() { $"https://login.microsoftonline.com/{App.PolicySet.Domain}" },
                requiredResourceAccess = new List<object>() { requiredAADAccess }
            };
            if (isProxy)
            {
                app.identifierUris.Clear();
                var requiredIEFAppAccess = new
                {
                    resourceAppId = IEFAppId,
                    resourceAccess = new List<object>()
                    {
                        new {
                            id = Guid.NewGuid().ToString("D"),
                            type = "Scope"
                        }
                    }
                };
                app.requiredResourceAccess.Add(requiredIEFAppAccess);
            }
            var json = JsonConvert.SerializeObject(app);
            var resp = await http.PostAsync($"https://graph.windows.net/myorganization/applications?api-version=1.6",
                new StringContent(json, Encoding.UTF8, "application/json"));
            if (resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                var appId = (string)JObject.Parse(body)["appId"];
                MainWindow.Trace.Add(new TraceItem() { Msg = $"IEF App {appName} created." });
                return appId;
            }
            throw new ApplicationException("App create failed");
        }

        private static async Task<AuthenticationResult> SilentLoginImpl()
        {
            var ctx = new AuthenticationContext("https://login.microsoftonline.com/common", new FileCache());
            var tokens = await ctx.AcquireTokenSilentAsync(
                "https://graph.windows.net",
                ConfigurationManager.AppSettings["aad:ClientId"]);
            return tokens;
        }

        private static async Task<string> GetDomainAsync()
        {
            var tokens = await SilentLoginImpl();
            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            var resp = await http.GetStringAsync($"https://graph.windows.net/myorganization/domains?api-version=1.6");
            var values = (JArray)(JObject.Parse(resp)["value"]);
            return (string) values.First()["name"];
        }

        private static async Task<JToken> GetAppAsync(string appName, bool startsWith = false)
        {
            var tokens = await SilentLoginImpl();
            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            var crit = startsWith ? $"startswith(displayName,'{appName}')" : $"displayName eq '{appName}'";
            var queryStr = $"https://graph.windows.net/myorganization/applications?api-version=1.6&$filter={crit}";
            var resp = await http.GetStringAsync(queryStr);
            var values = (JArray)(JObject.Parse(resp)["value"]);
            if (values.Count == 0)
                return null;
            return values.First();
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                Set(ref _name, value);
                App.PolicySet.NamePrefix = value;
            }
        }

        private string _IEFAppName;

        public string IEFAppName
        {
            get { return _IEFAppName; }
            set { _IEFAppName = value; }
        }
        private string _IEFAppId;
        public string IEFAppId
        {
            get { return _IEFAppId; }
            set { Set(ref _IEFAppId, value); }
        }


        private string _IEFProxyAppName;

        public string IEFProxyAppName
        {
            get { return _IEFProxyAppName; }
            set { _IEFProxyAppName = value; }
        }
        private string _IEFProxyAppId;
        public string IEFProxyAppId
        {
            get { return _IEFProxyAppId; }
            set { Set(ref _IEFProxyAppId, value); }
        }
        public string ExtensionAppId
        {
            get { return _ExtensionAppId; }
            set
            {
                Set(ref _ExtensionAppId, value);
            }
        }
        private string _ExtensionAppId;
        public string ExtensionObjId
        {
            get { return _ExtensionObjId; }
            set
            {
                Set(ref _ExtensionObjId, value);
            }
        }
        private string _ExtensionObjId;



        public ICommand VerifyB2CSetupCmd { get; private set; }

        private async Task<bool> LoginImplAsync()
        {
            var ctx = new AuthenticationContext("https://login.microsoftonline.com/common", new FileCache());
            try
            {
                var tokens = await ctx.AcquireTokenAsync(
                    "https://graph.windows.net",
                    ConfigurationManager.AppSettings["aad:ClientId"],
                    new Uri("https://b2cpolicyeditor"),
                    new PlatformParameters(PromptBehavior.Always));
                return true;
            }
            catch (Exception ex)
            {
                if (ex is AggregateException)
                {
                    var innerEx = ex.InnerException as AdalServiceException;
                    if (innerEx != null)
                        if (innerEx.ErrorCode == "authentication_canceled")
                        {
                            return false;
                        }
                }
                throw;
            }
        }
    }
}
