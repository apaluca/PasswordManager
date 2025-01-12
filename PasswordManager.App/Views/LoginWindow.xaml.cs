using PasswordManager.App.ViewModels;
using PasswordManager.Core.MVVM;
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
        public partial class LoginWindow : Window
        {
                public LoginWindow(LoginViewModel viewModel)
                {
                        InitializeComponent();
                        DataContext = viewModel;
                        Loaded += LoginWindow_Loaded;
                        Unloaded += LoginWindow_Unloaded;
                }

                private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
                {
                        PasswordBoxBehavior.SetupBehavior(PasswordBox);
                }

                private void LoginWindow_Unloaded(object sender, RoutedEventArgs e)
                {
                        PasswordBoxBehavior.CleanupBehavior(PasswordBox);
                }
        }
}
