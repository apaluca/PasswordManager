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
        public partial class ChangePasswordWindow : Window
        {
                public ChangePasswordWindow()
                {
                        InitializeComponent();
                        Loaded += ChangePasswordWindow_Loaded;
                        Unloaded += ChangePasswordWindow_Unloaded;
                }

                private void ChangePasswordWindow_Loaded(object sender, RoutedEventArgs e)
                {
                        PasswordBoxBehavior.SetupBehavior(CurrentPasswordBox);
                        PasswordBoxBehavior.SetupBehavior(NewPasswordBox);
                        PasswordBoxBehavior.SetupBehavior(ConfirmPasswordBox);
                }

                private void ChangePasswordWindow_Unloaded(object sender, RoutedEventArgs e)
                {
                        PasswordBoxBehavior.CleanupBehavior(CurrentPasswordBox);
                        PasswordBoxBehavior.CleanupBehavior(NewPasswordBox);
                        PasswordBoxBehavior.CleanupBehavior(ConfirmPasswordBox);
                }
        }
}
