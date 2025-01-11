using PasswordManager.Core.MVVM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.MVVM
{
        public class DialogService : IDialogService
        {
                public void ShowMessage(string message, string title = "Information")
                {
                        System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }

                public bool ShowConfirmation(string message, string title = "Confirm")
                {
                        return System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question)
                            == System.Windows.MessageBoxResult.Yes;
                }

                public void ShowError(string message, string title = "Error")
                {
                        System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
        }
}
