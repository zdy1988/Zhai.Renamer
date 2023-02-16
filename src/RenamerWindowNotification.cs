using System;
using Zhai.Famil.Common.Mvvm;

namespace Zhai.Renamer
{
    internal partial class RenamerWindowViewModel : ViewModelBase
    {
        public void SendNotificationMessage(string message)
        {
            if (!String.IsNullOrWhiteSpace(message))
            {
                NotificationMessage = "";
                NotificationMessage = message;
            }
        }

        private string notificationMessage;
        public string NotificationMessage
        {
            get { return notificationMessage; }
            set { Set(() => NotificationMessage, ref notificationMessage, value); }
        }
    }
}
