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
using Word = Microsoft.Office.Interop.Word;

namespace diplom2
{
    /// <summary>
    /// Логика взаимодействия для T.xaml
    /// </summary>
    public partial class T : Page
    {
        private DP2Entities _context = new DP2Entities();
        public T()
        {
            InitializeComponent();
            var allKat = DP2Entities.GetContext().Status.ToList();
            allKat.Insert(0, new Status { Name = "Все заказы" });
            CBT.ItemsSource = allKat;
            CBT.SelectedIndex = 0;

            UpdateTex();
        }

        private void UpdateTex()
        {
            var currentT = DP2Entities.GetContext().Tex.ToList();

            int SelectedST = Convert.ToInt32(CBT.SelectedIndex);
            if (CBT.SelectedIndex>0)
                currentT = currentT.Where(p => p.Status == SelectedST).ToList();


            currentT = currentT.Where(p => p.Nomer.ToString().Contains(TBST.Text.ToLower())).ToList();

            DGridKV.ItemsSource = currentT.OrderBy(p => p.Nomer).ToList();

        }

        private void AddT_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddT());
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                DP2Entities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                DGridKV.ItemsSource = DP2Entities.GetContext().Tex.ToList();
            }
        }

        private void DeleteT_Click(object sender, RoutedEventArgs e)
        {
            var TexForRemoving = DGridKV.SelectedItems.Cast<Tex>().ToList();

            if (MessageBox.Show($"Вы точно хотите удалить следующие {TexForRemoving.Count()} элементов?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DP2Entities.GetContext().Tex.RemoveRange(TexForRemoving);
                    DP2Entities.GetContext().SaveChanges();
                    MessageBox.Show("Данные удалены");

                    DGridKV.ItemsSource = DP2Entities.GetContext().Tex.ToList();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new EditT((sender as Button).DataContext as Tex));
        }

        private void CBT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTex();
        }

        private void TBST_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTex();
        }

        private void OtchetT_Click(object sender, RoutedEventArgs e)
        {
            var allT = _context.Tex.ToList();
            var allS = _context.Status.ToList();

            var application = new Word.Application();
            Word.Document document = application.Documents.Add();

            Word.Paragraph PP = document.Paragraphs.Add();
            Word.Range RR = PP.Range;
            DateTime date = DateTime.Today;
            RR.Text = $"от {date.ToString("dd.MM.yyyy")}";
            RR.set_Style("Обычный");
            RR.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            RR.InsertParagraphAfter();

            PP = document.Paragraphs.Add();
            Word.Range RR2 = PP.Range;
            RR2.Text = "Техника на складе";
            RR2.Font.Bold = 1;
            RR2.Font.Size = 14;
            RR2.Font.Name = ("Arial");
            RR2.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            RR2.InsertParagraphAfter();

            foreach (var status in allS)
            {
                Word.Paragraph kompParagraph = document.Paragraphs.Add();
                Word.Range kompRange = kompParagraph.Range;
                kompRange.Text = status.Name;
                kompRange.Font.Bold = 1;
                kompRange.Font.Size = 12;
                kompRange.Font.Name = ("Arial");
                kompRange.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                kompRange.InsertParagraphAfter();

                Word.Paragraph tableP = document.Paragraphs.Add();
                Word.Range tableR = tableP.Range;
                Word.Table kompTable = document.Tables.Add(tableR, 1, 4);
                kompTable.Borders.InsideLineStyle = kompTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                kompTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                Word.Range cellR;
                cellR = kompTable.Cell(1, 1).Range;
                cellR.Text = "Номер заказа";
                cellR = kompTable.Cell(1, 2).Range;
                cellR.Text = "Описание неисправностей";
                cellR = kompTable.Cell(1, 3).Range;
                cellR.Text = "Тип устройства";
                cellR = kompTable.Cell(1, 4).Range;
                cellR.Text = "Примерная стоимость";

                kompTable.Rows[1].Range.Bold = 1;
                kompTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                for (int i = 0; i < allT.Count(); i++)
                {

                    var currentT = allT[i];
                    if (currentT.Status == status.ID_Status)
                    {   kompTable.Rows.Add();
                        cellR = kompTable.Cell(i + 2, 1).Range;
                        cellR.Text = currentT.Nomer.ToString();
                        cellR = kompTable.Cell(i + 2, 2).Range;
                        cellR.Text = currentT.Opisanie;
                        cellR = kompTable.Cell(i + 2, 3).Range;
                        cellR.Text = currentT.Type1.Name.ToString();
                        cellR = kompTable.Cell(i + 2, 4).Range;
                        cellR.Text = currentT.Price.ToString();                    
                        
                    }
                    //else i++;
                }

                if (status != allS.LastOrDefault())
                    document.Words.Last.InsertBreak(Word.WdBreakType.wdLineBreak);

                application.Visible = true;

                document.SaveAs2(@"C:\Test.docx");
                document.SaveAs2(@"C:\Test.pdf", Word.WdExportFormat.wdExportFormatPDF);
            }
        }
    }
}
