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

namespace mustafina_glazki_s
{
    public partial class PrioritetWindow1 : Window
    {
        public int Result { get; private set; }
        public string Message { get; set; }

        
        public PrioritetWindow1(string message, int defaultValue)
        {
            InitializeComponent();
            Message = message;
            DataContext = this;
            PriorityTextBox.Text = defaultValue.ToString();
            PriorityTextBox.SelectAll();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PriorityTextBox.Text, out int result))
            {
                Result = result;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите корректное число!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}