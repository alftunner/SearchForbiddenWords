using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace SearchForbiddenWords
{
    public class WorkWithFilesOrDirectories
    {
        public List<string> report = new List<string>();
        public WorkWithFilesOrDirectories()
        {

        }
        public void SearchFiles(string path, string[] words, string pathFinal)
        {
            if (Directory.Exists(path))
            {
                string[] subDirs = Directory.GetDirectories(path);
                foreach (var dir in subDirs)
                {
                    if(dir != pathFinal)
                    {
                        try
                        {
                            string[] subDirsInner = Directory.GetDirectories(dir);
                            if (subDirsInner.Length > 0)
                            {
                                SearchFiles(dir, words, pathFinal);
                            }
                            string[] files = Directory.GetFiles(dir);
                            foreach (var file in files)
                            {
                                string[] fileNameArr = file.Split(".");
                                var fileExt = fileNameArr[fileNameArr.Length - 1];
                                if (fileExt == "txt" || fileExt == "doc" || fileExt == "docx")
                                {
                                    using (FileStream fstream = File.OpenRead(file))
                                    {
                                        byte[] array = new byte[fstream.Length];
                                        // считываем данные
                                        fstream.Read(array, 0, array.Length);
                                        // декодируем байты в строку
                                        string textFromFile = System.Text.Encoding.Default.GetString(array);
                                        foreach (var word in words)
                                        {
                                            if (textFromFile.Contains(word))
                                            {
                                                int amount = new Regex(word).Matches(textFromFile).Count;
                                                string reportStr = @$"В файле - {file}|Найдено слово {word}|Количество вхождений - {amount}|---------------------------";
                                                report.Add(reportStr);
                                                FileInfo fileInf = new FileInfo(file);
                                                fileInf.CopyTo(@$"{pathFinal}\{fileInf.Name}", true);
                                                fileInf.CopyTo(@$"{pathFinal}\ReplacE{fileInf.Name}", true);
                                                StreamReader reader = new StreamReader(@$"{pathFinal}\ReplacE{fileInf.Name}");
                                                string content = reader.ReadToEnd();
                                                reader.Close();

                                                content = Regex.Replace(content, word, "*******");

                                                StreamWriter writer = new StreamWriter(@$"{pathFinal}\ReplacE{fileInf.Name}");
                                                writer.Write(content);
                                                writer.Close();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                       
                }
                try
                {
                    string[] files = Directory.GetFiles(path);
                    foreach (var file in files)
                    {
                        string[] fileNameArr = file.Split(".");
                        var fileExt = fileNameArr[fileNameArr.Length - 1];
                        if (fileExt == "txt" || fileExt == "doc" || fileExt == "docx")
                        {
                            using (FileStream fstream = File.OpenRead(file))
                            {
                                byte[] array = new byte[fstream.Length];
                                // считываем данные
                                fstream.Read(array, 0, array.Length);
                                // декодируем байты в строку
                                string textFromFile = System.Text.Encoding.Default.GetString(array);
                                foreach (var word in words)
                                {
                                    if (textFromFile.Contains(word))
                                    {
                                        int amount = new Regex(word).Matches(textFromFile).Count;
                                        string reportStr = @$"В файле - {file}|Найдено слово {word}|Количество вхождений - {amount}|---------------------------";
                                        report.Add(reportStr);
                                        FileInfo fileInf = new FileInfo(file);
                                        fileInf.CopyTo(@$"{pathFinal}\{fileInf.Name}", true);
                                        fileInf.CopyTo(@$"{pathFinal}\ReplacE{fileInf.Name}", true);
                                        StreamReader reader = new StreamReader(@$"{pathFinal}\ReplacE{fileInf.Name}");
                                        string content = reader.ReadToEnd();
                                        reader.Close();

                                        content = Regex.Replace(content, word, "*******");

                                        StreamWriter writer = new StreamWriter(@$"{pathFinal}\ReplacE{fileInf.Name}");
                                        writer.Write(content);
                                        writer.Close();
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> report = new List<string>();
        public string pathToDownload;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void wordsInput_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            wordsInput.Text = "";
        }
        private void pathDirectoryInput_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            pathDirectoryInput.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] forbiddenWords = wordsInput.Text.Split(' ');
            string path = pathDirectoryInput.Text;
            pathToDownload = @$"{path}\report.txt";
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var worker = new WorkWithFilesOrDirectories();
            foreach (var drive in allDrives)
            {
                new Thread(() => {
                    worker.SearchFiles(drive.RootDirectory.ToString(), forbiddenWords, path);
                    using (FileStream fstream = new FileStream(@$"{path}\report.txt", FileMode.Append, FileAccess.Write))
                    {
                        foreach (var reportStr in worker.report)
                        {
                            var subReportStr = reportStr.Split("|");
                            foreach (var item in subReportStr)
                            {
                                // преобразуем строку в байты
                                byte[] array = System.Text.Encoding.Default.GetBytes(item + Environment.NewLine);
                                // запись массива байтов в файл
                                fstream.Write(array, 0, array.Length);
                            }
                        }
                    }
                }).Start();
            }
            Thread.Sleep(20000);
            download.IsEnabled = true;
        }
        private void Button_Click_report(object sender, RoutedEventArgs e)
        {
            Process.Start("C:\\Windows\\System32\\notepad.exe", pathToDownload);
        }
    }
}
