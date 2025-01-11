using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace PasswordManager.Core.MVVM
{
        public static class PasswordBoxBehavior
        {
                public static readonly DependencyProperty PasswordProperty =
                    DependencyProperty.RegisterAttached(
                        "Password",
                        typeof(string),
                        typeof(PasswordBoxBehavior),
                        new PropertyMetadata(string.Empty, OnPasswordPropertyChanged));

                private static readonly DependencyProperty IsUpdatingProperty =
                    DependencyProperty.RegisterAttached(
                        "IsUpdating",
                        typeof(bool),
                        typeof(PasswordBoxBehavior));

                public static void SetPassword(DependencyObject dp, string value)
                {
                        dp.SetValue(PasswordProperty, value);
                }

                public static string GetPassword(DependencyObject dp)
                {
                        return (string)dp.GetValue(PasswordProperty);
                }

                private static void SetIsUpdating(DependencyObject dp, bool value)
                {
                        dp.SetValue(IsUpdatingProperty, value);
                }

                private static bool GetIsUpdating(DependencyObject dp)
                {
                        return (bool)dp.GetValue(IsUpdatingProperty);
                }

                private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
                {
                        PasswordBox passwordBox = sender as PasswordBox;
                        if (passwordBox == null)
                                return;

                        if (GetIsUpdating(passwordBox))
                                return;

                        passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                        passwordBox.Password = (string)e.NewValue;
                        passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }

                public static void AttachBehavior(PasswordBox passwordBox)
                {
                        passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }

                private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
                {
                        PasswordBox passwordBox = sender as PasswordBox;
                        if (passwordBox == null)
                                return;

                        SetIsUpdating(passwordBox, true);
                        SetPassword(passwordBox, passwordBox.Password);
                        SetIsUpdating(passwordBox, false);
                }
        }
}
