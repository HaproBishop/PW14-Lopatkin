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
        }/// <summary>
         /// Поле, которое указывает на загруженность конфига в главное окно
         /// P.S. Используется для проверки на выгрузку данных из массива в таблицу, который обновляется
         /// в данном классе
         /// </summary>
        public bool _loadedСfg;
        Configuration _cfg;//Переменная конфигурации описана
        bool _savedСfg;//Проверка на выполненное сохранение конфига, которая используется перед выходом из окна настроек
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            _cfg = new Configuration();
            _cfg.LoadConfig();         //Выгрузка из файла данных в объект _cfg   
            RowLength.Text = _cfg.RowLength.ToString();//Считывание значений из свойств объекта
            ColumnLength.Text = _cfg.ColumnLength.ToString();
            if (_cfg.TryTip) MessageBox.Show("Здесь можно настроить размер таблицы по умолчанию при открытии программы", "Подсказка", MessageBoxButton.OK, MessageBoxImage.Information);
            if (_cfg.FirstOpenSettings)//Проверка на первый запуск (свойство в объекте)
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
            TryTip.IsChecked = _tryTipFromFile = _cfg.TryTip;
            _savedСfg = true;//Задание true для отсутствия вопроса при закрытии программы
        }

        private void SaveCfg_Click(object sender, RoutedEventArgs e)
        {
            bool proverowlength = int.TryParse(RowLength.Text, out int rowlength);
            bool provecolumnlength = int.TryParse(ColumnLength.Text, out int columnlength);
            if (proverowlength && provecolumnlength && rowlength >= 0 && columnlength >= 0)
            {
                _tryTipFromFile = _cfg.TryTip;
                _cfg.SaveConfig(rowlength, columnlength);  //Сохранение в файл данной конфигурации
                MessageBox.Show("Конфигурация сохранена", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Необходимо число больше или равное 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ColumnLength.Text = _cfg.ColumnLength.ToString();
                RowLength.Text = _cfg.RowLength.ToString();                
            }
            _savedСfg = true;            
        }

        private void LoadCfg_Click(object sender, RoutedEventArgs e)
        {
            try//Используется в случае ошибки конвертации в Int32
            {//Проверка на совпадение значений перед выгрузкой данных
                if (_cfg.RowLength == Convert.ToInt32(RowLength.Text) && _cfg.ColumnLength == Convert.ToInt32(ColumnLength.Text))
                {
                    _loadedСfg = true;//Задание переменной с типом bool значение успешности (Следует из названия)
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
        bool _tryTipFromFile;//Повторить подсказку из файла (дословный перевод по логике разработчика)
        bool _previousSavedCfg;//Предыдущее состояние конфига перед вызовом события TryTip_Click(задается относительно условия ниже)
        private void TryTip_Click(object sender, RoutedEventArgs e)
        {
            if((!_previousSavedCfg) && _savedСfg) _previousSavedCfg = _savedСfg;//Задание предыдущего состояние, если требуется
                                                                                              //P.S.Логика: если не было изменений(true), то _savedCfg требует восстановление, с учетом)
        //Проверяется _previousSavedCfg на наличие false, чтобы не делать лишнего присваивания 
            _savedСfg = false;
            if (TryTip.IsChecked == true) _cfg.TryTip = true;//Синхронизация значений галочки со свойством 
            else _cfg.TryTip = false;
            if (_previousSavedCfg && (TryTip.IsChecked == _tryTipFromFile)) _savedСfg = true;
        }


        private void RowLength_TextChanged(object sender, TextChangedEventArgs e)
        {
            _previousSavedCfg = _savedСfg = false;//Изменение значений при изменении полей строк и столбцов
        }//При сохранении true для _savedCfg, а _previousSavedCfg оставляет текущее значение для следующей установки галочки
    

        private void SettingsWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_savedСfg)
            {
                MessageBoxResult result = MessageBox.Show("Хотите сохранить изменения перед выходом? Настройки сбросятся до " +
                    "предыдущего состояния, так как вы не нажали клавишу сохранить.", "Сохранение и выход", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    RoutedEventArgs aEvent = new RoutedEventArgs();
                    SaveCfg_Click(sender, aEvent);
                }
                if (result == MessageBoxResult.Cancel) e.Cancel = true;
            }
        }
    }
}
