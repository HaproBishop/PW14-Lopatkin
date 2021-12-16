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
using LibMas;
using LibraryOfConfigurationForVisualTableSettings;

namespace PW13
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }
        public bool _loadedcfg;
        Configuration _cfg;
        bool _savedcfg;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            _cfg = new Configuration();
            _cfg.LoadConfig();            
            RowLength.Text = _cfg.RowLength.ToString();
            ColumnLength.Text = _cfg.ColumnLength.ToString();
            if (_cfg.TryTip)
            {
                TryTip.IsChecked = true;
                MessageBox.Show("Здесь можно настроить размер таблицы по умолчанию при открытии программы", "Подсказка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else TryTip.IsChecked = false;            
            if (_cfg.FirstOpenSettings)
            {                                
                MessageBoxResult resultoftry = MessageBox.Show("Вы хотите видеть подсказку при каждом открытии этого окна? Позже можно изменить настройки.", "Повторное открытие подсказки", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (resultoftry == MessageBoxResult.Yes) _cfg.TryTip = true;
                else
                {
                    _cfg.TryTip = false;
                }
                _cfg.FirstOpenSettings = false;
                SaveCfg_Click(sender, e);
            }
            _savedcfg = true;
        }

        private void SaveCfg_Click(object sender, RoutedEventArgs e)
        {
            bool proverowlength = int.TryParse(RowLength.Text, out int rowlength);
            bool provecolumnlength = int.TryParse(ColumnLength.Text, out int columnlength);
            if (proverowlength && provecolumnlength && rowlength >= 0 && columnlength >= 0)
            {
                _cfg.SaveConfig(rowlength, columnlength);                
                MessageBox.Show("Конфигурация сохранена", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Необходимо число больше или равное 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ColumnLength.Text = _cfg.ColumnLength.ToString();
                RowLength.Text = _cfg.RowLength.ToString();                
            }
            _savedcfg = true;            
        }

        private void LoadCfg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_cfg.RowLength == Convert.ToInt32(RowLength.Text) && _cfg.ColumnLength == Convert.ToInt32(ColumnLength.Text))
                {
                    _loadedcfg = true;
                    WorkMas._dmas = new int[_cfg.RowLength, _cfg.ColumnLength];
                }
                else MessageBox.Show("Для отображения таблицы необходимо синхронизировать данные", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Stop);
            }
            catch
            {
                MessageBox.Show("Введите число!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RowLength_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = "1234567890".IndexOf(e.Text) < 0;
        }

        private void TryTip_Click(object sender, RoutedEventArgs e)
        {
            _savedcfg = false;
            if (TryTip.IsChecked == true) _cfg.TryTip = true;
            else _cfg.TryTip = false;
        }


        private void RowLength_TextChanged(object sender, TextChangedEventArgs e)
        {
            _savedcfg = false;
        }

        private void SettingsWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_savedcfg)
            {
                MessageBoxResult result = MessageBox.Show("Хотите сохранить изменения перед выходом? Настройки сбросятся до " +
                    "предыдущего состояния, так как вы не нажали клавишу сохранить.", "Сохранение и выход", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    RoutedEventArgs ie = new RoutedEventArgs();
                    SaveCfg_Click(sender, ie);
                }
                if (result == MessageBoxResult.Cancel) e.Cancel = true;
            }
        }
    }
}
