using System;
using System.Windows;

namespace ValvApp00
{
    public partial class MainWindow : Window
    {
        private MainViewModel mvm;
       
        public MainWindow()
        {
            InitializeComponent();

            mvm = new MainViewModel();
            this.DataContext = mvm;
        }
    }
}
