using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Xml.Linq;

namespace EldenRingSaves
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _archivePath = string.Empty;
        private string _saveFolderPath = "EMPTY";
        private string archiveFolderName = "";
        private string oriArchiveName = "";
        private string _selectArchiveName = "";
        private List<string> archiveFolderNames = new();

        private ListBoxItem delItem = new();
        public string selectArchiveName
        {
            get
            {
                return _selectArchiveName;
            }
            set
            {
                _selectArchiveName = value;
                changeButton.Content = "Change to:" + value;
                delButton.Content = "DeleteArhive: " + value;
            }
        }
        public string archivePath
        {
            get
            {
                return _archivePath;
            }
            set
            {
                _archivePath = value;
                archivePathText.Text = _archivePath;
            }
        }

        public string saveFolderPath
        {
            get
            {
                return _saveFolderPath;
            }
            set
            {
                _saveFolderPath = value;
                savePathText.Text = _saveFolderPath;
            }
        }

        public readonly string configPath = "inisetting.txt";

        public MainWindow()
        {
            InitializeComponent();
            if (!File.Exists(configPath)) SetArchivePath("初始化，请选择存档文件夹");
            else
            {
                FileStream fs = new(configPath, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                archivePath = sr.ReadLine() ?? "C:\\Users\\Public";
                saveFolderPath = sr.ReadLine() ?? "C:\\Users";
                string currentArchiveFolder;
                while ((currentArchiveFolder = sr.ReadLine() ?? string.Empty) != string.Empty)
                {
                    if (currentArchiveFolder == "ArchiveFolders:") continue;

                    archiveFolderNames.Add(currentArchiveFolder);
                    ListBoxItem item = new ListBoxItem();
                    item.Content = currentArchiveFolder;
                    item.Selected += OnSelectItem;
                    lstNames.Items.Add(item);
                }
                sr.Close();
            }
            Start();
        }

        public void Start()
        {
            if((!archivePath.Contains("EldenRing\\")))
            {
                SetArchivePath("请选择正确的法环文件夹中的主存档文件夹");
            }

            while (saveFolderPath == "EMPTY" || !Directory.Exists(saveFolderPath))
            {
                SetSaveFolderPath("初始化，请选择备份存档路径");
            }
            txtName.Text = "此处输入存档名称";

            GetFolderAndArchivePath(archivePath, out archiveFolderName, out oriArchiveName);
        }
        private void OnSelectPathButton(object sender, RoutedEventArgs e)
        {
            SetArchivePath("请选择存档文件夹");
            if (!archivePath.Contains("EldenRing\\"))
            {
                SetArchivePath("请选择正确的法环文件夹中的主存档文件夹");
            }
        }
        private void OnSelectSavePathButton(object sender, RoutedEventArgs e)
        {
            SetSaveFolderPath("请选择拷贝文件夹");
        }
        private void OnSelectItem(object sender, RoutedEventArgs e)
        {
            delItem = (ListBoxItem)sender;
            selectArchiveName = delItem.Content.ToString() ?? "";
        }
        private void OnSaveButton(object sender, RoutedEventArgs e)
        {
            if ((!archivePath.Contains("EldenRing\\")))
            {
                SetArchivePath("请选择正确的法环文件夹中的主存档文件夹");
                return;
            }
            if (!string.IsNullOrWhiteSpace(txtName.Text))
            {
                if (archiveFolderNames.Contains(txtName.Text) || Directory.Exists(archivePath + txtName))
                {
                    MessageBox.Show("Already has the folder!!!");
                    txtName.Text = "";
                    return;
                }
                MessageBoxButton boxButton = MessageBoxButton.YesNo;

                var result = MessageBox.Show("将要保存至以下文件夹，请确认:\n" + saveFolderPath + '\\' + txtName.Text, "!!!", boxButton);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
                ListBoxItem item = new ListBoxItem();
                item.Content = txtName.Text;
                item.Selected += OnSelectItem;
                lstNames.Items.Add(item);
                archiveFolderNames.Add(txtName.Text);
                CopyDirectory(archivePath, saveFolderPath + '\\' + txtName.Text, true);

            }



            SaveArchiveConfig();
        }
        private void OnDelButton(object sender, RoutedEventArgs e)
        {
            if ((!archivePath.Contains("EldenRing\\")))
            {
                SetArchivePath("请选择正确的法环文件夹中的主存档文件夹");
            }
            if (selectArchiveName == "") return;
            MessageBoxButton boxButton = MessageBoxButton.YesNo;

            var result = MessageBox.Show("将要删除以下文件夹，请确认:\n" + saveFolderPath + '\\' + selectArchiveName, "!!!", boxButton);
            if (result == MessageBoxResult.No)
            {
                selectArchiveName = "";
                return;
            }

            lstNames.Items.Remove(delItem);
            archiveFolderNames.Remove(selectArchiveName);
            SaveArchiveConfig();
            FileSystem.DeleteDirectory(saveFolderPath + '\\' + selectArchiveName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            //Directory.Delete(saveFolderPath + '\\' + selectArchiveName, true);

        }
        private void OnChangeButton(object sender, RoutedEventArgs e)
        {
            if ((!archivePath.Contains("EldenRing\\")))
            {
                SetArchivePath("请选择正确的法环文件夹中的主存档文件夹");
            }
            if (selectArchiveName == "") return;
            MessageBoxButton boxButton = MessageBoxButton.YesNo;

            var result = MessageBox.Show("将要从存档:\n" + saveFolderPath + '\\' + selectArchiveName + "\n转换到\t" + archivePath, "!!!", boxButton);
            if (result == MessageBoxResult.No)
            {
                selectArchiveName = "";
                return;
            }

            //Directory.Delete(archivePath, true);
            FileSystem.DeleteDirectory(archivePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            CopyDirectory(saveFolderPath + '\\' + selectArchiveName, archivePath, true);
            //Directory.Delete(saveFolderPath + '\\' + selectArchiveName, true);
            //FileSystem.DeleteDirectory(saveFolderPath + '\\' + selectArchiveName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            //lstNames.Items.Remove(delItem);
            //archiveFolderNames.Remove(selectArchiveName);
            SaveArchiveConfig();
        }


        private void SetArchivePath(string titile = "")
        {

            var fileContent = string.Empty;

            OpenFolderDialog openFolderDialog = new();
            if (_archivePath == string.Empty || !Directory.Exists(archivePath)) openFolderDialog.InitialDirectory = "c:\\Users\\";
            else openFolderDialog.InitialDirectory = archivePath;
            openFolderDialog.Title = titile;
            var result = openFolderDialog.ShowDialog();
            if (result == false) return;
            fileContent = openFolderDialog.FolderName;

            if (File.Exists(configPath))
            {
                FileStream fs = new FileStream(configPath, FileMode.Open);
                StreamWriter sr = new StreamWriter(fs);
                fs.Seek(0, SeekOrigin.Begin);
                fs.SetLength(0);
                sr.WriteLine(fileContent);
                sr.WriteLine(saveFolderPath);
                sr.WriteLine("ArchiveFolders:");
                sr.Close();
            }
            else
            {
                FileStream fs = new FileStream(configPath, FileMode.Create);
                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(fileContent);
                sr.WriteLine(saveFolderPath);
                sr.WriteLine("ArchiveFolders:");
                sr.Close();
            }

            archivePath = fileContent;
        }
        private void SetSaveFolderPath(string titile = "")
        {

            var fileContent = string.Empty;

            OpenFolderDialog openFolderDialog = new();
            if (saveFolderPath == "EMPTY" || !Directory.Exists(saveFolderPath)) openFolderDialog.InitialDirectory = "c:\\Users\\";
            else openFolderDialog.InitialDirectory = saveFolderPath;
            openFolderDialog.Title = titile;
            var result = openFolderDialog.ShowDialog();
            if (result == false) return;
            fileContent = openFolderDialog.FolderName;

            if (File.Exists(configPath))
            {
                FileStream fs = new FileStream(configPath, FileMode.Open);
                StreamWriter sr = new StreamWriter(fs);
                fs.Seek(0, SeekOrigin.Begin);
                fs.SetLength(0);
                sr.WriteLine(archivePath);
                sr.WriteLine(fileContent);
                sr.WriteLine("ArchiveFolders:");
                sr.Close();
            }
            else
            {
                FileStream fs = new FileStream(configPath, FileMode.Create);
                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(archivePath);
                sr.WriteLine(fileContent);
                sr.WriteLine("ArchiveFolders:");
                sr.Close();
            }
            saveFolderPath = fileContent;
        }
        private void SaveArchiveConfig()
        {
            FileStream fs;
            StreamWriter sr;

            if (File.Exists(configPath))
            {
                fs = new FileStream(configPath, FileMode.Open);
                sr = new StreamWriter(fs);
                fs.Seek(0, SeekOrigin.Begin);
                fs.SetLength(0);
                sr.WriteLine(archivePath);
                sr.WriteLine(saveFolderPath);
            }
            else
            {
                fs = new FileStream(configPath, FileMode.Create);
                sr = new StreamWriter(fs);
                sr.WriteLine(archivePath);
                sr.WriteLine(saveFolderPath);
            }
            sr.WriteLine("ArchiveFolders:");
            foreach (var archiveFolderName in archiveFolderNames)
            {
                sr.WriteLine(archiveFolderName);
            }

            sr.Close();
        }



        private void GetFolderAndArchivePath(string ori, out string pFolder, out string pArchive)
        {
            pFolder = ""; pArchive = "";
            if (ori == null || ori.Length == 0) return;
            foreach (var c in ori)
            {
                pArchive += c;
                if (c == '\\')
                {
                    pFolder += pArchive;
                    pArchive = "";
                }
            }
        }
        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

    }
}