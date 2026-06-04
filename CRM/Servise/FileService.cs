using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CRM.Servise
{
    public enum FilePath
    {
        Users,
        Employers
    }
    public class FileService
    {
        public static FileService Instance { get; private set; }=new FileService();

        private FileService()
        {
            Instance = this;
        }

        public void Save<T>(ObservableCollection<T> collection, FilePath filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            using (FileStream fs = new FileStream(filePath.ToString() +".json", FileMode.Create))
            {
                JsonSerializer.Serialize<ObservableCollection<T>>(fs, collection, options);
            }
        }
        //ПРОБЛЕМА  Секьюреит криво загружается
        public void Download<T>(ObservableCollection<T> collection, FilePath filePath)
        {
            ObservableCollection<T> loadEmployer = new ObservableCollection<T>();
            //Если файла нет-создать
            using (FileStream fs = new FileStream(filePath.ToString() + ".json", FileMode.OpenOrCreate))
            {
                collection.Clear();
                if (fs.Length > 0) loadEmployer = JsonSerializer.Deserialize<ObservableCollection<T>>(fs);
                else return;
            }
            if (loadEmployer.Count > 0)
            {
                foreach (var i in loadEmployer)
                {
                    collection.Add(i);
                }
            }

        }
    }
}
