﻿using System;
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
using System.Windows.Shapes;

namespace B2CPolicyEditor.Views
{
    /// <summary>
    /// Interaction logic for AddIdPWizard.xaml
    /// </summary>
    public partial class AddIdPWizard : Window
    {
        public AddIdPWizard()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                var vm = (ViewModels.AddIdPWizard)DataContext;
                vm.Closing += () => Close();
            };
        }
    }
}
