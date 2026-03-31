using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace mustafina_glazki_s
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private Agent currentAgent = new Agent();
        public AddEditPage(Agent SelectedAgent)
        {
            InitializeComponent();
            if (SelectedAgent != null)
                currentAgent = SelectedAgent;
            DataContext = currentAgent;
            DeleteAgent.Visibility = (SelectedAgent != null && SelectedAgent.ID != 0) ? Visibility.Visible : Visibility.Collapsed;

        }

        private void ChangePictureBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog myOpenFileDialog = new OpenFileDialog();
            myOpenFileDialog.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*";

            if (myOpenFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFile = myOpenFileDialog.FileName;
                    if (!File.Exists(sourceFile))
                    {
                        MessageBox.Show("Исходный файл не найден.");
                        return;
                    }

                    string imgsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Imgs", "agents");
                    Directory.CreateDirectory(imgsFolder);

                    string fileName = Path.GetFileName(sourceFile);
                    string destPath = Path.Combine(imgsFolder, fileName);

                    int count = 1;
                    while (File.Exists(destPath))
                    {
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        string ext = Path.GetExtension(fileName);
                        string newName = $"{nameWithoutExt}_{count}{ext}";
                        destPath = Path.Combine(imgsFolder, newName);
                        count++;
                    }

                    File.Copy(sourceFile, destPath);

                    //_currentAgent.Logo = Path.GetFileName(destPath);
                    currentAgent.Logo = $@"agents\{Path.GetFileName(destPath)}";

                    LogoImage.Source = new BitmapImage(new Uri(destPath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при копировании файла: " + ex.Message);
                }
            }
        
        }

        private void DeleteAgent_Click(object sender, RoutedEventArgs e)
        {
            var currentAgent = Mustafina_glazkiEntities.GetContext();
            var selectedAgent = (Agent)DataContext;
            if (selectedAgent.ProductSale != null && selectedAgent.ProductSale.Any())
            {
                MessageBox.Show("Нельзя удалить: есть информация о реализации продукции");
                return;
            }
            if (MessageBox.Show("вы хотите удалить агента?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            if (currentAgent.AgentPriorityHistory != null)
            {
                foreach (var item in currentAgent.AgentPriorityHistory.ToList())
                {
                    Mustafina_glazkiEntities.GetContext().AgentPriorityHistory.Remove(item);
                }
            }
            if (currentAgent.Shop != null)
            {
                foreach (var shop in currentAgent.Shop.ToList())
                {
                    Mustafina_glazkiEntities.GetContext().Shop.Remove(shop);
                }
            }
           // currentAgent.Agent.Remove(currentAgent);
            try
            {
                currentAgent.SaveChanges();
                MessageBox.Show("Агент удалён.");
                Manager.MainFrame.Navigate(new agentPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении: " + ex.Message);
            }
        }

        private void SaveAgent_Click(object sender, RoutedEventArgs e)
        {
           // var currentAgent = Mustafina_glazkiEntities.GetContext().Agent.ToList();
            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(currentAgent.Title))
                errors.AppendLine("Укажите наименовние агента");
            if (string.IsNullOrWhiteSpace(currentAgent.Address))
                errors.AppendLine("Укажите адрес агента");
            if (string.IsNullOrWhiteSpace(currentAgent.DirectorName))
                errors.AppendLine("Укажите ФИО директора");
            if (ComboTYpe.SelectedItem == null)
                errors.AppendLine("Укажите тип агента");
            else currentAgent.AgentTypeID = ComboTYpe.SelectedIndex + 1;
            if (string.IsNullOrWhiteSpace(currentAgent.Priority.ToString()))
                errors.AppendLine("Укажите приоритет агента");
            if (currentAgent.Priority <= 0)
                errors.AppendLine("Укажите положительный приоритет агента");
            if (string.IsNullOrWhiteSpace(currentAgent.INN))
                errors.AppendLine("Укажите ИНН агента");
            if (string.IsNullOrWhiteSpace(currentAgent.KPP))
                errors.AppendLine("Укажите КПП агента");
            if (string.IsNullOrWhiteSpace(currentAgent.Phone))
                errors.AppendLine("Укажите телефон агента");
            else
            {
                string ph = currentAgent.Phone.Replace("(", "").Replace("-", "").Replace("+", "");
                if (((ph[1] == '9' || ph[1] == '4' || ph[1] == '8') && ph.Length != 11) || (ph[1] == '3' && ph.Length != 12))
                    errors.AppendLine("Укажите ПРАВИЛЬНЫЙ телефон");
            }
            if (string.IsNullOrWhiteSpace(currentAgent.Email))
                errors.AppendLine("Укажите почту агента");
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }
            if (currentAgent.ID == 0)
                Mustafina_glazkiEntities.GetContext().Agent.Add(currentAgent);
            try
            {
                Mustafina_glazkiEntities.GetContext().SaveChanges();
                MessageBox.Show("инфа сохранена");
                Manager.MainFrame.Navigate(new agentPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    
    }
}
