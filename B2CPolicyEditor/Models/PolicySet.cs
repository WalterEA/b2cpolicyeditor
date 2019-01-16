using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using B2CPolicyEditor.Extensions;
using Newtonsoft.Json;

namespace B2CPolicyEditor.Models
{
    public class PolicySet
    {
        public PolicySet()
        {
            NamePrefix = "Policy1";
            IEFAppName = "IdentityExperienceFramework";
            IEFProxyAppName = "ProxyIdentityExperienceframework";
            IsDirty = false;
        }
        [JsonIgnore]
        public bool IsDirty { get; set; }
        private string _namePrefix;
        public string NamePrefix
        {
            get
            {
                return _namePrefix;
            }
            set
            {
                if (_namePrefix != value)
                {
                    IsDirty = true;
                    _namePrefix = value;
                }
            }
        }
        private string _IEFAppName;

        public string IEFAppName
        {
            get { return _IEFAppName; }
            set
            {
                if (_IEFAppName != value)
                {
                    _IEFAppName = value;
                    IsDirty = true;
                }
            }
        }
        private string _IEFProxyAppName;

        public string IEFProxyAppName
        {
            get { return _IEFProxyAppName; }
            set
            {
                if (_IEFProxyAppName != value)
                {
                    _IEFProxyAppName = value;
                    IsDirty = true;
                }
            }
        }

        private XDocument _base;
        [JsonIgnore]
        public XDocument Base { get { return _base; } }
        //private XDocument _extensions;
        //[JsonIgnore]
        //public XDocument Extensions { get { return _extensions; } }

        private List<XDocument> _journeys;
        public void Load(string baseDir)
        {
            _base = null;
            _journeys = new List<XDocument>();
            FileNames = new List<string>();

            if (baseDir.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    _base = LoadFromStarterPack(baseDir, "TrustFrameworkBase");
                    //_base.Root.Element(Constants.dflt + "ClaimsProviders").Elements().First().Remove(); // remove FB
                    //_extensions = LoadFromStarterPack(baseDir, "TrustFrameworkExtensions");
                    //_extension.Root.Element(Constants.dflt + "ClaimsProviders").Elements().First().Remove(); // remove FB
                    var j = LoadFromStarterPack(baseDir, "SignUpOrSignin");
                    _journeys.Add(j);
                    j = LoadFromStarterPack(baseDir, "ProfileEdit");
                    _journeys.Add(j);
                    j = LoadFromStarterPack(baseDir, "PasswordReset");
                    _journeys.Add(j);
                } catch(Exception ex) 
                {
                    if (_base == null)
                    {
                        ViewModels.MainWindow.Trace.Add(new ViewModels.TraceItem() { Msg = "Base policy not found." });
                        throw;
                    }
                }
            } else
            { 
                //var schemaUri = ConfigurationManager.AppSettings["xml:Schema"];
                //var schema = System.Xml.XmlReader.Create(schemaUri);
                //var schemas = new XmlSchemaSet();
                //schemas.Add(Constants.dflt.ToString(), schema);

                foreach (var file in Directory.GetFiles(baseDir, "*.xml"))
                {
                    using (var str = File.OpenRead(file))
                    {
                        var fName = file.Split('\\').Last();
                        fName = fName.Remove(fName.Length - 4);
                        XDocument doc = null;
                        try
                        {
                            doc = XDocument.Load(str);
                            doc.Changed += (s, e) => { IsDirty = true; };
                            //TODO: Getting errors, disable for now
                            //doc.Validate(schemas, (o, e) =>
                            //{
                            //    throw new ApplicationException($"Error validating {file}: {e.Message}");
                            //});
                            var baseDoc = doc.Root.Element(Constants.dflt + "BasePolicy");
                            if (baseDoc == null)
                            {
                                if (_base != null)
                                    throw new Exception($"Multiple base files found while processing {file}");
                                _base = doc;
                                if (FileNames.Count == 0)
                                    FileNames.Add(fName);
                                else
                                    FileNames.Insert(0, fName);
                            }
                            else
                            {
                                if (doc.Root.Element(Constants.dflt + "RelyingParty") != null)
                                {
                                    _journeys.Add(doc);
                                    FileNames.Add(fName);
                                }
                            }
                        } catch (XmlException ex)
                        {
                            ViewModels.MainWindow.Trace.Add(new ViewModels.TraceItem() { Msg = $"Xml format exception: {ex.Message}" });
                        }
                    }
                }
            }
            if (_base == null)
            {
                ViewModels.MainWindow.Trace.Add(new ViewModels.TraceItem() { Msg = "Base policy not found" });
                //throw new ApplicationException($"Base policy not found in folder: {baseDir}");
            }
            
