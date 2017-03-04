namespace TouchpadPeaceFree
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    static class Program
    {
        internal static ProgramSettingsData ProgramSettings = new ProgramSettingsData();
        internal static bool FirstRun;
        private static string AppDataFileName = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                string assemblyLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
                FirstRun = false;

                int runningInstances = System.Diagnostics.Process.GetProcessesByName(
                    System.IO.Path.GetFileNameWithoutExtension(assemblyLocation)).Length;

                if (runningInstances > 1)
                {
                    MessageBox.Show(Strings.TouchpadPeaceInstanceFound);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                LoadProgramSettings();
            }
            catch (Exception) 
            { 
            }

            try
            {
                if (FirstRun)
                {
                    ContextMenus.OnHelp_Click(null, null);
                }

                using (TPCIcon tpcIcon = new TPCIcon())
                {
                    tpcIcon.Display();
                    AppDomain.CurrentDomain.ProcessExit += OnProcessExitRequest;
                    Application.Run();
                }
            }
            catch (Exception) 
            { 
            }
        }

        internal static void OnProcessExitRequest(object sender, EventArgs e)
        {
            Program.SaveProgramSettings();
        }

        internal static void SaveProgramSettings()
        {
            LoadOrSaveProgramSettings(false/*load*/);
        }

        internal static void LoadProgramSettings()
        {
            LoadOrSaveProgramSettings(true/*load*/);
        }

        private static void LoadOrSaveProgramSettings(bool load)
        {
            if (AppDataFileName == null)
            {
                string folderName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                folderName = Path.Combine(folderName, "TouchpadPeace");
                if (!Directory.Exists(folderName))
                {
                    try { Directory.CreateDirectory(folderName); }
                    catch { }

                    FirstRun = true;
                }

                AppDataFileName = Path.Combine(folderName, "TouchpadPeaceData.txt");
            }

            if (load)
            {
                String fileContents = "0";
                bool fileExists = File.Exists(AppDataFileName);
                if (fileExists)
                {
                    try
                    {
                        fileContents = File.ReadAllText(AppDataFileName);
                    }
                    catch (Exception)
                    {
                        fileExists = false;
                    }
                }
                else
                    FirstRun = true;

                ProgramSettings = ProgramSettingsData.fromString(fileContents);

                if (!fileExists)
                    LoadOrSaveProgramSettings(false/*load*/);
            }
            else
            {
                try
                {
                    File.WriteAllText(AppDataFileName, ProgramSettings.ToString());
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
