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
                ViewModels.MainWindow.Trace.Add(new TraceItem() { Msg = "Failed to update Journeys with IdP" });
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
        public static void BuildClaimCollection(this XElement tp, string collectionElementName, string claimTypeName, ObservableCollection<ClaimUsage> collection)
        {
            if (tp.Element(Constants.dflt + collectionElementName) != null)
            {
                foreach (var c in tp.Element(Constants.dflt + collectionElementName).Elements())
                    collection.Add(new ClaimUsage(c));
                collection.CollectionChanged += (s, e) =>
                {
                    switch (e.Action)
                    {
                        case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                            var claim = (ClaimUsage)e.NewItems[0];
                            var source = new XElement(Constants.dflt + claimTypeName);
                            tp.Element(Constants.dflt + collectionElementName).Add(source);
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

        public static XElement GetTechnicalProfile(string id)
        {
            return App.PolicySet.Base.Root
                .Element(Constants.dflt + "ClaimsProviders")
                    .Elements(Constants.dflt + "ClaimsProvider")
                        .Elements(Constants.dflt + "TechnicalProfiles")
                            .Elements(Constants.dflt + "TechnicalProfile").FirstOrDefault(p => p.Attribute("Id").Value == id);

        }
        public static void SetMetadataValue(this XElement parent, string keyName, string value) // tested only with TP
        {
            var metadata = parent.Element(Constants.dflt + "Metadata");
            if (metadata != null)
            {
                //metadata.SetElementValue(Constants.dflt + "Item", new XAttribute("Key", "ApplicationObjectId"));
                var objId = metadata.Elements(Constants.dflt + "Item").FirstOrDefault(i => i.Attribute("Key")?.Value == keyName);
                if (objId == null)
                    metadata.Add(new XElement(Constants.dflt + "Item", new XAttribute("Key", keyName), value));
                else
                    objId.Value = value;
            }
            else
            {
                //HACK: Possible hack - for some reason upload refuses unless Metadata follows immediately after Protocol element!
                parent.Element(Constants.dflt + "Protocol").AddAfterSelf(new XElement(Constants.dflt + "Metadata",
                    new XElement(Constants.dflt + "Item", new XAttribute("Key", keyName), value)));
            }
        }
        public static string GetMetadataValue(this XElement parent, string keyName)
        {
            var metadata = parent.Element(Constants.dflt + "Metadata");
            if (metadata == null) return string.Empty;
            return metadata.Elements().First(el => el.Attribute("Key").Value == keyName)?.Value;
        }
        public static XElement element(this XElement parent, string name)
        {
            var ret = parent.Element(Constants.dflt + name);
            return ret;
        }

        public static IEnumerable<XElement> elements(this XElement parent, string name)
        {
            return parent.Elements(Constants.dflt + name);
        }
        public static IEnumerable<XElement> elements(this IEnumerable<XElement> parent, string name)
        {
            return parent.Elements(Constants.dflt + name);
        }
        //public static XDocument Merge(this XDocument target, XDocument source)
        //{
        //    foreach(var claimType in source.Root.Element(Constants.dflt + "BuildingBlocks")
        //                        .Element(Constants.dflt + "ClaimsSchema")
        //                            .Elements(Constants.dflt + "ClaimType"))
        //    {
        //        if (target.Root.Element(Constants.dflt + "BuildingBlocks")
        //                        .Element(Constants.dflt + "ClaimsSchema")
        //                            .Elements(Constants.dflt + "ClaimType")
        //                                .FirstOrDefault(c => c.Attribute("Id")
        //                                    .Value == claimType.Attribute("Id").Value) == null)
        //        target.Root.Element(Constants.dflt + "BuildingBlocks")
        //                        .Element(Constants.dflt + "ClaimsSchema").Add(claimType);
        //    }
        //    foreach (var transform in source.Root.Element(Constants.dflt + "BuildingBlocks")
        //                        .Element(Constants.dflt + "ClaimsTransformations")
        //                            .Elements(Constants.dflt + "ClaimsTransformation"))
        //    {
        //        if (target.Root.Element(Constants.dflt + "BuildingBlocks")
        //                        .Element(Constants.dflt + "ClaimsTransformations")
        //                            .Elements(Constants.dflt + "ClaimsTransformation")
        //                                .FirstOrDefault(c => c.Attribute("Id")
        //                                    .Value == transform.Attribute("Id").Value) == null)
        //            target.Root.Element(Constants.dflt + "BuildingBlocks")
        //                        .Element(Constants.dflt + "ClaimsTransformations").Add(transform);
        //    }
        //    foreach (var sourceCP in source.Root.Element(Constants.dflt + "ClaimsProviders")
        //                        .Elements(Constants.dflt + "ClaimsProvider"))
        //    {
        //        var targetCP = target.Root.Element(Constants.dflt + "ClaimsProviders")
        //                                .Elements(Constants.dflt + "ClaimsProvider")
        //                                    .FirstOrDefault(c => c.Element(Constants.dflt + "DisplayName").Value == sourceCP.Element(Constants.dflt + "DisplayName").Value);
        //        if (targetCP == null)
        //            target.Root.Element(Constants.dflt + "ClaimsProviders").Add(sourceCP);
        //        else
        //        {
        //            foreach (var sourceTP in sourceCP.Element(Constants.dflt + "TechnicalProfiles").Elements(Constants.dflt + "TechnicalProfile"))
        //            {
        //                var targetTP = targetCP.Element(Constants.dflt + "TechnicalProfiles").Elements(Constants.dflt + "TechnicalProfile").FirstOrDefault(p => p.Attribute("Id").Value == sourceTP.Attribute("Id").Value);
        //                if (targetTP == null)
        //                    targetCP.Element(Constants.dflt + "TechnicalProfiles").Add(sourceTP);
        //                else
        //                    targetTP
        //                        .MergeCollections(sourceTP, "Metadata", "Item", "Key")
        //                        .MergeCollections(sourceTP, "InputClaimsTransformations", "InputClaimsTransformation", "ReferenceId")
        //                        .MergeCollections(sourceTP, "InputClaims", "InputClaim", "ClaimTypeReferenceId")
        //                        .MergeCollections(sourceTP, "PersistedClaims", "PersistedClaim", "ClaimTypeReferenceId")
        //                        .MergeCollections(sourceTP, "OutputClaims", "OutputClaim", "ClaimTypeReferenceId")
        //                        .MergeCollections(sourceTP, "OutputClaimsTransformations", "OutputClaimsTransformation", "ReferenceId");
        //            }
        //        }
        //    }

        //    return target;
        //}
        public static XDocument Merge(this XDocument target, XDocument source)
        {
            foreach (var claimType in source.Root.Element(Constants.dflt + "BuildingBlocks")
                                .Element(Constants.dflt + "ClaimsSchema")
                                    .Elements(Constants.dflt + "ClaimType"))
            {
                if (target.Root.Element(Constants.dflt + "BuildingBlocks")
                                .Element(Constants.dflt + "ClaimsSchema")
                                    .Elements(Constants.dflt + "ClaimType")
                                        .FirstOrDefault(c => c.Attribute("Id")
                                            .Value == claimType.Attribute("Id").Value) == null)
                    target.Root.Element(Constants.dflt + "BuildingBlocks")
                                    .Element(Constants.dflt + "ClaimsSchema").Add(claimType);
            }
            foreach (var transform in source.Root.Element(Constants.dflt + "BuildingBlocks")
                                .Element(Constants.dflt + "ClaimsTransformations")
                                    .Elements(Constants.dflt + "ClaimsTransformation"))
            {
                if (target.Root.Element(Constants.dflt + "BuildingBlocks")
                                .Element(Constants.dflt + "ClaimsTransformations")
                                    .Elements(Constants.dflt + "ClaimsTransformation")
                                        .FirstOrDefault(c => c.Attribute("Id")
                                            .Value == transform.Attribute("Id").Value) == null)
                    target.Root.Element(Constants.dflt + "BuildingBlocks")
                                .Element(Constants.dflt + "ClaimsTransformations").Add(transform);
            }
            foreach (var sourceCP in source.Root.Element(Constants.dflt + "ClaimsProviders")
                                .Elements(Constants.dflt + "ClaimsProvider"))
            {
                var targetCP = target.Root.Element(Constants.dflt + "ClaimsProviders")
                                        .Elements(Constants.dflt + "ClaimsProvider")
                                            .FirstOrDefault(c => c.Element(Constants.dflt + "DisplayName").Value == sourceCP.Element(Constants.dflt + "DisplayName").Value);
                if (targetCP == null)
                    target.Root.Element(Constants.dflt + "ClaimsProviders").Add(sourceCP);
                else
                {
                    foreach (var sourceTP in sourceCP.Element(Constants.dflt + "TechnicalProfiles").Elements(Constants.dflt + "TechnicalProfile"))
                    {
                        var targetTP = targetCP.Element(Constants.dflt + "TechnicalProfiles").Elements(Constants.dflt + "TechnicalProfile").FirstOrDefault(p => p.Attribute("Id").Value == sourceTP.Attribute("Id").Value);
                        if (targetTP == null)
                            targetCP.Element(Constants.dflt + "TechnicalProfiles").Add(sourceTP);
                        else
                            targetTP
                                .MergeCollections(sourceTP, "Metadata", "Item", "Key")
                                .MergeCollections(sourceTP, "InputClaimsTransformations", "InputClaimsTransformation", "ReferenceId")
                                .MergeCollections(sourceTP, "InputClaims", "InputClaim", "ClaimTypeReferenceId")
                                .MergeCollections(sourceTP, "PersistedClaims", "PersistedClaim", "ClaimTypeReferenceId")
                                .MergeCollections(sourceTP, "OutputClaims", "OutputClaim", "ClaimTypeReferenceId")
                                .MergeCollections(sourceTP, "OutputClaimsTransformations", "OutputClaimsTransformation", "ReferenceId");
                    }
                }
            }
            foreach (var journey in source.Root.Element(Constants.dflt + "UserJourneys")
                                    .Elements(Constants.dflt + "UserJourney"))
            {
                var id = journey.Attribute("Id").Value;
                if (id == "*")
                {
                    foreach(var tj in target.Root.element("UserJourneys")
                        .elements("UserJourney"))
                            tj.MergeJourneys(journey);
                }
                else
                {
                    var currJourney = target.Root.element("UserJourneys")
                        .elements("UserJourney")
                            .FirstOrDefault(j => j.Attribute("Id")
                                .Value == journey.Attribute("Id").Value);
                    if (currJourney != null)
                        currJourney.Remove();
                    target.Root.Element(Constants.dflt + "UserJourneys").Add(journey);
                    if (currJourney == null)
                    {
                        var policyName = journey.Attribute("Id").Value;
                        var journeyRP = XDocument.Load(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("B2CPolicyEditor.IdPPolicies.UserJourney.xml"));
                        journeyRP.Root.Element(Constants.dflt + "RelyingParty")
                            .Element(Constants.dflt + "DefaultUserJourney").SetAttributeValue("ReferenceId", policyName);
                        App.PolicySet.Journeys.Add(journeyRP);
                        App.PolicySet.FileNames.Add(policyName);
                    }
                }
            }

            return target;
        }
        public static XElement MergeJourneys(this XElement target, XElement source)
        {
            foreach(var step in source.element("OrchestrationSteps").elements("OrchestrationStep"))
            {
                var mergeInstruction = step.Attribute(Constants.merge + "Order")?.Value;
                if (!String.IsNullOrEmpty(mergeInstruction))
                {
                    step.Attribute(Constants.merge + "Order").Remove();
                    switch (mergeInstruction)
                    {
                        case "BeforeLast":
                            var last = target.element("OrchestrationSteps").elements("OrchestrationStep").Last();
                            var stepNo = int.Parse(last.Attribute("Order").Value);
                            last.SetAttributeValue("Order", stepNo + 1);
                            step.SetAttributeValue("Order", stepNo);
                            last.AddBeforeSelf(step);
                            break;
                        default:
                            throw new Exception("Bad merge instruction");
                    }
                }
            }
            return target;
        }
        public static XElement MergeCollections(this XElement target, XElement source, string collectionName, string itemName, string keyAttr)
        {
            if (source.Element(Constants.dflt + collectionName) == null)
                return target;
            if (target.Element(Constants.dflt + collectionName) == null)
            {
                // Add new element in same relative position as it is in Source
                var sibling = source.Element(Constants.dflt + collectionName).ElementsBeforeSelf().Last();
                if (sibling == null)
                    target.AddFirst(new XElement(Constants.dflt + collectionName));
                else
                    target.Element(sibling.Name).AddAfterSelf(new XElement(Constants.dflt + collectionName));
            }
            foreach (var c in source.Element(Constants.dflt + collectionName).Elements(Constants.dflt + itemName))
            {
                // Try to replace any existing items with same attr values
                var targetItems = target.Element(Constants.dflt + collectionName).Elements(Constants.dflt + itemName);
                if (targetItems != null)
                {
                    var targetItem = targetItems.FirstOrDefault(t => t.Attribute(keyAttr).Value == c.Attribute(keyAttr).Value);
                    if (targetItem != null)
                    {
                        foreach (var attr in c.Attributes())
                            targetItem.SetAttributeValue(attr.Name, attr.Value);
                        continue;
                    }
                }
                // Otherwise just add the item
                target.Element(Constants.dflt + collectionName).Add(c);
            }
            return target;
        }
        internal static void SetTOUVersion(this XDocument doc, string version)
        {
            doc.Root
                .element("BuildingBlocks")
                    .element("ClaimsTransformations")
                        .elements("ClaimsTransformation")
                            .First(t => t.Attribute("Id").Value == "UpdateTOUVersion")
                                .element("InputParameters")
                                    .element("InputParameter")
                                        .SetAttributeValue("Value", version);
            var checkStep = doc.Root
                .element("UserJourneys")
                    .elements("UserJourney")
                        .First(j => j.Attribute("Id").Value == "SignUpOrSignIn")
                            .element("OrchestrationSteps")
                                .elements("OrchestrationStep")
                                    .First(s =>
                                        s.element("ClaimsExchanges")
                                            .element("ClaimsExchange")
                                                .Attribute("Id").Value == "ConfirmUpdatedTOU");
            checkStep.element("Preconditions")
                .element("Precondition")
                    .elements("Value").Skip(1).First().Value = version;
        }
        internal static void ChangeLocalUserIdTypeInJourneys(this XDocument doc, bool isUserId)
        {
            foreach(var step in doc.Root
                .element("UserJourneys")
                    .elements("UserJourney")
                        .elements("OrchestrationSteps")
                            .elements("OrchestrationStep"))
            {
                var exchanges = step.element("ClaimsExchanges");
                if (exchanges != null)
                {
                    foreach (var exchange in exchanges.elements("ClaimsExchange"))
                    {
                        var tp = exchange.Attribute("TechnicalProfileReferenceId");
                        if (isUserId)
                        {
                            var map = EmailToUserIdTPMappings.FirstOrDefault(t => t.Item1 == tp.Value);
                            if (map != null)
                                tp.Value = map.Item2;
                        } else
                        {
                            var map = EmailToUserIdTPMappings.FirstOrDefault(t => t.Item2 == tp.Value);
                            if (map != null)
                                tp.Value = map.Item1;
                        }
                    }
                }
            }
        }
        private static List<Tuple<string, string>> EmailToUserIdTPMappings = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("SelfAsserted-LocalAccountSignin-Email", "SelfAsserted-LocalAccountSignin-UserId"),
            new Tuple<string, string>("LocalAccountSignUpWithLogonEmail", "LocalAccountSignupWithUserId"),
            new Tuple<string, string>("LocalAccountDiscoveryUsingEmailAddress", "LocalAccountDiscoveryUsingUserId")
        };
    }
}
