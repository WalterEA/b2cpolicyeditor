using System;
using System.Collections.Generic;
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
using System.Xml.Linq;

namespace B2CPolicyEditor.Views
{
    /// <summary>
    /// Interaction logic for OAuthConfiguration.xaml
    /// </summary>
    public partial class OAuthConfiguration : Page
    {
        public OAuthConfiguration()
        {
            InitializeComponent();
        }
        public OAuthConfiguration(object tp)
        {
            InitializeComponent();
            DataContext = new ViewModels.OAuthConfiguration((XElement) tp);
        }
    }
}
