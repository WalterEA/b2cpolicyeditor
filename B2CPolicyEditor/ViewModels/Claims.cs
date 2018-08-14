using B2CPolicyEditor.Models;
using DataToolkit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace B2CPolicyEditor.ViewModels
{
    class Claims: ObservableObject
    {
        public Claims()
        {
            ClaimsList = new ObservableCollection<Claim>();
            foreach(var c in App.PolicySet.Base.Root.Element(Constants.dflt + "BuildingBlocks").Element(Constants.dflt + "ClaimsSchema").Elements(Constants.dflt + "ClaimType"))
            {
                ClaimsList.Add(new Claim()
                {
                    //Id = c.Attribute("Id")?.Value,
                    //DataType = c.Element(Constants.dflt + "DataType")?.Value,
                    //DisplayName = c.Element(Constants.dflt + "DisplayName")?.Value,
                    //InputType = c.Element(Constants.dflt + "UserInputType")?.Value,
                    Source = c,
                });
            }
            ClaimsList.CollectionChanged += (o, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        var claim = (Claim) e.NewItems[0];
                        var source = new XElement(Constants.dflt + "ClaimType",
                            new XElement(Constants.dflt + "DisplayName"),
                            new XElement(Constants.dflt + "DataType", "String"),
                            new XElement(Constants.dflt + "UserInputType", "TextBox"));
                        App.PolicySet.Base.Root
                            .Element(Constants.dflt + "BuildingBlocks")
                                .Element(Constants.dflt + "ClaimsSchema").Add(source);
                        claim.Source = source;
                        claim.IsCustom = true;
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        claim = (Claim) e.OldItems[0];
                        App.PolicySet.Base.Root
                            .Element(Constants.dflt + "BuildingBlocks")
                                .Element(Constants.dflt + "ClaimsSchema")
                                    .Elements(Constants.dflt + "ClaimType").First(c => c.Attribute("Id").Value == claim.Id).Remove();
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        break;
                    default:
                        break;
                }
            };
        }
        public ObservableCollection<Claim> ClaimsList { get; set; }
    }

    public class Claim: ObservableObject
    {
        public Claim()
        {
            IsCustom = false;
        }
        public string Id
        {
            get { return Source.Attribute("Id")?.Value; }
            set
            {
                if(IsCustom)
                {
                    if (!value.StartsWith("extension_"))
                        value = "extension_" + value;
                }
                if (Source.Attribute("Id")?.Value != value)
                {
                    Source.SetAttributeValue("Id", value);
                    OnPropertyChanged("Id");
                }
            }
        }

        public string DisplayName
        {
            get { return Source.Element(Constants.dflt + "DisplayName")?.Value; }
            set { Source.SetElementValue(Constants.dflt + "DisplayName", value); }
        }

        public string DataType
        {
            get { return Source.Element(Constants.dflt + "DataType")?.Value; }
            set { Source.SetElementValue(Constants.dflt + "DataType", value); }
        }
        public string InputType
        {
            get { return Source.Element(Constants.dflt + "UserInputType")?.Value; }
            set { Source.SetElementValue(Constants.dflt + "UserInputType", value); }
        }
        public XElement Source { get; set; }

        public bool IsCustom { get; set; }

    }
}
