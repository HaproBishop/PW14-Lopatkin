using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LibraryOfConfigurationForVisualTableSettings
{
    public class Configuration
    {        
        public bool TryTip { get; set; }
        /// <summary>
        /// Используется для MessageBox, который гласит о предложении убрать постоянное появление подсказки при открытии настроек. 
        /// Также используется для настроек CheckBox
        /// </summary>
        public bool FirstOpenSettings { get; set; }
        /// <summary>
        /// Используется для отображения информации пользователю о наличии ошибки, связанной с данными в конфиге
        /// </summary>        
        public int RowLength { get; set; }
        public int ColumnLength { get; set; }
        public Configuration()
        {
            ClearConfig();
        }
        public void LoadConfig()
        {
            try
            {
                StreamReader reader = new StreamReader("config.ini");
                TryTip = Convert.ToBoolean(reader.ReadLine());
                FirstOpenSettings = Convert.ToBoolean(reader.ReadLine());
                RowLength = Convert.ToInt32(reader.ReadLine());
                ColumnLength = Convert.ToInt32(reader.ReadLine());
                reader.Close();
            }
            catch
            {
                SaveConfig();
            }
        }

        public void ClearConfig()
        {            
            TryTip = true;
            FirstOpenSettings = true;            
        }
        public void SaveConfig()
        {
            StreamWriter writer = new StreamWriter("config.ini");
            writer.WriteLine(TryTip);
            writer.WriteLine(FirstOpenSettings);
            writer.WriteLine(RowLength);
            writer.WriteLine(ColumnLength);
            writer.Close();
        }
        public void SaveConfig(int rowlength, int columnlength)
        {
            RowLength = rowlength;
            ColumnLength = columnlength;
            SaveConfig();
        }
    }
}
