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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mustafina_glazki_s
{
    /// <summary>
    /// Логика взаимодействия для agentPage.xaml
    /// </summary>
    public partial class agentPage : Page
    {
        public agentPage()
        {
            InitializeComponent();
            var currentAgent = Mustafina_glazkiEntities.GetContext().Agent.ToList();
            ListViewAgent.ItemsSource= currentAgent;
            ComdoType.SelectedIndex=0;
            ComboSort.SelectedIndex=0;
            Upd();
           
        }

        private void Upd()
        {
            //типы
            var currentAgent = Mustafina_glazkiEntities.GetContext().Agent.ToList();
            if (ComdoType.SelectedIndex==1)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "МФО").ToList();

            }
            if (ComdoType.SelectedIndex==2)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "ООО").ToList();
            }
            if (ComdoType.SelectedIndex==3)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "ЗАО").ToList();
            }
            if (ComdoType.SelectedIndex==4)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "МКК").ToList();
            }
            if (ComdoType.SelectedIndex ==5)
            {
                currentAgent = currentAgent.Where(p => p.AgentType.Title == "ОАО").ToList();
            }
            if (ComdoType.SelectedIndex==6)
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
                currentAgent = currentAgent.OrderBy(p => p.Sales).ToList();
            }
            if (ComboSort.SelectedIndex == 4)
            {
                currentAgent = currentAgent.OrderByDescending(p => p.Sales).ToList(); //по убыванию
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
                return phoneNumber.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", ""); //возвращает очищенный телефон агента и очищенный поисковый запрос 
            }
            //поиск
            currentAgent = currentAgent.Where(p => p.Title.ToLower().Contains(TBoxSearch.Text.ToLower()) || CleanPhoneNumber(p.Phone).Contains(CleanPhoneNumber(TBoxSearch.Text)) ||
            p.Email.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
            ListViewAgent.ItemsSource = currentAgent;  //телефон содержит строку поиска!
 
    
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
            Upd();
        }
    }
}
