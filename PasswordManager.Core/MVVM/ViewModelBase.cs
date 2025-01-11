using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.MVVM
{
        public abstract class ViewModelBase : INotifyPropertyChanged
        {
                public event PropertyChangedEventHandler PropertyChanged;

                protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
                {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }

                protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
                {
                        if (Equals(field, value)) return false;
                        field = value;
                        OnPropertyChanged(propertyName);
                        return true;
                }

                private bool _isBusy;
                public bool IsBusy
                {
                        get => _isBusy;
                        protected set => SetProperty(ref _isBusy, value);
                }

                private string _errorMessage;
                public string ErrorMessage
                {
                        get => _errorMessage;
                        protected set => SetProperty(ref _errorMessage, value);
                }

                protected virtual void ClearError()
                {
                        ErrorMessage = null;
                }

                protected virtual void SetError(string message)
                {
                        ErrorMessage = message;
                }
        }
}
