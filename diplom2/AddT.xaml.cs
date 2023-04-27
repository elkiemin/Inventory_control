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

namespace diplom2
{
    /// <summary>
    /// Логика взаимодействия для AddT.xaml
    /// </summary>
    public partial class AddT : Page
    {
        private Tex _currentTex = new Tex();   
        public AddT()
        {
            InitializeComponent();
            DataContext = _currentTex;
            ComboBoxStatus.ItemsSource = DP2Entities.GetContext().Status.ToList();
            ComboBoxTip.ItemsSource = DP2Entities.GetContext().Type.ToList();

        }

        private void XT_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new T());
        }

        private void SaveT_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (_currentTex.Nomer < 0)
                errors.AppendLine("Укажите номер");
            if (string.IsNullOrWhiteSpace(_currentTex.Opisanie))
                errors.AppendLine("Опишите причину неисправности");
            // if (_currentKom.Kategoria == null)
            //  errors.AppendLine("Выберите категорию");
            if (_currentTex.Price < 0)
                errors.AppendLine("Укажите стоимость");
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }
            if (_currentTex.ID_Tex == 0)
                DP2Entities.GetContext().Tex.Add(_currentTex);
            try
            {
                DP2Entities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена!");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message.ToString()); }
        }
    }
}
