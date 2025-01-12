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
        public partial class EncryptionKeyWindow : Window
        {
                public string Key { get; set; }

                public EncryptionKeyWindow(string key)
                {
                        InitializeComponent();
                        Key = key;
                        DataContext = this;
                }

                private void CopyButton_Click(object sender, RoutedEventArgs e)
                {
                        Clipboard.SetText(Key);
                        MessageBox.Show("Key copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                private void CloseButton_Click(object sender, RoutedEventArgs e)
                {
                        Close();
                }
        }
}
