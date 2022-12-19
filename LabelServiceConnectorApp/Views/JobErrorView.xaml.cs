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

namespace LabelServiceConnector.Views
{
    /// <summary>
    /// Interaction logic for JobErrorView.xaml
    /// </summary>
    public partial class JobErrorView : Window
    {
        public JobErrorView()
        {
            InitializeComponent();
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