            IsDirty = false;
        }
        private XDocument LoadFromStarterPack(string baseDir, string fileName)
        {
            var name = $"{baseDir}{fileName}.xml";
            ViewModels.MainWindow.Trace.Add(new ViewModels.TraceItem() { Msg = $"Loading {name}" });
            XDocument doc = null;
            try
            {
                doc = XDocument.Load($"{baseDir}{fileName}.xml");
                FileNames.Add(fileName);
                doc.Changed += (s, e) => IsDirty = true;
            } catch(Exception ex)
            {

            }
            return doc;
        }
        [JsonIgnore]
        public string Domain
        {
            get
            {
                return _base.Root.Attribute("TenantId").Value;
            }
            set
            {
                IsDirty = true;
                _base.Root.Attribute("TenantId").Value = value;
                //_extension.Root.Attribute("TenantId").Value = value;
                //_extension.Root.Element(Constants.dflt + "BasePolicy").Element(Constants.dflt + "TenantId").Value = value;
                foreach(var j in _journeys)
                {
                    j.Root.Attribute("TenantId").Value = value;
                    j.Root.Element(Constants.dflt + "BasePolicy").Element(Constants.dflt + "TenantId").Value = value;
                }
            }
        }
        [JsonIgnore]
        public string IEFAppId
        {
            get
            {
                try
                {
                    var tp =
                         _base.Root.Element(Constants.dflt + "ClaimsProviders").
                             Elements(Constants.dflt + "ClaimsProvider").
                                 Elements(Constants.dflt + "TechnicalProfiles").
                                     Elements(Constants.dflt + "TechnicalProfile").First(el => el.Attribute("Id").Value == "login-NonInteractive");
                    return tp.Element(Constants.dflt + "Metadata").Elements(Constants.dflt + "Item").FirstOrDefault(c => c.Attribute("Key").Value == "IdTokenAudience")?.Value;
                } catch(Exception ex)
                {
                    ViewModels.MainWindow.Trace.Add(new ViewModels.TraceItem() { Msg = "IEF App ID not found. OK if new policy or local users not used." });
                }
                return String.Empty;
            }
            set
            {
                var tp =
                    _base.Root.Element(Constants.dflt + "ClaimsProviders").
                        Elements(Constants.dflt + "ClaimsProvider").
                            Elements(Constants.dflt + "TechnicalProfiles").
                                Elements(Constants.dflt + "TechnicalProfile").First(el => el.Attribute("Id").Value == "login-NonInteractive");
                var id = tp.Element(Constants.dflt + "Metadata").Elements(Constants.dflt + "Item").FirstOrDefault(c => c.Attribute("Key").Value == "IdTokenAudience");
                if (id != null)
                    id.Value = value;
                else
                    tp.Element(Constants.dflt + "Metadata").Add(new XElement(Constants.dflt + "Item", new XAttribute("Key", "IdTokenAudience"), value));
                id = tp.Element(Constants.dflt + "InputClaims").Elements(Constants.dflt + "InputClaim").FirstOrDefault(c => c.Attribute("ClaimTypeReferenceId").Value == "resource_id");
                if (id != null)
                    id.Attribute("DefaultValue").Value = value;
                else
                    tp.Element(Constants.dflt + "InputClaims")
                        .Add(new XElement(Constants.dflt + "InputClaim", 
                            new XAttribute("ClaimTypeReferenceId", "resource_id"),
                            new XAttribute("PartnerClaimType", "resource"),
                            new XAttribute("DefaultValue", value)));
                IsDirty = true;
            }
        }
        [JsonIgnore]
        public string IEFProxyAppId
        {
            get
            {
                try
                {
                    return
                         _base.Root.Element(Constants.dflt + "ClaimsProviders").
                             Elements(Constants.dflt + "ClaimsProvider").
                                 Elements(Constants.dflt + "TechnicalProfiles").
                                     Elements(Constants.dflt + "TechnicalProfile").First(el => el.Attribute("Id").Value == "login-NonInteractive").
                                        Element(Constants.dflt + "Metadata").
                                            Elements(Constants.dflt + "Item").First(c => c.Attribute("Key").Value == "client_id").Value;
                } catch(Exception ex)
                {
                    ViewModels.MainWindow.Trace.Add(new ViewModels.TraceItem() { Msg = "IEF App ID not found. OK if new policy or local users not used." });
                    return String.Empty;
                }
            }
            set
            {
                IsDirty = true;
                var tp =
                    _base.Root.Element(Constants.dflt + "ClaimsProviders").
                        Elements(Constants.dflt + "ClaimsProvider").
                            Elements(Constants.dflt + "TechnicalProfiles").
                                Elements(Constants.dflt + "TechnicalProfile").First(el => el.Attribute("Id").Value == "login-NonInteractive");
                var id = tp.Element(Constants.dflt + "Metadata").Elements(Constants.dflt + "Item").FirstOrDefault(c => c.Attribute("Key").Value == "client_id");
                if (id != null)
                    id.Value = value;
                else
                    tp.Element(Constants.dflt + "Metadata").Add(new XElement(Constants.dflt + "Item", new XAttribute("Key", "client_id"), value));
                id = tp.Element(Constants.dflt + "InputClaims").Elements(Constants.dflt + "InputClaim").FirstOrDefault(c => c.Attribute("ClaimTypeReferenceId").Value == "client_id");
                if (id != null)
                    id.Attribute("DefaultValue").Value = value;
                else
                    tp.Element(Constants.dflt + "InputClaims").Add(new XElement(Constants.dflt + "InputClaim", new XAttribute("ClaimTypeReferenceId", "client_id"), new XAttribute("DefaultValue", value)));
            }
        }
        [JsonIgnore]
        public string B2CExtensionAppId
        {
            get
            {
                try
                {
                    string idVal =
                        _base.Root.Element(Constants.dflt + "ClaimsProviders").
                            Elements(Constants.dflt + "ClaimsProvider").
                                Elements(Constants.dflt + "TechnicalProfiles").
                                    Elements(Constants.dflt + "TechnicalProfile").First(el => el.Attribute("Id").Value == "AAD-Common").
                                        Element(Constants.dflt + "Metadata").
                                            Elements(Constants.dflt + "Item").FirstOrDefault(c => c.Attribute("Key").Value == "ClientId").Value;
                    Guid idGuid;
                    if (Guid.TryParse(idVal, out idGuid))
                        return idVal;
                } catch(NullReferenceException ex)
                {
                    // ignore - returninmg empty below
                }
                return String.Empty;
            }
            set
            {
                var tp =
                    _base.Root.Element(Constants.dflt + "ClaimsProviders").
                        Elements(Constants.dflt + "ClaimsProvider").
                            Elements(Constants.dflt + "TechnicalProfiles").
                                Elements(Constants.dflt + "TechnicalProfile").First(el => el.Attribute("Id").Value == "AAD-Common");
                tp.SetMetadataValue("ClientId", value);
            }
        }
        [JsonIgnore]
        public string B2CExtensionObjId
        {
            get
            {
                try
                {
                    string idVal =
                        _base.Root.Element(Constants.dflt + "ClaimsProviders").
                            Elements(Constants.dflt + "ClaimsProvider").
                                Elements(Constants.dflt + "TechnicalProfiles").
                                    Elements(Constants.dflt + "TechnicalProfile").First(el => el.Attribute("Id").Value == "AAD-Common").
                                        Element(Constants.dflt + "Metadata").
                                            Elements(Constants.dflt + "Item").FirstOrDefault(c => c.Attribute("Key").Value == "ApplicationObjectId").Value;
                    Guid idGuid;
                    if (Guid.TryParse(idVal, out idGuid))
                        return idVal;
                }
                catch (NullReferenceException ex)
                {
                    // ignore - returninmg empty below
                }
                return String.Empty;
            }
            set
            {
                var tp =
                    _base.Root.Element(Constants.dflt + "ClaimsProviders").
                        Elements(Constants.dflt + "ClaimsProvider").
                            Elements(Constants.dflt + "TechnicalProfiles").
                                Elements(Constants.dflt + "TechnicalProfile").First(el => el.Attribute("Id").Value == "AAD-Common");
                tp.SetMetadataValue("ApplicationObjectId", value);
            }
        }

