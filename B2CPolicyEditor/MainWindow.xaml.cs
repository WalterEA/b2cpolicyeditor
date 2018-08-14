using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using B2CPolicyEditor.Models;

namespace B2CPolicyEditor
{
    // https://login.microsoftonline.com/common/oauth2/authorize?client_id=32dadd7b-359e-4c02-9a77-f9b89fcf3a4d&response_type=code&response_mode=form_post&prompt=admin_consent
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainWindow();
            _rightPane.Navigating += (obj, ev) =>
            {

            };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveCurrent(false);
            base.OnClosing(e);
        }

        //private void OnAddIdP(object sender, RoutedEventArgs e)
        //{
        //    var vm = new ViewModels.AddIdPWizard();
        //    var wiz = new Views.AddIdPWizard() {DataContext = vm, Owner = this };
        //    wiz.ShowDialog();
        //    //wiz.Close();
        //    if (vm.IsApplied)
        //    {
        //        var dc = (ViewModels.MainWindow)DataContext;
        //        dc.UpdateTree();
        //        dc.SelectArtifact(vm.CreatedIdP.Element(Constants.dflt + "TechnicalProfiles").Element(Constants.dflt + "TechnicalProfile"));
        //    }
        //}

        private bool SaveCurrent(bool allowCancel)
        {
            if (App.PolicySet.IsDirty)
            {
                var resp = MessageBox.Show("Save updates?", "Save", allowCancel? MessageBoxButton.YesNoCancel: MessageBoxButton.YesNo);
                if (resp == MessageBoxResult.Cancel)
                    return false;
                if (resp == MessageBoxResult.Yes)
                    ((ViewModels.MainWindow)DataContext).Save.Execute(null);
            }
            return true;
        }
    }
}
