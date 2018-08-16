using B2CPolicyEditor.Models;
using DataToolkit;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace B2CPolicyEditor.ViewModels
{
    public class AADIdP : ObservableObject
    {
        public AADIdP(XElement tp)
        {
            _tp = tp;
            _cp = _tp.Parent.Parent; // claimsprovider
            var b2cDomain = App.PolicySet.Base.Root.Attribute("TenantId").Value.Split('.')[0];
            B2CNameInIdP = $"B2C {b2cDomain} Federation";
            TpId = tp.Attribute("Id").Value;

            Configure = new DelegateCommand(async () =>
            {
                // Do not use FileCache to avoid conflict with B2C admin signin
                var ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
                try
                {
                    var tokens = await ctx.AcquireTokenAsync(
                        "https://graph.windows.net",
                        ConfigurationManager.AppSettings["aad:ClientId"],
                        new Uri("https://b2cpolicyeditor"),
                        new PlatformParameters(PromptBehavior.Always));
                    MainWindow.Trace.Add(new TraceItem() { Msg = $"AAD user {tokens.UserInfo.DisplayableId} logged in" });

                    // find out what domain it is. 
                    var http = new HttpClient();
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
                    var resp = await http.GetStringAsync($"https://graph.windows.net/myorganization/domains?api-version=1.6");
                    var values = (JArray)(JObject.Parse(resp)["value"]);
                    DomainName = (string)values.First(d => (bool)d["isDefault"])["name"];
                    _aadName = DomainName.Split('.')[0];
                    DisplayName = $"{_aadName} employee";
                    Description = $"Login using {DomainName} Azure AD tenant";
                    LoginUrl = new Uri(tokens.Authority);
                    TenantId = tokens.TenantId;

                    var currApp = await http.GetStringAsync($"https://graph.windows.net/myorganization/applications?api-version=1.6&$filter=displayName eq '{B2CNameInIdP}'");
                    values = (JArray)(JObject.Parse(currApp)["value"]);
                    if (values.Count == 0)
                    {
                        MainWindow.Trace.Add(new TraceItem() { Msg = $"Creating B2C reference in AAD with display name: ''{B2CNameInIdP}''" });
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
                        AppSecret = Utilities.GraphHelper.CreateSecret(32);
                        var appSecret = new
                        {
                            endDate = DateTime.UtcNow.AddYears(1),
                            keyId = Guid.NewGuid().ToString("D"),
                            startDate = DateTime.UtcNow,
                            type = "Symmetric",
                            usage = "Verify",
                            value = AppSecret
                        };
                        var app = new
                        {
                            displayName = B2CNameInIdP,
                            homepage = $"https://login.microsoftonline.com/te/{App.PolicySet.Domain}/oauth2/authresp",
                            identifierUris = new List<string>() { $"https://{App.PolicySet.Domain}" },
                            oauth2AllowIdTokenImplicitFlow = true,
                            publicClient = false,
                            replyUrls = new List<string>() { $"https://login.microsoftonline.com/te/{App.PolicySet.Domain}/oauth2/authresp" },
                            requiredResourceAccess = new List<object>() { requiredAADAccess },
                            keyCredentials = new List<object>() { appSecret },
                        };
                        var json = JsonConvert.SerializeObject(app);
                        var postResp = await http.PostAsync($"https://graph.windows.net/myorganization/applications?api-version=1.6",
                            new StringContent(json, Encoding.UTF8, "application/json"));
                        if (postResp.IsSuccessStatusCode)
                        {
                            var body = await postResp.Content.ReadAsStringAsync();
                            AppId = (string)JObject.Parse(body)["appId"];
                            MainWindow.Trace.Add(new TraceItem() { Msg = $"B2C reference created in AAD. AppId: {AppId}" });
                            MainWindow.Trace.Add(new TraceItem() { Msg = $"Please add the following key to your B2C tenant with name ''{AppSecretName}''" });
                        }
                        else
                            throw new ApplicationException("App create failed");
                    } else
                    {
                        AppId = (string)values.First()["appId"];
                        MainWindow.Trace.Add(new TraceItem() { Msg = $"Existing B2C reference found in AAD with appId: {AppId}" });
                    }

                    //_cp.SetElementValue(Constants.dflt + "Domain", DomainName);
                    //_cp.SetElementValue(Constants.dflt + "DisplayName", DisplayName);

                    //_tp.SetAttributeValue("Id", TechnicalProfileName);
                    //_tp.SetElementValue(Constants.dflt + "DisplayName", DisplayName);
                    //_tp.SetElementValue(Constants.dflt + "Description", Description);

                    var meta = _tp.Element(Constants.dflt + "Metadata");
                    meta.Elements(Constants.dflt + "Item").First(i => i.Attribute("Key")?.Value == "METADATA").Value = $"https://login.windows.net/{DomainName}/.well-known/openid-configuration";
                    meta.Elements(Constants.dflt + "Item").First(i => i.Attribute("Key")?.Value == "ProviderName").Value = $"https://sts.windows.net/{TenantId}/";
                    meta.Elements(Constants.dflt + "Item").First(i => i.Attribute("Key")?.Value == "client_id").Value = AppId;
                    meta.Elements(Constants.dflt + "Item").First(i => i.Attribute("Key")?.Value == "IdTokenAudience").Value = AppId;

                    //_tp.Element(Constants.dflt + "CryptographicKeys").Elements(Constants.dflt + "Key").First(c => c.Attribute("Id")?.Value == "client_secret").SetAttributeValue("Id", AppSecretName);

                    //_tp.Element(Constants.dflt + "OutputClaims").Elements(Constants.dflt + "OutputClaim").First(c => c.Attribute("ClaimTypeReferenceId")?.Value == "identityProvider").SetAttributeValue("ClaimTypeReferenceId", TpId);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }
        XElement _cp;
        XElement _tp;

        private string _aadName; 
        public string TpId
        {
            get { return _TpId; }
            set
            {
                Set(ref _TpId, value);
            }
        }
        private string _TpId;

        public string B2CNameInIdP
        {
            get { return _B2CNameInIdP; }
            set
            {
                Set(ref _B2CNameInIdP, value);
            }
        }
        private string _B2CNameInIdP;

        public string DomainName
        {
            get => _cp.Element(Constants.dflt + "Domain")?.Value;
            set
            {
                _cp.SetElementValue(Constants.dflt + "Domain", value);
                OnPropertyChanged("DomainName");
            }
        }

        public string DisplayName
        {
            get => _cp.Element(Constants.dflt + "DisplayName")?.Value;
            set
            {
                _cp.SetElementValue(Constants.dflt + "DisplayName", value);
                _tp.SetElementValue(Constants.dflt + "DisplayName", value);
                OnPropertyChanged("DisplayName");
            }
        }

        public string Description
        {
            get => _tp.Element(Constants.dflt + "Description")?.Value;
            set
            {
                _tp.SetElementValue(Constants.dflt + "Description", value);
                OnPropertyChanged("Description");
            }
        }

        public string TenantId { get; set; }
        public Uri LoginUrl { get; set; }

        public string AppId { get; set; }
        public string HelpUrl { get => "https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-setup-aad-custom"; }

        public string AppSecret
        {
            get { return _AppSecret; }
            set
            {
                Set(ref _AppSecret, value);
            }
        }
        private string _AppSecret;
        public string AppSecretName
        {
            get => _tp.Element(Constants.dflt + "CryptographicKeys")
                    .Elements(Constants.dflt + "Key")
                        .First(k => k.Attribute("Id").Value == "client_secret").Attribute("StorageReferenceId").Value;
            set
            {
                _tp.Element(Constants.dflt + "CryptographicKeys")
                    .Elements(Constants.dflt + "Key")
                        .First(k => k.Attribute("Id").Value == "client_secret").Attribute("StorageReferenceId").Value = value;
                OnPropertyChanged("AppSecretName");
            }
        }

        public ICommand Configure { get; private set; }
    }
}
