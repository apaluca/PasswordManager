using PasswordManager.Core.MVVM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

                public string ShowPrompt(string message, string title = "Input Required")
                {
                        var dialog = new Window
                        {
                                Title = title,
                                Width = 300,
                                Height = 150,
                                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                Owner = Application.Current.MainWindow
                        };

                        var panel = new StackPanel { Margin = new Thickness(10) };
                        var label = new TextBlock { Text = message, Margin = new Thickness(0, 0, 0, 5) };
                        var textBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
                        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                        var okButton = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(0, 0, 5, 0) };
                        var cancelButton = new Button { Content = "Cancel", Width = 75, Height = 25 };

                        buttonPanel.Children.Add(okButton);
                        buttonPanel.Children.Add(cancelButton);
                        panel.Children.Add(label);
                        panel.Children.Add(textBox);
                        panel.Children.Add(buttonPanel);
                        dialog.Content = panel;

                        string result = null;
                        okButton.Click += (s, e) => { result = textBox.Text; dialog.DialogResult = true; };
                        cancelButton.Click += (s, e) => dialog.DialogResult = false;

                        return dialog.ShowDialog() == true ? result : null;
                }
        }
}
