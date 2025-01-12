using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.MVVM.Interfaces
{
        public interface IDialogService
        {
                void ShowMessage(string message, string title = "Information");
                bool ShowConfirmation(string message, string title = "Confirm");
                void ShowError(string message, string title = "Error");
                string ShowPrompt(string message, string title = "Input Required");
        }
}
