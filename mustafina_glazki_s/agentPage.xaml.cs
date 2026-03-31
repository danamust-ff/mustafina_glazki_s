using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
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

namespace mustafina_glazki_s
{
    public class DiscountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int discount && discount >= 25)
            {
                return new SolidColorBrush(Colors.LightGreen);
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class agentPage : Page
    {
        public agentPage()
        {
            InitializeComponent();
            LoadAgents();
        }

        private void LoadAgents()
        {
            var currentAgent = Mustafina_glazkiEntities.GetContext().Agent.ToList();
            ComdoType.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
            Upd();
        }

        private void Upd()
        {
            //типы
            var currentAgent = Mustafina_glazkiEntities.GetContext().Agent.ToList();
            if (ComdoType.SelectedIndex == 1)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "МФО").ToList();

            }
            if (ComdoType.SelectedIndex == 2)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "ООО").ToList();
            }
            if (ComdoType.SelectedIndex == 3)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "ЗАО").ToList();
            }
            if (ComdoType.SelectedIndex == 4)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "МКК").ToList();
            }
            if (ComdoType.SelectedIndex == 5)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "ОАО").ToList();
            }
            if (ComdoType.SelectedIndex == 6)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "ПАО").ToList();
            }


            //сортировка (наимен, и тд)
            if (ComboSort.SelectedIndex == 1)
            {
                currentAgent = currentAgent.OrderBy(p => p.Title).ToList();
            }
            if (ComboSort.SelectedIndex == 2)
            {
                currentAgent = currentAgent.OrderByDescending(p => p.Title).ToList();
            }
            if (ComboSort.SelectedIndex == 3)
            {
                currentAgent = currentAgent.OrderBy(p => p.Discount).ToList();
            }
            if (ComboSort.SelectedIndex == 4)
            {
                currentAgent = currentAgent.OrderByDescending(p => p.Discount).ToList();
            }
            if (ComboSort.SelectedIndex == 5)
            {
                currentAgent = currentAgent.OrderBy(p => p.Priority).ToList();
            }
            if (ComboSort.SelectedIndex == 6)
            {
                currentAgent = currentAgent.OrderByDescending(p => p.Priority).ToList();
            }

            //проверка на ввод номера
            string CleanPhoneNumber(string phoneNumber)
            {
                return phoneNumber.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            }
            //поиск
            currentAgent = currentAgent.Where(p => p.Title.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
                CleanPhoneNumber(p.Phone).Contains(CleanPhoneNumber(TBoxSearch.Text)) ||
                p.Email.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            TableList = currentAgent; //сохранение тек результата
            ChangePage(0, 0);
        }

        //страницы
        int CountRecords;
        int CountPage;
        int CurrentPage = 0;
        int RecordsPage = 20;
        List<Agent> CurrentPageList = new List<Agent>();
        List<Agent> TableList;

        private void ChangePage(int direction, int? selectedPage)
        {
            CurrentPageList.Clear();
            CountRecords = TableList.Count;
            CountPage = (CountRecords + RecordsPage - 1) / RecordsPage;

            if (CountPage == 0) CountPage = 1;

            if (selectedPage.HasValue && selectedPage >= 0 && selectedPage < CountPage)
            {
                CurrentPage = (int)selectedPage;
            }
            else
            {
                if (direction == 1 && CurrentPage > 0)
                {
                    CurrentPage--;
                }
                else if (direction == 2 && CurrentPage < CountPage - 1)
                {
                    CurrentPage++;
                }
                else return;
            }

            int startIn = CurrentPage * RecordsPage;
            int endIn = Math.Min(startIn + RecordsPage, CountRecords);
            for (int i = startIn; i < endIn; i++)
            {
                CurrentPageList.Add(TableList[i]);
            }

            PageListBox.Items.Clear();
            for (int i = 1; i <= CountPage; i++)
            {
                PageListBox.Items.Add(i);
            }
            PageListBox.SelectedIndex = CurrentPage;
            ListViewAgent.ItemsSource = CurrentPageList;
            ListViewAgent.Items.Refresh();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Upd();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Upd();
        }

        private void ComdoType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Upd();
        }

     
        private void ListViewAgent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedCount = ListViewAgent.SelectedItems.Count;
            if (selectedCount > 1)
            {
                ChangePriorityBtn.Visibility = Visibility.Visible;
            }
            else
            {
                ChangePriorityBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PageListBox.SelectedItem != null)
            {
                ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
            }
        }

        private void AddAgent_Click_1(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage((sender as Button).DataContext as Agent));
        }

        // Реализация кнопки изменения приоритета
        private async void ChangePriorityBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedAgents = ListViewAgent.SelectedItems.Cast<Agent>().ToList();

            if (selectedAgents.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одного агента!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int maxPriority = 0;
            foreach (var agent in selectedAgents)
            {
                if (agent.Priority > maxPriority)
                {
                    maxPriority = agent.Priority;
                }
            }

            // Создаем модальное окно для ввода нового приоритета
            var inputDialog = new PrioritetWindow1($"Введите новый приоритет для {selectedAgents.Count} агентов:", maxPriority);

            if (inputDialog.ShowDialog() == true)
            {
                int newPriority = inputDialog.Result;

                if (newPriority < 0)
                {
                    MessageBox.Show("Приоритет не может быть отрицательным!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    var context = Mustafina_glazkiEntities.GetContext();

                    foreach (var agent in selectedAgents)
                    {
                        var agentToUpdate = context.Agent.FirstOrDefault(a => a.ID == agent.ID);
                        if (agentToUpdate != null)
                        {
                            agentToUpdate.Priority = newPriority;

                            
                            var priorityHistory = new AgentPriorityHistory
                            {
                                AgentID = agentToUpdate.ID,
                               
                                PriorityValue = newPriority,

                                ChangeDate = DateTime.Now
                            };

                            // Добавляем в контекст
                            context.AgentPriorityHistory.Add(priorityHistory);
                        }
                    }

                    await context.SaveChangesAsync();

                    // Обновляем интерфейс
                    LoadAgents();
                    MessageBox.Show($"Приоритет {selectedAgents.Count} агентов успешно изменен на {newPriority}!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}