using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32.TaskScheduler;

namespace Steam_Overlay_on_EGS_games
{
    internal static class Program
    {
        static bool taskRegistered = false;
        static string gameURL = @"%URL%";

        [STAThread]
        static void Main()
        {
            if (!CheckIfRunning("EpicGamesLauncher")) {
                RegisterTaskandRunIt(gameURL);
                while(!CheckIfRunning("EpicGamesLauncher"))
                    Thread.Sleep(500);
            }
            else
                Process.Start(gameURL);


            Process EpicGame = GetRunningProcessWithEpicCommands();

            if (taskRegistered)
                using (TaskService ts = new TaskService())
                    ts.RootFolder.DeleteTask("EGS silent startup");

            if (Debugger.IsAttached)
                MessageBox.Show(EpicGame.StartInfo.FileName);

            EpicGame.Start();
            Thread.Sleep(5000);
            EpicGame.WaitForExit();

            Environment.Exit(0);
        }

        static bool CheckIfRunning(string ProgramName)
        {
            Process[] processes = Process.GetProcessesByName(ProgramName);

            foreach (var process in processes)
                if (process.ProcessName == ProgramName)
                    return true;

            return false;
        }

        static Process GetRunningProcessWithEpicCommands()
        {
            while (true)
            {
                GameProcess[] processes = GetProcessesLike("-epicapp");
                foreach (var item in processes)
                {
                    try
                    {
                        item.process.Kill();
                        Trace.WriteLine(item.process.ProcessName + " KILLED");
                        return item.process;
                    }
                    catch { }
                    item.process.Dispose();
                }
                processes = null;
            }
        }

        static void RegisterTaskandRunIt(string URL)
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "EGStealer start epic silent";

                // Create an action that will launch Notepad whenever the trigger fires
                //td.Actions.Add(new ExecAction("cmd.exe", "/C start \"\" \"" + URL + "\""));
                td.Actions.Add(new ExecAction("explorer.exe", "\"" + URL + "\""));

                // Register the task in the root folder
                ts.RootFolder.RegisterTaskDefinition("EGS silent startup", td);

                taskRegistered = true;

                Microsoft.Win32.TaskScheduler.Task task = ts.FindTask("EGS silent startup");
                task.Run();
            }

        }

        public static string GetProcessPath(this Process process)
        {
            try
            {
                using (ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = " + process.Id))
                using (ManagementObjectCollection moc = mos.Get())
                    return (from mo in moc.Cast<ManagementObject>() select mo["ExecutablePath"]).First().ToString();
            }
            catch { }

            return null;
        }

        public static GameProcess[] GetProcessesLike(string search)
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE CommandLine LIKE '%" + search + "%'"))
                return searcher.Get()
                .Cast<ManagementObject>()
                .Select(mo =>
                    new GameProcess(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]))))
                .ToArray();
        }
    }

    internal class GameProcess
    {
        public Process process;

        public GameProcess(Process _process)
        {
            process = _process;

            string commandline = GetCommandLine(process);

            process.StartInfo.FileName = commandline.Substring(1, commandline.IndexOf("\" ") - 1);
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(process.StartInfo.FileName);
            process.StartInfo.Arguments = commandline.Replace("\"" + process.StartInfo.FileName + "\" ", "");
        }

        private static string GetCommandLine(Process process)
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                ManagementBaseObject managObject = objects.Cast<ManagementBaseObject>().SingleOrDefault();

                if (managObject != null)
                    if (managObject["CommandLine"] != null)
                        return managObject["CommandLine"].ToString();

                return null;
            }
        }
    }
}
