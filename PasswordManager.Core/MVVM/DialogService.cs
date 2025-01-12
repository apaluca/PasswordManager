using PasswordManager.Core.MVVM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordManager.Core.MVVM
{
        public class DialogService : IDialogService
        {
                public void ShowMessage(string message, string title = "Information")
                {
                        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                }

                public bool ShowConfirmation(string message, string title = "Confirm")
                {
                        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.Yes;
                }

                public void ShowError(string message, string title = "Error")
                {
                        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }
}
