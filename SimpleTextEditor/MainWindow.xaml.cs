using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleTextEditor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string fileName;
        public MainWindow()
        {
            InitializeComponent();
            
            comboFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            comboFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        }

        private void ReachBoxEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object temp = reachBoxEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
            buttonBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
            temp = reachBoxEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
            buttonItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
            temp = reachBoxEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            buttonUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

            temp = reachBoxEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            comboFontFamily.SelectedItem = temp;
            temp = reachBoxEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
            comboFontSize.Text = temp.ToString();
        }

        #region Открытие файла
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";
                if (dialog.ShowDialog() == true)
                {
                    FileStream fileStream = new FileStream(dialog.FileName, FileMode.Open);
                    fileName = Path.GetFileName(dialog.FileName);
                    TextRange range = new TextRange(reachBoxEditor.Document.ContentStart, reachBoxEditor.Document.ContentEnd);
                    range.Load(fileStream, DataFormats.Text);
                }

                int autoSaveInterval = 10;
                CreateTimer(autoSaveInterval);
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Сохранить как файл
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";
                if (dialog.ShowDialog() == true)
                {
                    FileStream fileStream = new FileStream(dialog.FileName, FileMode.Create);
                    TextRange range = new TextRange(reachBoxEditor.Document.ContentStart, reachBoxEditor.Document.ContentEnd);
                    range.Save(fileStream, DataFormats.Text);
                }
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Выбор шрифта
        private void ComboFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboFontFamily.SelectedItem != null)
                reachBoxEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, comboFontFamily.SelectedItem);
        }

        private void ComboFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            reachBoxEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, comboFontSize.Text);
        }
        #endregion

        #region Автосохранение файла в папке пользователя
        private void CreateTimer(int seconds)
        {
            Task.Factory.StartNew(() =>
            {
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Elapsed += AutoSaveTimer_Tick;
                timer.Interval = seconds * 1000;
                timer.Start();
            });
        }
        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                // do whatever you want to do with shared object.
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                    string content = GetContent();

                    SaveContent(content, path + "/" + fileName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                //Other wise re-invoke the method with UI thread access
                Application.Current.Dispatcher.Invoke(new System.Action(() => AutoSaveTimer_Tick(this, EventArgs.Empty)));
            }
        }
        
        private string GetContent()
        {
            var textRange = new TextRange(reachBoxEditor.Document.ContentStart, reachBoxEditor.Document.ContentEnd);

            return textRange.Text;
        }
        private void SaveContent(string content, string filePath)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    byte[] array = System.Text.Encoding.Default.GetBytes(content);
                    stream.Write(array, 0, array.Length);
                }
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
