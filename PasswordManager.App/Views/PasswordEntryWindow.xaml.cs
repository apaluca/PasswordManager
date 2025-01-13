using PasswordManager.App.ViewModels;
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
using System.Windows.Shapes;

namespace PasswordManager.App.Views
{
        public partial class PasswordEntryWindow : Window
        {
                public PasswordEntryWindow()
                {
                        InitializeComponent();
                }

                private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
                {
                        if (DataContext is PasswordEntryViewModel viewModel && sender is PasswordBox passwordBox)
                        {
                                viewModel.Password = passwordBox.Password;
                        }
                }

                private void Window_Loaded(object sender, RoutedEventArgs e)
                {
                        PasswordBoxControl.Password = (DataContext as PasswordEntryViewModel)?.Password;
                }

        }
}
