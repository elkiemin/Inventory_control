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
    /// Логика взаимодействия для AddK.xaml
    /// </summary>
    public partial class AddK : Page
    {
        private Komp _currentKom = new Komp ();
        public AddK(Komp selectedKomp)
        {
            InitializeComponent();

            DataContext = _currentKom;
            ComboBoxKategoria.ItemsSource = DP2Entities.GetContext().Kategoria.ToList();
            
        }

        private void XK_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new K());
        }

        private void SaveK_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (_currentKom.Art < 0)
                errors.AppendLine("Укажите артикул");
            if (string.IsNullOrWhiteSpace(_currentKom.Name))
                errors.AppendLine("Укажите наименование");
           // if (_currentKom.Kategoria == null)
              //  errors.AppendLine("Выберите категорию");
            if (_currentKom.Price < 0)
                errors.AppendLine("Укажите стоимость");
            if (_currentKom.Cost < 0)
                errors.AppendLine("Укажите себестоимость");
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }
            
            if (_currentKom.ID_Komplect == 0)
                _currentKom.Date = DateTime.Now;
                DP2Entities.GetContext().Komp.Add(_currentKom);
                
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