        public List<XDocument> Journeys { get => _journeys; set => _journeys = value; }

        public List<string> FileNames;
        private void SetPolicyHeader(XDocument doc, string policyName, string baseName = null)
        {
            doc.Root.Attribute("TenantId").Value = App.PolicySet.Domain;
            doc.Root.Attribute("PolicyId").Value = $"B2C_1A_{App.PolicySet.NamePrefix}{policyName}";
            doc.Root.Attribute("PublicPolicyUri").Value = $"http://{App.PolicySet.Domain}/B2C_1A_{App.PolicySet.NamePrefix}{policyName}";

            if (baseName != null)
            {
                doc.Root.Element(Constants.dflt + "BasePolicy").Element(Constants.dflt + "TenantId").Value = App.PolicySet.Domain;
                doc.Root.Element(Constants.dflt + "BasePolicy").Element(Constants.dflt + "PolicyId").Value = $"B2C_1A_{App.PolicySet.NamePrefix}{baseName}";
            }
            //TODO: also journeys!
        }
        public void Save()
        {
            SetPolicyHeader(_base, FileNames[0]);
            //SetPolicyHeader(_extension, "Ext", "Base");
            _base.Save($"{App.MRU.ProjectFolder}/{FileNames[0]}.xml");
            // _extension.Root.Save("extension.xml");
            var i = 1; // 0 == base policy
            foreach (var j in _journeys)
            {
                SetPolicyHeader(j, FileNames[i], FileNames[0]);
                j.Save($"{App.MRU.ProjectFolder}/{FileNames[i]}.xml");
                ++i;
            }
            IsDirty = false;
        }

