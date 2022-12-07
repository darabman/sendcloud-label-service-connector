namespace LabelServiceConnector.Models
{
    public class ClientNotificationEventArgs
    {
        public string NotificationMessage { get; set; }

        public System.Windows.Forms.ToolTipIcon NotificationIcon { get; set; }

        public ClientNotificationEventArgs()
        {
            NotificationMessage = string.Empty;
        }
    }
}