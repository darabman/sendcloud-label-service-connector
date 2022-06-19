﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Forms = System.Windows.Forms;
using System.Windows.Input;
using System.Drawing;

namespace LabelServiceConnector
{
    partial class App : Application
    {
        private void BuildNotifyIcon()
        {
            _notifyIcon.Icon = new Icon("Resources/icon.ico");
            _notifyIcon.Text = "SendCloud Label Service Connector";

            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.MinimumSize = new System.Drawing.Size(100, 0);          
            _notifyIcon.ContextMenuStrip.Items.Add(new Forms.ToolStripButton(
                "Close",
                image: null,
                OnNotifyClose));
            

            _notifyIcon.Visible = true;
        }

        private void OnNotifyClose(object? sender, EventArgs e)
        {
            var clickResult = MessageBox.Show("Would you click to close SendCloud Label Service Connector?", "Close Application", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (clickResult == MessageBoxResult.Yes)
            {
                Current.Shutdown();
            }
        }
    }
}
