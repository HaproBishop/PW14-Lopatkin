﻿using System;
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
using System.IO;
using Microsoft.Win32;
using LibMas;
using VisualTable;
using FindCountMoreAvgColumnLibrary;
using System.Windows.Threading;
using LibraryOfConfigurationForVisualTableSettings;
using System.Data;
//Практическая работа №13. Лопаткин Сергей ИСП-31
//Задание №8. Дана матрица размера M * N. В каждом ее столбце найти количество элементов, 
//больших среднего арифметического всех элементов этого столбца
namespace PW13
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer cell = new DispatcherTimer();//Таймер для StatusBar
            cell.Tick += Cell_Tick;
            cell.Interval = new TimeSpan(0, 0, 0, 0, 200);
            cell.IsEnabled = true;
        }
        private void Cell_Tick(object sender, EventArgs e)
        {
            string _defaultcurrentcell = "Ячейка не выбрана";
            if (WorkMas._dmas != null)
            {
                string _linelength = "";
                if (VisualTable.SelectedIndex == -1 || VisualTable.CurrentCell.Column == null) CurrentCell.Text = _defaultcurrentcell;
                else CurrentCell.Text = (VisualTable.CurrentCell.Column.DisplayIndex + 1).ToString() + " столбец / " + (VisualTable.SelectedIndex + 1) + " строка";
                if (WorkMas._dmas.GetLength(1) <= 4 && WorkMas._dmas.GetLength(1) != 0)
                    _linelength = WorkMas._dmas.GetLength(1).ToString() + " столбца / ";
                else
                    _linelength = WorkMas._dmas.GetLength(1).ToString() + " столбцов / ";
                if (WorkMas._dmas.GetLength(0) <= 4 && WorkMas._dmas.GetLength(0) != 0)
                    _linelength += WorkMas._dmas.GetLength(0).ToString() + " строки";
                else
                    _linelength += WorkMas._dmas.GetLength(0).ToString() + " строк";
                TableLength.Text = _linelength;
            }
            else
            {
                CurrentCell.Text = _defaultcurrentcell;
                TableLength.Text = "Таблица не создана";
            }
        }
        public static bool _closeWithoutMessage;
        /// <summary>
        /// Шаблонное открытие таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog
            {
                Title = "Открытие таблицы",
                Filter = "Все файлы (*.*) | *.* | Текстовые файлы | *.txt",
                FilterIndex = 2,
                DefaultExt = "*.txt",
            };

            if (openfile.ShowDialog() == true)
            {
                try
                {
                    WorkMas.Open_File(openfile.FileName); //Обращение к функции с параметром (название текстового файла, в котором хранятся данные)
                    VisualTable.ItemsSource = VisualArray.ToDataTable(WorkMas._dmas).DefaultView; //Отображение данных, считанных с файла
                    ClearResults();
                    VisualArray.ClearUndoAndCancelUndo();
                }
                catch
                {
                    MessageBox.Show("Невозможно считать данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Шаблонное сохранение таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog
            {
                Title = "Сохранение таблицы",
                Filter = "Все файлы (*.*) | *.* | Текстовые файлы | *.txt",
                FilterIndex = 2,
                DefaultExt = "*.txt",
            };

            if (savefile.ShowDialog() == true)
            {
                if (e.Source == SaveMenu || e.Source == SaveToolBar) WorkMas._twomas = true;
                else WorkMas._twomas = false;
                if (WorkMas._dmas != null)
                {
                    WorkMas.Save_File(savefile.FileName); //Обращение к функции с параметром (аналогично предыдущему) 
                    VisualArray.ClearUndoAndCancelUndo();
                }
                else MessageBox.Show("Нет данных для сохранения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Очистка таблицы;
        /// Также производится очистка undo and cancelundo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearTable_Click(object sender, RoutedEventArgs e)
        {
            VisualTable.ItemsSource = WorkMas.ClearTable(); //Обращение к функции "очистки" массива и возвращение null для DataGrid(Очистка таблицы)
            VisualArray.ClearUndoAndCancelUndo();//Обращение к методу очистки undo and cancelundo            
            ClearResults();
        }
        /// <summary>
        /// Создание пустой таблицы, заполненной нулями
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateMas_Click(object sender, RoutedEventArgs e)
        {
            ClearResults();
            bool prv_columns = int.TryParse(CountColumns.Text, out int columns);
            bool prv_rows = int.TryParse(CountRows.Text, out int rows);
            if (prv_columns && prv_rows && columns >= 0 && rows >= 0)
            {
                WorkMas.CreateMas(in rows, in columns);
                VisualArray.ClearUndoAndCancelUndo();
                VisualTable.ItemsSource = VisualArray.ToDataTable(WorkMas._dmas).DefaultView;
            }
            else
            {
                MessageBox.Show("Ошибка. Числа должны быть больше или равны 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                CountRows.Focus();
            }
        }
        /// <summary>
        /// Выход (Закрытие программы)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, RoutedEventArgs e) //Закрытие программы
        {
            Close();
        }
        /// <summary>
        /// Сообщение пользователю о работе программы, а также о разработчике
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutProgram_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Практическая работа №13. Лопаткин Сергей Михайлович. " +
                "Задание №8. Дана матрица размера M * N. В каждом ее столбце найти количество элементов, " +
                "больших среднего арифметического всех элементов этого столбца",
                "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        /// <summary>
        /// Заполнение массива
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Fill_Click(object sender, RoutedEventArgs e)
        {
            ClearResults();
            bool prv_range = int.TryParse(Range.Text, out int range);
            if (WorkMas._dmas != null)
            {
                if (prv_range && range > 0) //2-ое условие - проверка на заполнение без скелета(В нашем случае - проверка на скелет не нужна)
                {
                    WorkMas.FillDMas(in range);//Обращение с передачей информации об диапазоне
                    VisualTable.ItemsSource = VisualArray.ToDataTable(WorkMas._dmas).DefaultView; //Отображение таблицы с заполненными значениями
                }
                else
                {
                    MessageBox.Show("Введен некорректно диапазон значений, необходимо больше 0",
                    "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Range.Focus();
                }
            }
            else MessageForUserAboutTableIsNull();
        }
        /// <summary>
        /// Справка пользователю об особенностях программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Support_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("1) В программе нельзя вводить более трехзначных чисел для диапазона и двухзначных для столбцов и строк.\n" +
                "2) Заполнение происходит от 0 до указанного вами значения\n" +
                "3) Для включения кнопок \"Выполнить\" и \"Заполнить\" необходимо создать таблицу.\n" +
                "4) Пользователю, который НЕ ИМЕЕТ мышки, может воспользоваться горячими клавишами для изменения таблицы. Приведен следующий список:\n" +
                "- Ctrl+S - сохранение исходной таблицы\n" +
                "- Ctrl+O - открытие исходной сохраненной таблицы\n" +
                "- Ctrl+Shift+A(D) - добавление(удаление) нового столбца\n" +
                "- Ctrl+Alt+A(D) - добавление(удаление) новой строки\n" +
                "5) Можно попасть в настройки путем нажатия F2", "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        /// <summary>
        /// Событие окончания изменения значения ячейки 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisualTable_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int iRow = e.Row.GetIndex();
            int iColumn = e.Column.DisplayIndex;
            bool tryedit = int.TryParse(((TextBox)e.EditingElement).Text, out int value);
            if (tryedit)
            {
                WorkMas._dmas[iRow, iColumn] = value;//Занесение значения в двумерный массив(указанная ячейка)
                VisualArray.ReserveTable(WorkMas._dmas);//Резервирование текущей таблицы при успешном изменении значения
                ClearResults();
            }
            else
            {
                MessageBox.Show("Ошибка. Необходимо ввести число, а не символ.", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ((TextBox)e.EditingElement).Text = cell;//Возвращение значения ячейки при неверном вводе
            }
        }
        //Переключение дефолта относительно полученного фокуса
        private void Range_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.Source == Range)
            {
                CreateMas.IsDefault = false;
                Fill.IsDefault = true;
            }
            else
            {
                CreateMas.IsDefault = true;
                Fill.IsDefault = false;
            }
        }
        /// <summary>
        /// Событие нажатия клавиш для окна (В данном случае используется для горячих клавиш)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.O) Open_Click(sender, e);
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.S) Save_Click(sender, e);
            if (e.Key == Key.F1) Support_Click(sender, e);
            if (e.Key == Key.F2) Settings_Click(sender, e);
            if (e.Key == Key.F12) AboutProgram_Click(sender, e);
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                Undo_Click(sender, e);
            }

            if (((e.KeyboardDevice.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) ==
                (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.Z) ^
                (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Y))
            {
                CancelUndo_Click(sender, e);
            }
            if ((e.KeyboardDevice.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) ==
                (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.A)
            {
                AddColumn_Click(sender, e);
            }
            if ((e.KeyboardDevice.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) ==
                (ModifierKeys.Control | ModifierKeys.Alt) && e.Key == Key.A)
            {
                AddRow_Click(sender, e);
            }
            if ((e.KeyboardDevice.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) ==
                (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.D)
            {
                DeleteColumn_Click(sender, e);
            }
            if ((e.KeyboardDevice.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) ==
                (ModifierKeys.Control | ModifierKeys.Alt) && e.Key == Key.D)
            {
                DeleteRow_Click(sender, e);
            }
        }

        private void VisualTable_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = "-1234567890".IndexOf(e.Text) < 0;
        }
        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            if (WorkMas._dmas != null)
            {
                ClearResults();
                VisualTable.ItemsSource = VisualArray.AddNewColumn(ref WorkMas._dmas).DefaultView;
            }
            else MessageForUserAboutTableIsNull();
        }
        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            if (WorkMas._dmas != null)
            {
                ClearResults();
                VisualTable.ItemsSource = VisualArray.AddNewRow(ref WorkMas._dmas).DefaultView;
            }
            else MessageForUserAboutTableIsNull();
        }
        private void DeleteColumn_Click(object sender, RoutedEventArgs e)
        {
                if (VisualTable.CurrentCell.Column != null)
                {
                    ClearResults();
                    VisualTable.ItemsSource = VisualArray.DeleteColumn(ref WorkMas._dmas, VisualTable.CurrentCell.Column.DisplayIndex).DefaultView;
                }
                else MessageForUserAboutUnselectedCell();
        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (VisualTable.SelectedIndex != -1)
            {
                ClearResults();
                VisualTable.ItemsSource = VisualArray.DeleteRow(ref WorkMas._dmas, VisualTable.SelectedIndex).DefaultView;
            }
            else MessageForUserAboutUnselectedCell();
        }
        private void ClearResults()
        {
            AvgOfColumns.ItemsSource = null;
            CountMoreAvgOfColumns.ItemsSource = null;
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (WorkMas._dmas != null)
            {
                VisualArray.ClearUndoAndCancelUndo();
                int[][] result = FindCountMoreAvgColumnClass.FindCountMoreAvgColumn(WorkMas._dmas);
                AvgOfColumns.ItemsSource = VisualArray.ToDataTable(result[0]).DefaultView;
                CountMoreAvgOfColumns.ItemsSource = VisualArray.ToDataTable(result[1]).DefaultView;
                MessageBoxResult saveresult = MessageBox.Show("Вы хотите сохранить результаты среднего арифметического столбцов?", "Сохранение результатов среднего арифметического",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (saveresult == MessageBoxResult.Yes)
                {
                    WorkMas._mas = result[0];
                    Save_Click(sender, e);
                }
                saveresult = MessageBox.Show("Вы хотите сохранить результаты количества значений ячеек, больших среднего" +
                    " арифметического?", "Сохранение результатов количества",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (saveresult == MessageBoxResult.Yes)
                {
                    WorkMas._mas = result[1];
                    Save_Click(sender, e);
                }
            }
            else MessageForUserAboutTableIsNull();
        }/// <summary>
         /// Метод для шаблонного отображения сообщения пользователю о требующейся созданной таблицы для возможности заполнения
         /// </summary>
        public void MessageForUserAboutTableIsNull()
        {
            MessageBox.Show("В несуществующую таблицу нельзя занести данные! Создайте таблицу для заполнения" +
                " ее значениями!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            CountRows.Focus();
        }
        /// <summary>
        /// Метод для шаблонного отображения сообщения пользователю о невыбранной ячейке
        /// </summary>
        public void MessageForUserAboutUnselectedCell()
        {
            MessageBox.Show("Необходимо выбрать ячейку с определенным номером столбца или строки, чтобы произвести удаление!",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        /// <summary>
        /// Отмена изменений в таблице
        /// </summary>
        public void Undo_Click(object sender, RoutedEventArgs e)
        {
            VisualTable.ItemsSource = VisualArray.CancelChanges().DefaultView;
            WorkMas._dmas = VisualArray.SyncData();
        }/// <summary>
         /// Отмена восстановления предыдущего состояния таблицы
         /// </summary>
        public void CancelUndo_Click(object sender, RoutedEventArgs e)
        {
            VisualTable.ItemsSource = VisualArray.CancelUndo().DefaultView;
            WorkMas._dmas = VisualArray.SyncData();//Обязательная синхронизация
        }
        private void MainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_closeWithoutMessage)
            {
                MessageBoxResult result = MessageBox.Show("Вы точно хотите выйти из программы?", "Закрытие программы", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) e.Cancel = false;
                else e.Cancel = true;
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingswin = new SettingsWindow();
            settingswin.Owner = this;
            settingswin.ShowDialog(); 
            if (settingswin._loadedСfg)
            {
                CountRows.Text = WorkMas._dmas.GetLength(0).ToString();
                CountColumns.Text = WorkMas._dmas.GetLength(1).ToString();
                CreateMas_Click(sender, e);
            }
        }
        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            PasswordWindow passwordwin = new PasswordWindow
            {
                Owner = this
            };
            passwordwin.ShowDialog();
            Configuration cfg = new Configuration();
            cfg.LoadConfig();
            WorkMas._dmas = new int[cfg.RowLength, cfg.ColumnLength];
            CountRows.Text = WorkMas._dmas.GetLength(0).ToString();
            CountColumns.Text = WorkMas._dmas.GetLength(1).ToString();
            CreateMas_Click(sender, e);
        }
        string cell;
        private void VisualTable_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            int iColumn = e.Column.DisplayIndex;
            int iRow = e.Row.GetIndex();
            DataRowView row = (DataRowView)VisualTable.Items[iRow];
            cell = row[iColumn].ToString();
        }
    }
}