using B2CPolicyEditor.Extensions;
using B2CPolicyEditor.Models;
using DataToolkit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace B2CPolicyEditor.ViewModels
{
    class MainWindow : ObservableObject
    {
        public static MainWindow Current { get; private set; }
        public MainWindow()
        {
            Current = this;
            Trace = new ObservableCollection<TraceItem>();
            NewPolicy = new DelegateCommand(() =>
            {
                if (SaveCurrent(true))
                {
                    var dlgVm = new ViewModels.NewPolicyLoad();
                    var dlg = new Views.NewPolicyLoad() { DataContext = dlgVm };
                    dlgVm.Closing += () => dlg.Close();
                    dlg.ShowDialog();
                    UpdateTree();
                }
            });
            Open = new DelegateCommand(() =>
            {
                if (SaveCurrent(true))
                {
                    if (App.PolicySet.IsDirty)
                        Save.Execute(null);
                    var dlg = new System.Windows.Forms.FolderBrowserDialog();
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                        return;
                    var projectDir = dlg.SelectedPath;
                    //var projectDir = ConfigurationManager.AppSettings["xml:ProjectDir"];
                    var projFile = $"{projectDir}/PolicySet.json";
                    if (File.Exists(projFile))
                    {
                        using (var str = File.OpenText(projFile))
                        {
                            var json = str.ReadToEnd();
                            App.PolicySet = JsonConvert.DeserializeObject<Models.PolicySet>(json);
                        }
                    }
                    else
                        App.PolicySet = new Models.PolicySet() { NamePrefix = "Prefix" };

                    App.PolicySet.ProjectFolder = projectDir;

                    //var sourceDir = "";
                    //if ((App.PolicySet.FileNames != null) && File.Exists($"{projectDir}/{App.PolicySet.FileNames[0]}.xml"))
                    //    sourceDir = projectDir;
                    //else
                    //    sourceDir = ConfigurationManager.AppSettings["xml:Base"];
                    //App.PolicySet.Load(sourceDir);
                    App.PolicySet.Load(projectDir);
                    PopulateTreeView();
                }
            });
            Save = new DelegateCommand(() =>
            {
                var projectDir = App.PolicySet.ProjectFolder; // ConfigurationManager.AppSettings["xml:ProjectDir"];
                if (String.IsNullOrEmpty(projectDir))
                {
                        var dlg = new System.Windows.Forms.FolderBrowserDialog() { ShowNewFolderButton = true };
                        //dlg.RootFolder = Environment.SpecialFolder.Desktop;
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                    App.PolicySet.ProjectFolder = projectDir = dlg.SelectedPath;
                };
                var projFile = $"{projectDir}/PolicySet.json";
                using (var str = File.CreateText(projFile))
                {
                    var json = JsonConvert.SerializeObject(App.PolicySet);
                    str.Write(json);
                }
                App.PolicySet.Save();
                Trace.Add(new TraceItem() { Msg = $"Policy {NamePrefix} generated to {projectDir}" });
                App.PolicySet.IsDirty = false;
            });
            AddIdP = new DelegateCommand(() =>
            {
                var vm = new ViewModels.AddIdPWizard();
                var wiz = new Views.AddIdPWizard() { DataContext = vm };
                wiz.ShowDialog();
                //wiz.Close();
                if (vm.IsApplied)
                {
                    UpdateTree();
                    SelectArtifact(vm.CreatedIdP.Element(Constants.dflt + "TechnicalProfiles").Element(Constants.dflt + "TechnicalProfile"));
                }
            });
            //AddIdP = new DelegateCommand((p) =>
            //{
            //    var header = Items.FirstOrDefault(i => i.Category == TreeViewVMItem.TreeViewItemCatorgies.IdPs);
            //    var el = PolicyDocExtensions.CreateIdP((string) p);
            //    var newItem = new TreeViewVMItem()
            //    {
            //        DataSource = el,
            //        Name = el.Element(Constants.dflt + "DisplayName").Value,
            //        Command = new DelegateCommand((obj) => DetailView = new Views.SAMLIdP() { DataContext = new ViewModels.SAMLIdP((XElement)obj) }),
            //        Category = TreeViewVMItem.TreeViewItemCatorgies.Detail,
            //        DetailType = TreeViewVMItem.TreeViewItemDetails.IdP
            //    };
            //    header.Items.Add(newItem);
            //    SelectedArtifact = newItem;
            //});
            //AddCustomIdP = new DelegateCommand((p) =>
            //{
            //    var header = Items.FirstOrDefault(i => i.Category == TreeViewVMItem.TreeViewItemCatorgies.IdPs);
            //    var el = PolicyDocExtensions.CreateIdP((string)p);
            //    var newItem = new TreeViewVMItem()
            //    {
            //        DataSource = el.Element(Constants.dflt + "TechnicalProfiles").Element(Constants.dflt + "TechnicalProfile"),
            //        Name = el.Element(Constants.dflt + "DisplayName").Value,
            //        Command = new DelegateCommand((obj) => DetailView = new Views.SAMLIdP() { DataContext = new ViewModels.SAMLIdP((XElement)obj) }),
            //        Category = TreeViewVMItem.TreeViewItemCatorgies.Detail,
            //        DetailType = TreeViewVMItem.TreeViewItemDetails.IdP
            //    };
            //    header.Items.Add(newItem);
            //    SelectedArtifact = newItem;
            //});
            DeleteItem = new DelegateCommand(() =>
            {
                if ((SelectedArtifact == null) || (SelectedArtifact.Category != TreeViewVMItem.TreeViewItemCatorgies.Detail))
                    return;
                switch(SelectedArtifact.DetailType)
                {
                    case TreeViewVMItem.TreeViewItemDetails.IdP:
                        App.PolicySet.RemoveIdP((XElement) SelectedArtifact.DataSource);
                        var header = Items.First(i => i.Category == TreeViewVMItem.TreeViewItemCatorgies.IdPs);
                        header.Items.Remove(SelectedArtifact);
                        SelectedArtifact = Items[0];
                        break;
                    default:
                        break;
                }
            });
            NewJourney = new DelegateCommand(() =>
            {
                var header = Items.FirstOrDefault(i => i.Category == TreeViewVMItem.TreeViewItemCatorgies.Journeys);
                header.Items.Add(new TreeViewVMItem()
                {
                    Name = "Journey",
                    Category = TreeViewVMItem.TreeViewItemCatorgies.Detail,
                    Command = new DelegateCommand((obj) => DetailView = new Views.JourneyEditor() { DataContext = new ViewModels.JourneyEditor((XElement)obj) }),
                    DetailType = TreeViewVMItem.TreeViewItemDetails.Journey
                });
            });

            PolicySetup = new DelegateCommand(() => DetailView = new Views.PolicySetup());
            ShowClaims = new DelegateCommand(() => DetailView = new Views.Claims() { DataContext = new ViewModels.Claims() } );
        }
        public void UpdateTree()
        {
            if (App.PolicySet.Base != null)
                PopulateTreeView();
        }
        private void PopulateTreeView()
        {
            DetailView = null;
            NamePrefix = App.PolicySet.NamePrefix;

            Items = new ObservableCollection<TreeViewVMItem>()
            {
                new TreeViewVMItem()
                {
                    Name = "Policy set setup",
                    Command = PolicySetup,
                    Category = TreeViewVMItem.TreeViewItemCatorgies.Other
                },
                new TreeViewVMItem()
                {
                    Name = "Claims",
                    Command = ShowClaims,
                    Category = TreeViewVMItem.TreeViewItemCatorgies.Other
                },
                new TreeViewVMItem()
                {
                    Category = TreeViewVMItem.TreeViewItemCatorgies.IdPs,
                    Name = "Claims providers",
                    Items = GetExternalClaimProviders(),
                },
                //new TreeViewVMItem()
                //{
                //    Name = "Issued token types",
                //    Category = TreeViewVMItem.TreeViewItemCatorgies.Other
                //},
                new TreeViewVMItem()
                {
                    Name = "User journey types",
                    Category = TreeViewVMItem.TreeViewItemCatorgies.Journeys,
                    Items = GetUserJourneys()
                },
            };
            SelectedArtifact = Items[0]; // otherwise, binding does not get invoked
        }

        private ObservableCollection<TreeViewVMItem> GetExternalClaimProviders()
        {
            var cps = new ObservableCollection<TreeViewVMItem>();
            // Find all CPs NOT using the 'proprietary' protocol (ie. OAuth or OIDC)
            foreach (var el in App.PolicySet.Base.Root
                .Element(Constants.dflt + "ClaimsProviders")
                    .Elements() // ClaimsProvider
                        .Elements(Constants.dflt + "TechnicalProfiles")
                            .Elements()
                                .Where(tp => (tp.Element(Constants.dflt + "Protocol") != null)))
            {
                var protocolName = el.Element(Constants.dflt + "Protocol").Attribute("Name").Value;
                if ((String.Compare(protocolName, "Proprietary", true) == 0) || (String.Compare(protocolName, "None", true) == 0))
                    continue;
                var cp = new TreeViewVMItem()
                {
                    DataSource = el,
                    Category = TreeViewVMItem.TreeViewItemCatorgies.Detail,
                    DetailType = TreeViewVMItem.TreeViewItemDetails.IdP
                };
                var attr = el.Element(Constants.dflt + "DisplayName");
                if (attr != null)
                    cp.Name = attr.Value;
                var domain = el.Parent.Parent.Element(Constants.dflt + "Domain");
                if ((domain != null) && (new string[] { "facebook.com", "google.com", "live.com", "google.com", "linkedin.com", "twitter.com" }).Contains(domain.Value))
                    cp.Command = new DelegateCommand((obj) => DetailView = new Views.SocialIdP() { DataContext = new ViewModels.SocialIdP((XElement)obj) });
                else if (protocolName == "OAuth2")
                    cp.Command = new DelegateCommand((obj) => DetailView = new Views.OAuthConfiguration(obj));
                else if (protocolName == "SAML2")
                    cp.Command = new DelegateCommand((obj) => DetailView = new Views.SAMLIdP() { DataContext = new ViewModels.SAMLIdP((XElement)obj) });
                else // AAD?
                {
                    var meta = el.Element(Constants.dflt + "Metadata");
                    if (meta != null)
                    {
                        var sts = meta.Elements(Constants.dflt + "Item").Where(i => i.Attribute("Key")?.Value == "METADATA").First();
                        if ((sts != null) && (sts.Value.StartsWith("https://login.windows.net/contoso.com/.well-known/openid-configuration")))
                            cp.Command = new DelegateCommand((obj) => DetailView = new Views.AADIdP() { DataContext = new ViewModels.AADIdP((XElement)obj) });
                    }
                }
                cps.Add(cp);
            }

            return cps;
        }
        private ObservableCollection<TreeViewVMItem> GetUserJourneys()
        {
            var js = new ObservableCollection<TreeViewVMItem>();
            foreach(var j in App.PolicySet.Base.Root.Element(Constants.dflt + "UserJourneys").Elements())
            {
                js.Add(new TreeViewVMItem()
                {
                    Name = j.Attribute("Id").Value,
                    DataSource = j,
                    Command = new DelegateCommand((obj) => DetailView = new Views.JourneyEditor() { DataContext = new ViewModels.JourneyEditor((XElement)obj) }),
                    Category = TreeViewVMItem.TreeViewItemCatorgies.Detail,
                    DetailType = TreeViewVMItem.TreeViewItemDetails.Journey,
                    Items = GetJourneyTokens(j),
                });
            }
            return js;
        }
        private ObservableCollection<TreeViewVMItem> GetJourneyTokens(XElement journey)
        {
            var ts = new ObservableCollection<TreeViewVMItem>();
            var id = journey.Attribute("Id").Value;
            foreach(var journeyDoc in App.PolicySet.Journeys)
            {
                var rp = journeyDoc.Root.Element(Constants.dflt + "RelyingParty");
                if (rp.Element(Constants.dflt + "DefaultUserJourney").Attribute("ReferenceId").Value == id)
                {
                    ts.Add(new TreeViewVMItem()
                    {
                        Name = journeyDoc.Root.Attribute("PolicyId").Value.Substring(7),  // skip B2C_1A_
                        DataSource = journeyDoc,
                        Command = new DelegateCommand((obj) => DetailView = new Views.TokenEditor() { DataContext = new ViewModels.TokenEditor((XDocument)obj) }),
                        Category = TreeViewVMItem.TreeViewItemCatorgies.Detail,
                        DetailType = TreeViewVMItem.TreeViewItemDetails.Token,
                    });
                }
            }
            return ts;
        }
        private TreeViewVMItem GetArtifact(XElement source, TreeViewVMItem parent = null)
        {
            IEnumerable<TreeViewVMItem> items = parent == null ? Items : parent.Items;
            if (items == null) return null;
            foreach (var a in items)
            {
                if (a.DataSource == source)
                    return a;
                var resp = GetArtifact(source, a);
                if (resp != null) return resp;
            }
            return null;
        }

        private string _namePrefix;

        public string NamePrefix
        {
            get { return _namePrefix; }
            set
            {
                Set(ref _namePrefix, value);
            }
        }
        private TreeViewVMItem _selectedArtifact;

        public TreeViewVMItem SelectedArtifact
        {
            get { return _selectedArtifact; }
            set
            {
                if (Set(ref _selectedArtifact, value))
                {
                    if ((_selectedArtifact != null) && (_selectedArtifact.Command != null))
                        _selectedArtifact.Command.Execute(_selectedArtifact.DataSource);
                    else
                        DetailView = null;
                    ((DelegateCommand)DeleteItem).Enabled = ((_selectedArtifact != null) && (SelectedArtifact.Category == TreeViewVMItem.TreeViewItemCatorgies.Detail));
                }
            }
        }
        public void SelectArtifact(XElement source)
        {
            var artifact = GetArtifact(source);
            if (artifact == null) throw new ApplicationException("Artifact (treeview vm) representing this model element was not found in the tree.");
            SelectedArtifact = artifact;
        }
        private Page _detailView;
        public Page DetailView
        {
            get { return _detailView;  }
            set
            {
                Set(ref _detailView, value);
            }
        }

        private ObservableCollection<TreeViewVMItem> _items;
        public ObservableCollection<TreeViewVMItem> Items
        {
            get { return _items; }
            set { Set(ref _items, value); }
        }
        public ICommand NewPolicy { get; private set; }
        public ICommand Open { get; private set; }
        public ICommand Save { get; private set; }
        public ICommand TenantDetails { get; private set; }
        public ICommand AddIdP { get; private set; }
        //public ICommand AddCustomIdP { get; private set; }
        public ICommand Generate { get; private set; }
        public ICommand NewJourney { get; private set; }

        static ObservableCollection<TraceItem> _trace;
        public static ObservableCollection<TraceItem> Trace
        {
            get { return _trace; }
            private set { _trace = value; }
        }
        public ICommand PolicySetup { get; private set; }
        public ICommand DeleteItem { get; private set; }
        public ICommand ShowClaims { get; private set; }
        private bool SaveCurrent(bool allowCancel)
        {
            if (App.PolicySet.IsDirty)
            {
                var resp = MessageBox.Show("Save updates?", "Save", allowCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo);
                if (resp == MessageBoxResult.Cancel)
                    return false;
                if (resp == MessageBoxResult.Yes)
                    Save.Execute(null);
            }
            return true;
        }
        private bool _isDisabled;
        public bool IsDisabled
        {
            get { return _isDisabled; }
            set
            {
                Set(ref _isDisabled, value);
            }
        }
    }

    public class TreeViewVMItem: ObservableObject
    {
        internal enum TreeViewItemCatorgies { Other, IdPs, Claims, Journeys, Detail };
        internal enum TreeViewItemDetails {  Other, IdP, Claim, Journey, Token }
        internal TreeViewItemCatorgies Category { get; set; }
        internal TreeViewItemDetails DetailType { get; set; }
        public string Name
        {
            get { return _Name; }
            set
            {
                Set(ref _Name, value);
            }
        }
        private string _Name;

        public ICommand Command { get; set; }
        public ObservableCollection<TreeViewVMItem> Items { get; set; }
        public object DataSource
        {
            get { return _DataSource; }
            set
            {
                Set(ref _DataSource, value);
            }
        }
        private object _DataSource;

    }

    public class TraceItem
    {
        public TraceItem()
        {
            Timestamp = DateTime.Now;
        }
        public DateTime Timestamp { get; private set; }
        public string Msg { get; set; }
    }
}