        //public XElement NewIdp(string type, string name, string protocol, out string helpUrl)
        //{
        //    var idpName = String.Empty;
        //    helpUrl = String.Empty;
        //    var protocolName = "OAuth2";
        //    var metadata = new XElement(Constants.dflt + "Metadata");
        //    switch (type)
        //    {
        //        case "AAD":
        //            idpName = "Contoso";
        //            helpUrl = "https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-setup-aad-custom";
        //            protocolName = "OpenIDConnect";
        //            metadata = new
        //                XElement(Constants.dflt + "Metadata",
        //                    new XElement(Constants.dflt + "Item", new XAttribute("Key", "METADATA"), "https://login.windows.net/contoso.com/.well-known/openid-configuration"),
        //                    new XElement(Constants.dflt + "Item", new XAttribute("ProviderName", "https://sts.windows.net/00000000-0000-0000-0000-000000000000")),
        //                    new XElement(Constants.dflt + "Item", new XAttribute("client_id", "00000000-0000-0000-0000-000000000000")),
        //                    new XElement(Constants.dflt + "Item", new XAttribute("IdTokenAudience", "00000000-0000-0000-0000-000000000000")),
        //                    new XElement(Constants.dflt + "Item", new XAttribute("response_types", "id_token")),
        //                    new XElement(Constants.dflt + "Item", new XAttribute("UsePolicyInRedirectUri", false)));
        //            break;
        //        case "FB":
        //            helpUrl = "https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-get-started-custom#next-steps";
        //            idpName = "Facebook";
        //            protocolName = "OAuth2";
        //            metadata =
        //                new XElement(Constants.dflt + "Metadata",
        //                        new XElement(Constants.dflt + "Item", new XAttribute("Key", "ProviderName"), ""),
        //                        new XElement(Constants.dflt + "Item", new XAttribute("Key", "authorization_endpoint"), ""),
        //                        new XElement(Constants.dflt + "Item", new XAttribute("Key", "AccessTokenEndpoint"), ""),
        //                        new XElement(Constants.dflt + "Item", new XAttribute("Key", "UsePolicyInRedirectUri"), "0"),
        //                        new XElement(Constants.dflt + "Item", new XAttribute("Key", "AccessTokenResponseFormat"), "json"));
        //            break;
        //        case "GG":
        //            helpUrl = "https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-custom-setup-goog-idp";
        //            break;
        //        default:
        //            break;
        //    }
        //    var el = new XElement(Constants.dflt + "ClaimsProvider",
        //                new XElement(Constants.dflt + "Domain", $"{idpName.ToLower()}.com"),
        //                new XElement(Constants.dflt + "DisplayName", idpName),
        //                new XElement(Constants.dflt + "TechnicalProfiles",
        //                    new XElement(Constants.dflt + "TechnicalProfile", new XAttribute("Id", $"{name}Profile"),
        //                    new XElement(Constants.dflt + "DisplayName", idpName),
        //                    new XElement(Constants.dflt + "Protocol", new XAttribute("Name", protocolName)),
        //                    new XElement(Constants.dflt + "OutputTokenFormat", "JWT"),
        //                    metadata,
        //                    new XElement(Constants.dflt + "CryptographicKeys",
        //                        new XElement(Constants.dflt + "Key", new XAttribute("Id", "client_secret"), new XAttribute("StorageReferenceId", $"B2C_1A_{idpName}Secret")),
        //                    new XElement(Constants.dflt + "InputClaims"),
        //                    new XElement(Constants.dflt + "OutputClaims",
        //                        new XElement(Constants.dflt + "OutputClaim", new XAttribute("ClaimTypeReferenceId", "socialIdpUserId"), new XAttribute("PartnerClaimsType", "id")),
        //                        new XElement(Constants.dflt + "OutputClaim", new XAttribute("ClaimTypeReferenceId", "givenName"), new XAttribute("PartnerClaimsType", "first_name")),
        //                        new XElement(Constants.dflt + "OutputClaim", new XAttribute("ClaimTypeReferenceId", "surname"), new XAttribute("PartnerClaimsType", "last_name")),
        //                        new XElement(Constants.dflt + "OutputClaim", new XAttribute("ClaimTypeReferenceId", "displayName"), new XAttribute("PartnerClaimsType", "name")),
        //                        new XElement(Constants.dflt + "OutputClaim", new XAttribute("ClaimTypeReferenceId", "email"), new XAttribute("PartnerClaimsType", "email")),
        //                        new XElement(Constants.dflt + "OutputClaim", new XAttribute("ClaimTypeReferenceId", "identityProvider"), new XAttribute("PartnerClaimsType", $"{idpName.ToLower()}.com")),
        //                        new XElement(Constants.dflt + "OutputClaim", new XAttribute("ClaimTypeReferenceId", "authenticationSource"), new XAttribute("PartnerClaimsType", "socialIdpAuthentication")),
        //                    new XElement(Constants.dflt + "OutputClaimsTransformations",
        //                        new XElement(Constants.dflt + "OutputClaimTransformation", new XAttribute("ReferenceId", "CreateRandomUPNUserName")),
        //                        new XElement(Constants.dflt + "OutputClaimTransformation", new XAttribute("ReferenceId", "CreateUserPrincipalName")),
        //                        new XElement(Constants.dflt + "OutputClaimTransformation", new XAttribute("ReferenceId", "CreateAlternativeSecurityId"))),
        //                    new XElement(Constants.dflt + "UseTechnicalProfileForSessionManagement", new XAttribute("ReferenceId", "SM_SocialLogin")))))));
        //    _base.Root.Element(Constants.dflt + "ClaimsProviders").Add(el);
        //    var tp = el.Element(Constants.dflt + "TechnicalProfiles").Element(Constants.dflt + "TechnicalProfile");
        //    switch (type)
        //    {
        //        case "AAD":
        //            tp.Element(Constants.dflt + "OutputClaims").Add(
        //                new XElement(Constants.dflt + "OutputClaim",
        //                    new XAttribute("ClaimTypeReferenceId", "tenantId"),
        //                    new XAttribute("PartnerClaimType", "tid")));
        //            break;
        //        case "FB":
        //            tp.Element(Constants.dflt + "OutputTokenFormat").Remove();
        //            break;
        //        case "GG":
        //            break;
        //        default:
        //            break;
        //    }
        //    return el;
        //}
        public void RemoveIdP(XElement tp)
        {
            List<string> tpRefs = new List<string>();
            var id = tp.Attribute("Id").Value;
            tp.Parent.Parent.Remove();
            foreach(var j in _base.Root.Element(Constants.dflt + "UserJourneys").Elements(Constants.dflt + "UserJourney"))
            {
                foreach(var step in j.Element(Constants.dflt + "OrchestrationSteps").Elements(Constants.dflt+"OrchestrationStep"))
                {
                    var exchanges = step.Element(Constants.dflt + "ClaimsExchanges");
                    if (exchanges == null) continue;
                    foreach (var ex in exchanges.Elements(Constants.dflt + "ClaimsExchange"))
                    {
                        if (ex.Attribute("TechnicalProfileReferenceId")?.Value == id)
                        {
                            tpRefs.Add(ex.Attribute("Id").Value);
                            ex.Remove();
                        }
                    }
                }
            }

            foreach (var j in _base.Root.Element(Constants.dflt + "UserJourneys").Elements(Constants.dflt + "UserJourney"))
            {
                foreach (var step in j.Element(Constants.dflt + "OrchestrationSteps").Elements(Constants.dflt + "OrchestrationStep"))
                {
                    var sels = step.Element(Constants.dflt + "ClaimsProviderSelections");
                    if (sels == null) continue;
                    foreach (var sel in sels.Elements(Constants.dflt + "ClaimsProviderSelection"))
                    {
                        if (tpRefs.Contains(sel.Attribute("TargetClaimsExchangeId")?.Value))
                            sel.Remove();
                    }
                }
            }
        }
    }
}
