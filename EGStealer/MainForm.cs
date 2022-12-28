using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Toolbelt.Drawing;

namespace EGStealer
{
    public partial class MainForm : Form
    {
        string code = Utils.ReadFileFromIncrustedResources("EGS-game-stealing-process.cs");
        public MainForm()
        {
            InitializeComponent();
        }

        internal void InvokeUI(Action a)
        {
            this.Invoke(new MethodInvoker(a));
        }

        internal void InvokeUIAsync(Action a)
        {
            this.BeginInvoke(new MethodInvoker(a));
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Thread t = new Thread(() =>
            {
                if (!File.Exists($"{Utils.MyAppData}Microsoft.Win32.TaskScheduler.dll"))
                {
                    string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("Microsoft.Win32.TaskScheduler.dll"));
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        FileStream file = new FileStream($"{Utils.MyAppData}Microsoft.Win32.TaskScheduler.dll", FileMode.Create);
                        stream.CopyTo(file);
                    }
                }

                Utils.GetURLFiles($"{Utils.MyAppData}URLFiles", false);
                Utils.GetURLFiles(Utils.DesktopPath, true);
                Utils.Shortcuts = Utils.Shortcuts.OrderBy(u => u.Name).ToList();

                foreach (var shortcut in Utils.Shortcuts)
                {
                    InvokeUIAsync(() => checkedListBox1.Items.Add(shortcut.Name, shortcut.StealProcess));
                    Trace.WriteLine(shortcut.IconPath);
                }

                Thread t2 = new Thread(() =>
                {
                    foreach (var shortcut in Utils.Shortcuts)
                    {
                        if (Path.GetExtension(shortcut.IconPath) == ".ico")
                            continue;

                        if (Utils.NoIconGames.ContainsKey(Path.GetFileNameWithoutExtension(shortcut.IconPath)))
                            shortcut.IconPath = $"{Path.GetDirectoryName(shortcut.IconPath)}\\{Utils.NoIconGames[Path.GetFileNameWithoutExtension(shortcut.IconPath)]}{Path.GetExtension(shortcut.IconPath)}";

                        if (!File.Exists($"{Utils.MyTempPath}{shortcut.Name}.ico"))
                            IconExtractor.Extract1stIconTo(shortcut.IconPath, File.Create($"{Utils.MyTempPath}{shortcut.Name}.ico"));
                    }
                });
                t2.Start();
            });
            t.Start();
        }

        private async void CompileButton_Click(object sender, EventArgs e)
        {
            string originalText = CompileButton.Text;
            CompileButton.Enabled = false;

            await Utils.WaitForFile($"{Utils.MyAppData}Microsoft.Win32.TaskScheduler.dll");

            foreach (var (item, index) in Utils.Shortcuts.WithIndex())
            {
                if (item.StealProcess)
                {
                    if (Path.GetExtension(item.IconPath) != ".ico")
                    {
                        CompileButton.Text = $"EXTRACTING ICON OF {item.Name}";
                        await Utils.WaitForFile($"{Utils.MyTempPath}{item.Name}.ico");
                        Thread.Sleep(100);
                    }

                    CompileButton.Text = $"GENERATING STEALER PROGRAM OF {item.Name}";
                    Compiler.Compile(code, $"{Utils.MyAppData}{item.Name}.exe", item.Name, item.URL, Path.GetExtension(item.IconPath) == ".ico" ? item.IconPath : $"{Utils.MyTempPath}{item.Name}.ico");
                    if (checkBox1.Checked)
                    {
                        if (item.IsFromDesktop)
                        {
                            if (File.Exists($"{Utils.MyAppData}URLFiles\\{item.Name}.url"))
                                File.Delete($"{Utils.MyAppData}URLFiles\\{item.Name}.url");

                            File.Move(item.Path, $"{Utils.MyAppData}URLFiles\\{item.Name}.url");
                            item.IsFromDesktop = false;
                        }

                        if (File.Exists($"{Utils.DesktopPath}\\{item.Name} Stealed.lnk"))
                            File.Delete($"{Utils.DesktopPath}\\{item.Name} Stealed.lnk");

                        CreateShortcut(item.Name, $"{Utils.MyAppData}{item.Name}.exe", Utils.MyAppData);
                    }
                    else
                    {
                        CreateShortcut($"{item.Name} Stealed", $"{Utils.MyAppData}{item.Name}.exe", Utils.MyAppData);
                    }
                    checkedListBox1.SetItemChecked(index, false);
                }
            }

            CompileButton.Text = originalText;
            CompileButton.Enabled = true;
            MessageBox.Show("Programs for stealing process created successfully" + (checkBox1.Checked ? "\n & EGS shortcuts replaced" : ""));
        }

        internal static void CreateShortcut(string Name, string Output, string workingDir)
        {
            object shDesktop = (object)"Desktop";
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\" + Name + ".lnk";
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "EGS stealing process for " + Name;
            shortcut.TargetPath = Output;
            shortcut.WorkingDirectory = workingDir;
            shortcut.Save();
        }

        private void checkedListBox1_ItemCheck_1(object sender, ItemCheckEventArgs e)
        {
            var shortcut = Utils.Shortcuts[e.Index];
            shortcut.StealProcess = e.NewValue == CheckState.Checked ? true : false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            setCheckState(false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            setCheckState(true);
        }

        private void setCheckState(bool state)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                checkedListBox1.SetItemCheckState(i, (state ? CheckState.Checked : CheckState.Unchecked));
        }
    }
}
