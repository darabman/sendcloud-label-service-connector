using LabelServiceConnector.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LabelServiceConnector.ViewModels
{
    public class JobErrorViewModel : ViewModelBase
    {
        private const string _messageTemplateString = "{0} job(s) could not be processed";

        private string _message;

        public string ErrorCountMessage
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(ErrorCountMessage));
            }
        }
    }
}
