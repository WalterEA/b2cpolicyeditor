using B2CPolicyEditor.Models;
using DataToolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace B2CPolicyEditor.ViewModels
{
    public class NewPolicyLoad: ObservableObject
    {

        public NewPolicyLoad()
        {
            SelectLocalAndSocial = true;
            //_PolicyFolder = @"c:\temp\Policies";
            _PolicyPrefix = "Test";

            Load = new DelegateCommand(() =>
            {
                App.PolicySet = new Models.PolicySet() { NamePrefix = PolicyPrefix };
                var url = String.Format(Constants.PolicyBaseUrl, Constants.PolicyFolderNames[_selectedPattern]);
                App.PolicySet.Load(url);
                if (Closing != null)
                    Closing();
            });
            //BrowseFolder = new DelegateCommand(() =>
            //{
            //    var dlg = new System.Windows.Forms.FolderBrowserDialog() { ShowNewFolderButton = true };
            //    //dlg.RootFolder = Environment.SpecialFolder.Desktop;
            //    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            //    PolicyFolder = dlg.SelectedPath;
            //});
        }
        public event Action Closing;
        private int _selectedPattern;
        public bool SelectLocal
        {
            get { return _SelectLocal; }
            set
            {
                if (Set(ref _SelectLocal, value) && value)
                    _selectedPattern = 0;
            }
        }
        private bool _SelectLocal;
        public bool SelectSocial
        {
            get { return _SelectSocial; }
            set
            {
                if (Set(ref _SelectSocial, value) && value)
                    _selectedPattern = 1;
            }
        }
        private bool _SelectSocial;
        public bool SelectLocalAndSocial
        {
            get { return _SelectLocalAndSocial; }
            set
            {
                if (Set(ref _SelectLocalAndSocial, value) && value)
                    _selectedPattern = 2;
            }
        }
        private bool _SelectLocalAndSocial;

        public bool SelectLocalAndSocialMFA
        {
            get { return _SelectLocalAndSocialMFA; }
            set
            {
                if (Set(ref _SelectLocalAndSocialMFA, value) && value)
                    _selectedPattern = 3;
            }
        }
        private bool _SelectLocalAndSocialMFA;
        //public string PolicyFolder
        //{
        //    get { return _PolicyFolder; }
        //    set
        //    {
        //        if (Set(ref _PolicyFolder, value))
        //            Validate();
        //    }
        //}
        //private string _PolicyFolder;
        public string PolicyPrefix
        {
            get { return _PolicyPrefix; }
            set
            {
                if (Set(ref _PolicyPrefix, value))
                    Validate();
            }
        }
        private string _PolicyPrefix;
        public ICommand BrowseFolder { get; private set; }
        public ICommand Load { get; private set; }

        private void Validate()
        {
            //var folderOk = Directory.Exists(_PolicyFolder);
            //if (!folderOk) MainWindow.Trace.Add(new TraceItem() { Msg = $"Folder {_PolicyFolder} does not exist" });

            var prefixOk = Regex.Match(_PolicyPrefix, @"^[A-Za-z][\w]").Success;
            if (!prefixOk) MainWindow.Trace.Add(new TraceItem { Msg = $"Not a valid prefix. Must be Alphanumeric, starting with alpha." });

            ((DelegateCommand)Load).Enabled = prefixOk; // && folderOk;
        }
    }
}
