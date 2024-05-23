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

namespace Tubes3_SakedikKasep
{
    /// <summary>
    /// Interaction logic for bioDetails.xaml
    /// </summary>
    public partial class bioDetails : Window
    {
        public bioDetails(object bio)
        {
            InitializeComponent();

            this.DataContext = bio;
        }

        private void closeWindow(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
