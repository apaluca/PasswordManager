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
                        new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged)
                        {
                                BindsTwoWayByDefault = true
                        });

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

                private static bool GetIsUpdating(DependencyObject dp)
                {
                        return (bool)dp.GetValue(IsUpdatingProperty);
                }

                private static void SetIsUpdating(DependencyObject dp, bool value)
                {
                        dp.SetValue(IsUpdatingProperty, value);
                }

                private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
                {
                        if (sender is PasswordBox passwordBox)
                        {
                                passwordBox.PasswordChanged -= PasswordChanged;

                                if (!GetIsUpdating(passwordBox))
                                {
                                        passwordBox.Password = (string)e.NewValue;
                                }

                                passwordBox.PasswordChanged += PasswordChanged;
                        }
                }

                private static void PasswordChanged(object sender, RoutedEventArgs e)
                {
                        if (sender is PasswordBox passwordBox)
                        {
                                SetIsUpdating(passwordBox, true);
                                SetPassword(passwordBox, passwordBox.Password);
                                SetIsUpdating(passwordBox, false);
                        }
                }

                public static void SetupBehavior(PasswordBox passwordBox)
                {
                        if (passwordBox != null)
                        {
                                passwordBox.PasswordChanged += PasswordChanged;
                        }
                }

                public static void CleanupBehavior(PasswordBox passwordBox)
                {
                        if (passwordBox != null)
                        {
                                passwordBox.PasswordChanged -= PasswordChanged;
                        }
                }
        }
}
