using System.Windows;

namespace BOLD
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
