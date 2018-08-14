using B2CPolicyEditor.Models;
using B2CPolicyEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace B2CPolicyEditor.Extensions
{
    public static class PolicyDocExtensions
    {
        public static XElement GetBase(this XDocument doc)
        {
            return doc.Root.Element(Constants.dflt + "BasePolicy");
        }
        public static void AddIdPToJourney(this XElement journey, XElement idp)
        {
            if (journey.Name != (Constants.dflt + "UserJourney")) throw new ApplicationException("Not a journey");
            var step1 = journey.Element(Constants.dflt + "OrchestrationSteps").Elements().First(e => e.Attribute("Order")?.Value == "1");
            var selection = step1.Element(Constants.dflt + "ClaimsProviderSelections");
            if (selection == null)
                return; // journey does not support adding new IdPs
            try
            {
                var tpID = idp.Element(Constants.dflt + "TechnicalProfiles").Elements(Constants.dflt + "TechnicalProfile").First().Attribute("Id").Value;
                var idpRef = $"{tpID}Exchange";
                selection.Add(new XElement(Constants.dflt + "ClaimsProviderSelection", new XAttribute("TargetClaimsExchangeId", idpRef)));
                var step2 = journey.Element(Constants.dflt + "OrchestrationSteps").Elements().Where(e => e.Attribute("Order")?.Value == "2").First();
                var exchanges = step2.Element(Constants.dflt + "ClaimsExchanges");
                exchanges.Add(new XElement(Constants.dflt + "ClaimsExchange",
                    new XAttribute("Id", idpRef),
                    new XAttribute("TechnicalProfileReferenceId", tpID)));
            } catch(Exception ex)
            {
                throw;
            }
        }
        public static XElement CreateIdP(string type, string name = "")
        {
            //foreach (var item in System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceNames())
            //{

            //}
            var idp = XElement.Load(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream($"B2CPolicyEditor.IdPPolicies.{type}.xml"));
            var tp = idp.Element(Constants.dflt + "TechnicalProfiles").Element(Constants.dflt + "TechnicalProfile");
            var newId = String.IsNullOrEmpty(name) ? tp.Attribute("Id").Value : name;
            var sameTps = App.PolicySet.Base.Root.Element(Constants.dflt + "ClaimsProviders")
                .Elements(Constants.dflt + "ClaimsProvider")
                    .Elements(Constants.dflt + "TechnicalProfiles")
                        .Elements(Constants.dflt + "TechnicalProfile").Where(p => p.Attribute("Id")?.Value == newId).Count();
            if (sameTps > 0) throw new Exception($"A TechnicalProfile with this Id({newId}) already exists");
            if (!String.IsNullOrEmpty(name))
            {
                tp.SetAttributeValue("Id", name);
                tp.Element(Constants.dflt + "OutputClaims")
                    .Elements(Constants.dflt + "OutputClaim")
                        .First(c => c.Attribute("ClaimTypeReferenceId").Value == "identityProvider").SetAttributeValue("DefaultValue", name);
                var keyData = tp.Element(Constants.dflt + "CryptographicKeys")
                        .Elements(Constants.dflt + "Key");
                if (type == "SAML")
                {
                    keyData.First(k => k.Attribute("Id").Value == "SamlAssertionSigning").SetAttributeValue("StorageReferenceId", $"B2C_1A_{name}SigningCert");
                    keyData.First(k => k.Attribute("Id").Value == "SamlMessageSigning").SetAttributeValue("StorageReferenceId", $"B2C_1A_{name}SigningCert");
                }
                else
                {
                    keyData.First(k => k.Attribute("Id").Value == "client_secret").SetAttributeValue("StorageReferenceId", $"B2C_1A_{name}Key");
                }
            }
            return idp;
        }
        public static void BuildClaimCollection(this XElement _tp, string collectionElementName, string claimTypeName, ObservableCollection<ClaimUsage> collection)
        {
            if (_tp.Element(Constants.dflt + collectionElementName) != null)
            {
                foreach (var c in _tp.Element(Constants.dflt + collectionElementName).Elements())
                    collection.Add(new ClaimUsage(c));
                collection.CollectionChanged += (s, e) =>
                {
                    switch (e.Action)
                    {
                        case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                            var claim = (ClaimUsage)e.NewItems[0];
                            var source = new XElement(Constants.dflt + claimTypeName);
                            _tp.Element(Constants.dflt + collectionElementName).Add(source);
                            claim.Source = source;
                            break;
                        case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                            claim = (ClaimUsage)e.OldItems[0];
                            claim.Source.Remove();
                            break;
                    }
                };
            }
        }
    }
}
