using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// 
    public class SaleHistoryItem
    {
        public Product Product { get; set; }
        public int ProductCount { get; set; }
        public DateTime SaleDate { get; set; }

        // Для отображения в ListView
        public string ProductName => Product?.Title ?? "Неизвестный продукт";
    }


    public partial class AddEditPage : Page
    {


        private Agent currentAgent = new Agent();
        private List<SaleHistoryItem> salesHistory = new List<SaleHistoryItem>();
        private List<Product> products = new List<Product>();


        public AddEditPage(Agent SelectedAgent)
        {
            InitializeComponent();

            if (SelectedAgent != null)
                currentAgent = SelectedAgent;

            DataContext = currentAgent;
            DeleteAgent.Visibility = (SelectedAgent != null && SelectedAgent.ID != 0) ? Visibility.Visible : Visibility.Collapsed;

            // Загрузка списка продуктов
            LoadProducts();

            // Загрузка истории продаж, если агент существует
            if (currentAgent.ID != 0)
            {
                LoadSalesHistory();
            }

            // Установка текущей даты в DatePicker по умолчанию
            SaleDatePicker.SelectedDate = DateTime.Today;
        }


        private void LoadProducts()
        {
            try
            {
                products = Mustafina_glazkiEntities.GetContext().Product.ToList();
                ProductComboBox.ItemsSource = products;

                // Настройка поиска в ComboBox
                ProductComboBox.IsTextSearchEnabled = false; // Отключаем стандартный поиск
                ProductComboBox.StaysOpenOnEdit = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки продуктов: " + ex.Message);
            }
        }

        private void ProductComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string searchText = comboBox.Text;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                comboBox.ItemsSource = products;
            }
            else
            {
                var filteredProducts = products
                    .Where(p => p.Title.ToLower().Contains(searchText.ToLower()))
                    .ToList();
                comboBox.ItemsSource = filteredProducts;
            }

            comboBox.IsDropDownOpen = true;
        }

        private void LoadSalesHistory()
        {
            try
            {
                var context = Mustafina_glazkiEntities.GetContext();

                // Загружаем историю продаж для текущего агента
                var sales = context.ProductSale
                    .Where(ps => ps.AgentID == currentAgent.ID)
                    .Include("Product")
                    .ToList();

                salesHistory.Clear();
                foreach (var sale in sales)
                {
                    salesHistory.Add(new SaleHistoryItem
                    {
                        Product = sale.Product,
                        ProductCount = sale.ProductCount,  
                        SaleDate = sale.SaleDate           
                    });
                }

                SalesListView.ItemsSource = null;
                SalesListView.ItemsSource = salesHistory;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки истории продаж: " + ex.Message);
            }
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
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(currentAgent.Title))
                errors.AppendLine("Укажите наименование агента");
            if (string.IsNullOrWhiteSpace(currentAgent.Address))
                errors.AppendLine("Укажите адрес агента");
            if (string.IsNullOrWhiteSpace(currentAgent.DirectorName))
                errors.AppendLine("Укажите ФИО директора");
            if (ComboTYpe.SelectedItem == null)
                errors.AppendLine("Укажите тип агента");
            else
            {
                // Получаем выбранный элемент ComboBoxItem
                var selectedItem = ComboTYpe.SelectedItem as ComboBoxItem;
                if (selectedItem != null && selectedItem.Tag != null)
                {
                    currentAgent.AgentTypeID = int.Parse(selectedItem.Tag.ToString());
                }
                else
                {
                   
                    currentAgent.AgentTypeID = ComboTYpe.SelectedIndex + 1;
                }
            }

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

            var context = Mustafina_glazkiEntities.GetContext();

            try
            {
                // Сначала сохраняем агента
                if (currentAgent.ID == 0)
                    context.Agent.Add(currentAgent);

                context.SaveChanges(); // Сохраняем агента, чтобы получить ID

                // Теперь сохраняем продажи
                SaveSalesHistory(context);

                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.Navigate(new agentPage());
            }
            catch (Exception ex)
            {
                // Получаем полную информацию об ошибке
                string fullErrorMessage = ex.Message;

                // Рекурсивно получаем все внутренние исключения
                Exception currentException = ex;
                while (currentException.InnerException != null)
                {
                    currentException = currentException.InnerException;
                    fullErrorMessage += "\n\n" + currentException.Message;

                    
                    if (currentException is System.Data.SqlClient.SqlException sqlEx)
                    {
                        fullErrorMessage += $"\nНомер ошибки SQL: {sqlEx.Number}";
                        fullErrorMessage += $"\nСтрока: {sqlEx.LineNumber}";
                        fullErrorMessage += $"\nПроцедура: {sqlEx.Procedure}";
                    }
                }

                MessageBox.Show(fullErrorMessage, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveSalesHistory(Mustafina_glazkiEntities context)
        {
            try
            {
                // Получаем существующие продажи для агента
                var existingSales = context.ProductSale.Where(ps => ps.AgentID == currentAgent.ID).ToList();

                // Удаляем старые продажи
                foreach (var sale in existingSales)
                {
                    context.ProductSale.Remove(sale);
                }

                // Получаем максимальный ID из всех записей
                int maxId = 0;
                if (context.ProductSale.Any())
                {
                    maxId = context.ProductSale.Max(ps => ps.ID);
                }

                // Добавляем новые продажи
                foreach (var saleItem in salesHistory)
                {
                    maxId++;

                    var newSale = new ProductSale
                    {
                        ID = maxId,
                        ProductID = saleItem.Product.ID,
                        AgentID = currentAgent.ID,
                        SaleDate = saleItem.SaleDate,
                        ProductCount = saleItem.ProductCount
                    };
                    context.ProductSale.Add(newSale);
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении истории продаж: {ex.Message}", ex);
            }
        }



        private void DeleteSaleBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var saleToRemove = button?.Tag as SaleHistoryItem;

            if (saleToRemove != null)
            {
                if (MessageBox.Show($"Удалить продажу продукта \"{saleToRemove.Product?.Title}\" от {saleToRemove.SaleDate:dd.MM.yyyy}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    salesHistory.Remove(saleToRemove);
                    RefreshListView();
                }
            }   
        }

        private void AddSaleBtn_Click(object sender, RoutedEventArgs e)
        {
            // Проверка выбранного продукта
    if (ProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукт!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка количества
            if (string.IsNullOrWhiteSpace(CountTextBox.Text))
            {
                MessageBox.Show("Введите количество!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CountTextBox.Text, out int count) || count <= 0)
            {
                MessageBox.Show("Введите корректное количество (положительное число)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка даты
            if (SaleDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату продажи!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime saleDate = SaleDatePicker.SelectedDate.Value;

            var selectedProduct = (Product)ProductComboBox.SelectedItem;

            // Проверка на дублирование (опционально)
            var existingSale = salesHistory.FirstOrDefault(s => s.Product.ID == selectedProduct.ID && s.SaleDate.Date == saleDate.Date);
            if (existingSale != null)
            {
                if (MessageBox.Show("Продажа этого продукта на выбранную дату уже существует. Обновить количество?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    existingSale.ProductCount = count;
                    RefreshListView();
                    ClearSaleForm();
                    return;
                }
            }

            // Добавление новой продажи
            salesHistory.Add(new SaleHistoryItem
            {
                Product = selectedProduct,
                ProductCount = count,
                SaleDate = saleDate
            });

            RefreshListView();
            ClearSaleForm();

            MessageBox.Show("Продажа добавлена в список. Не забудьте сохранить изменения!",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void RefreshListView()
        {
            SalesListView.ItemsSource = null;
            SalesListView.ItemsSource = salesHistory;
        }

        private void ClearSaleForm()
        {
            ProductComboBox.SelectedItem = null;
            ProductComboBox.Text = string.Empty;
            CountTextBox.Text = string.Empty;
            SaleDatePicker.SelectedDate = DateTime.Today;
        }

        private void ProductComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string searchText = comboBox.Text;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                comboBox.ItemsSource = products;
            }
            else
            {
                var filteredProducts = products
                    .Where(p => p.Title.ToLower().Contains(searchText.ToLower()))
                    .ToList();
                comboBox.ItemsSource = filteredProducts;
            }

            comboBox.IsDropDownOpen = true;
            
        }
    }
}
