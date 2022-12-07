using System;
using System.Windows;
using Forms = System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace LabelServiceConnector
{
    partial class App : Application
    {
        private readonly int NotificationDurationSeconds = 2;

        private void BuildNotifyIcon()
        {
            _notifyIcon.Icon = new Icon("Resources/icon.ico");
            _notifyIcon.Text = "SendCloud Label Service Connector";

            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.MinimumSize = new System.Drawing.Size(100, 0);
            _notifyIcon.ContextMenuStrip.Items.Add(new Forms.ToolStripButton(
                "Archive All Delivered Shipments",
                image: null,
                OnNotifyArchive));
            _notifyIcon.ContextMenuStrip.Items.Add(new Forms.ToolStripButton(
                "Close",
                image: null,
                OnNotifyClose));

            _notifyIcon.Visible = true;
        }

        private void OnNotifyArchive(object? sender, EventArgs e)
        {
            try
            {
                _archiverAgent.DownloadAll();
            }
            catch (Exception ex)
            {
                OnClientNotification(ToolTipIcon.Error, $"Failed to manually download 'Delivered' parcels, please check logs for more information");

                _logger.LogError($"General fault while downloading delivered parcels");
                _logger.LogDebug(ex.ToString() + $" {ex.Message}");
            }
        }

        private void OnNotifyClose(object? sender, EventArgs e)
        {
            var clickResult = System.Windows.MessageBox.Show("Möchten Sie die Anwendung 'Label Service Connector' schließen?", "Anwendung schließens", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (clickResult == MessageBoxResult.Yes)
            {
                Current.Shutdown();
            }
        }

        private void OnClientNotification(Forms.ToolTipIcon icon, string message)
        {
            _notifyIcon.ShowBalloonTip(NotificationDurationSeconds * 1000,
                       null,
                       message,
                       icon);
        }
    }
}
