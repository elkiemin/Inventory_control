using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
using System.Windows.Threading;
using Word = Microsoft.Office.Interop.Word;

namespace diplom2
{
    /// <summary>
    /// Логика взаимодействия для K.xaml
    /// </summary>
    public partial class K : Page
    {
        private DP2Entities _context = new DP2Entities();
        public K()
        {
            InitializeComponent();

            var allKat = DP2Entities.GetContext().Kategoria.ToList();
            allKat.Insert(0, new Kategoria { Name = "Все категории" });
            CBK.ItemsSource = allKat;
            CBK.SelectedIndex = 0;

            UpdateKomp();
        }

        private void UpdateKomp()
        {
            var currentKomp = DP2Entities.GetContext().Komp.ToList();

            int SelectedKat = Convert.ToInt32(CBK.SelectedIndex);
            if (CBK.SelectedIndex > 0)
                currentKomp = currentKomp.Where(p => p.Kategoria == SelectedKat).ToList();

            currentKomp = currentKomp.Where(p => p.Name.ToLower().Contains(TBS.Text.ToLower())).ToList();

            DGridK.ItemsSource = currentKomp.OrderBy(p => p.Name).ToList();

        }

        private void AddK_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddK(null));
        }

        private void DeleteK_Click(object sender, RoutedEventArgs e)
        {
            var KomForRemoving = DGridK.SelectedItems.Cast<Komp>().ToList();

            if (MessageBox.Show($"Вы точно хотите удалить следующие {KomForRemoving.Count()} элементов?","Внимание",MessageBoxButton.YesNo,MessageBoxImage.Question)==MessageBoxResult.Yes)
            {
                try
                {
                    DP2Entities.GetContext().Komp.RemoveRange(KomForRemoving);
                    DP2Entities.GetContext().SaveChanges();
                    MessageBox.Show("Данные удалены");

                    DGridK.ItemsSource = DP2Entities.GetContext().Komp.ToList();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new EditK((sender as Button).DataContext as Komp));
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                DP2Entities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                DGridK.ItemsSource = DP2Entities.GetContext().Komp.ToList();
            }
        }

        private void TBS_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateKomp();
        }

        private void CBK_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateKomp();
        }

        private void OtchetKomp_Click(object sender, RoutedEventArgs e)
        {
            var allKomp = _context.Komp.ToList();
            var allCat = _context.Kategoria.ToList();

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
            RR2.Text = "Комплектующие на складе";
            RR2.Font.Bold = 1;
            RR2.Font.Size = 14;
            RR2.Font.Name = ("Arial");
            RR2.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            RR2.InsertParagraphAfter();

            foreach (var kat in allCat)
            {
                Word.Paragraph kompParagraph = document.Paragraphs.Add();
                Word.Range kompRange = kompParagraph.Range;
                kompRange.set_Style("Заголовок 1");
                kompRange.Text = kat.Name;
                kompParagraph.set_Style("Обычный");
                kompRange.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                kompRange.InsertParagraphAfter();

                Word.Paragraph tableP = document.Paragraphs.Add();
                Word.Range tableR = tableP.Range;
                Word.Table kompTable = document.Tables.Add(tableR, 1, 5);
                kompTable.Borders.InsideLineStyle = kompTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                kompTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                Word.Range cellR;
                cellR = kompTable.Cell(1, 1).Range;
                cellR.Text = "Артикул";
                cellR = kompTable.Cell(1, 2).Range;
                cellR.Text = "Наименование";
                cellR = kompTable.Cell(1, 3).Range;
                cellR.Text = "Количество";
                cellR = kompTable.Cell(1, 4).Range;
                cellR.Text = "Цена закуп. за ед., руб";
                cellR = kompTable.Cell(1, 5).Range;
                cellR.Text = "Сумма, руб";

                kompTable.Rows[1].Range.Bold = 1;
                kompTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                for (int i = 0; i < allKomp.Count(); i++)
                {
                        var currentKomp = allKomp[i];
                    if (currentKomp.Kategoria == kat.ID_K && currentKomp.Quantity != 0)
                        {
                            kompTable.Rows.Add();
                            cellR = kompTable.Cell(i + 2, 1).Range;
                            cellR.Text = currentKomp.Art.ToString();
                            cellR = kompTable.Cell(i + 2, 2).Range;
                            cellR.Text = currentKomp.Name;
                            cellR = kompTable.Cell(i + 2, 3).Range;
                            cellR.Text = currentKomp.Quantity.ToString();
                            cellR = kompTable.Cell(i + 2, 4).Range;
                            cellR.Text = currentKomp.Cost.ToString();
                            var Sum1 = currentKomp.Cost * currentKomp.Quantity;
                            cellR = kompTable.Cell(i + 2, 5).Range;
                            cellR.Text = Sum1.ToString();
                        }
                   

                }

                if (kat != allCat.LastOrDefault())
                    document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);

                application.Visible = true;

                document.SaveAs2(@"C:\Test.docx");
                document.SaveAs2(@"C:\Test.pdf", Word.WdExportFormat.wdExportFormatPDF);
            }
        }

        private void Nakladnaya_Click(object sender, RoutedEventArgs e)
        {
            var allKomp = _context.Komp.ToList();

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
            RR2.Text = "НАКЛАДНАЯ №___";
            RR2.Font.Bold = 1;
            RR2.Font.Size = 16;
            RR2.Font.Name = ("Arial");
            RR2.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            RR2.InsertParagraphAfter();

            PP = document.Paragraphs.Add();
            Word.Range RR3 = PP.Range;
            RR3.Text = "Кому:_______________________________________________________________________________";
            RR3.Font.Bold = 1;
            RR3.Font.Size = 11;
            RR3.Font.Name = ("Times New Roman");
            RR3.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            RR3.InsertParagraphAfter();

            PP = document.Paragraphs.Add();
            Word.Range RR4 = PP.Range;
            RR4.Text = "От кого:_____________________________________________________________________________";
            RR4.Font.Bold = 1;
            RR4.Font.Size = 11;
            RR4.Font.Name = ("Times New Roman");
            RR4.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            RR4.InsertParagraphAfter();

            Word.Paragraph kompParagraph = document.Paragraphs.Add();
            Word.Range kompRange = kompParagraph.Range;

            Word.Paragraph tableP = document.Paragraphs.Add();
            Word.Range tableR = tableP.Range;
            Word.Table kompTable = document.Tables.Add(tableR, 1, 5);
            kompTable.Borders.InsideLineStyle = kompTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            kompTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

            Word.Range cellR;
            cellR = kompTable.Cell(1, 1).Range;
            cellR.Text = "№";
            cellR = kompTable.Cell(1, 2).Range;
            cellR.Text = "Наименование";
            cellR = kompTable.Cell(1, 3).Range;
            cellR.Text = "Количество";
            cellR = kompTable.Cell(1, 4).Range;
            cellR.Text = "Цена, руб";
            cellR = kompTable.Cell(1, 5).Range;
            cellR.Text = "Сумма, руб";

            kompTable.Rows[1].Range.Bold = 1;
            kompTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            var Itog = 0;
            for (int i = 0; i < allKomp.Count(); i++)
            {
                var currentKomp = allKomp[i];
                var nomer = 1;
                DateTime dateT = DateTime.Today;
                if (allKomp[i].Date >= dateT && currentKomp.Date != null)
                {
                    kompTable.Rows.Add();
                    cellR = kompTable.Cell(i + 2, 1).Range;
                    cellR.Text = nomer.ToString();
                    nomer += 1;
                    cellR = kompTable.Cell(i + 2, 2).Range;
                    cellR.Text = currentKomp.Name;
                    cellR = kompTable.Cell(i + 2, 3).Range;
                    cellR.Text = currentKomp.Quantity.ToString();
                    cellR = kompTable.Cell(i + 2, 4).Range;
                    cellR.Text = currentKomp.Cost.ToString();
                    var Sum1 = currentKomp.Cost * currentKomp.Quantity;
                    cellR = kompTable.Cell(i + 2, 5).Range;
                    cellR.Text = Sum1.ToString();
                    Itog += (int)Sum1;
                    }
                    
                }
           
            PP = document.Paragraphs.Add();
            Word.Range RR7 = PP.Range;
            RR7.Text = $"Итого:{Itog.ToString()} руб.";
            RR7.Font.Size = 10;
            RR7.Font.Name = ("Times New Roman");
            RR7.Font.Bold = 1;
            RR7.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            RR7.InsertParagraphAfter();

            PP = document.Paragraphs.Add();
            Word.Range RR5 = PP.Range;
            RR5.Text = "Сдал: _____________   _________________________ Принял: _____________   _________________________";
            RR5.Font.Size = 10;
            RR5.Font.Name = ("Times New Roman");
            RR5.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            RR5.InsertParagraphAfter();

            PP = document.Paragraphs.Add();
            Word.Range RR6 = PP.Range;
            RR6.Text = "                      подпись                                       Ф., И., О.                                                подпись                                            Ф., И., О.";
            RR6.Font.Size = 8;
            RR6.Font.Name = ("Times New Roman");
            RR6.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            RR6.InsertParagraphAfter();
            application.Visible = true;

            document.SaveAs2(@"C:\Test.docx");
            document.SaveAs2(@"C:\Test.pdf", Word.WdExportFormat.wdExportFormatPDF);
            }
    }
}

