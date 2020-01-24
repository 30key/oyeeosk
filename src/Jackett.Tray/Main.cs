﻿using Jackett.Common.Models.Config;
using Jackett.Common.Services;
using Jackett.Common.Services.Interfaces;
using Jackett.Common.Utils;
using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jackett.Tray
{
    public partial class Main : Form
    {
        private IProcessService processService;
        private IServiceConfigService windowsService;
        private ITrayLockService trayLockService;
        private ISerializeService serializeService;
        private IConfigurationService configurationService;
        private ServerConfig serverConfig;
        private Process consoleProcess;
        private Logger logger;
        private bool closeApplicationInitiated;

        public Main(string updatedVersion)
        {
            Hide();
            InitializeComponent();

            Opacity = 0;
            Enabled = false;
            WindowState = FormWindowState.Minimized;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;

            RuntimeSettings runtimeSettings = new RuntimeSettings()
            {
                CustomLogFileName = "TrayLog.txt"
            };

            LogManager.Configuration = LoggingSetup.GetLoggingConfiguration(runtimeSettings);
            logger = LogManager.GetCurrentClassLogger();

            logger.Info("Starting Jackett Tray v" + EnvironmentUtil.JackettVersion);

            processService = new ProcessService(logger);
            windowsService = new WindowsServiceConfigService(processService, logger);
            trayLockService = new TrayLockService();
            serializeService = new SerializeService();
            configurationService = new ConfigurationService(serializeService, processService, logger, runtimeSettings);
            serverConfig = configurationService.BuildServerConfig(runtimeSettings);

            toolStripMenuItemAutoStart.Checked = AutoStart;
            toolStripMenuItemAutoStart.CheckedChanged += toolStripMenuItemAutoStart_CheckedChanged;

            toolStripMenuItemWebUI.Click += toolStripMenuItemWebUI_Click;
            toolStripMenuItemShutdown.Click += toolStripMenuItemShutdown_Click;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                toolStripMenuItemAutoStart.Visible = true;
            }

            if (!windowsService.ServiceExists())
            {
                // We are not installed as a service so just start the web server via JackettConsole and run from the tray.
                logger.Info("Starting server from tray");
                StartConsoleApplication();
            }

            updatedVersion = updatedVersion.Equals("yes", StringComparison.OrdinalIgnoreCase) ? EnvironmentUtil.JackettVersion : updatedVersion;

            if (!string.IsNullOrWhiteSpace(updatedVersion))
            {
                notifyIcon1.BalloonTipTitle = "Jackett";
                notifyIcon1.BalloonTipText = $"Jackett has updated to version {updatedVersion}";
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon1.ShowBalloonTip(10000);
                logger.Info($"Display balloon tip, updated to {updatedVersion}");
            }

            Task.Factory.StartNew(WaitForEvent);
        }

        private void WaitForEvent()
        {
            trayLockService.WaitForSignal();
            logger.Info("Received signal from tray lock service");

            if (windowsService.ServiceExists() && windowsService.ServiceRunning())
            {
                //We won't be able to start the tray app up again from the updater, as when running via a windows service there is no interaction with the desktop
                //Fire off a console process that will start the tray 20 seconds later

                string trayExePath = Assembly.GetEntryAssembly().Location;

                var startInfo = new ProcessStartInfo()
                {
                    Arguments = $"/c timeout 20 > NUL & \"{trayExePath}\" --UpdatedVersion yes",
                    FileName = "cmd.exe",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                logger.Info("Starting 20 second delay tray launch as Jackett is running as a Windows service: " + startInfo.FileName + " " + startInfo.Arguments);
                Process.Start(startInfo);
            }

            CloseTrayApplication();
        }

        private void toolStripMenuItemWebUI_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "http://127.0.0.1:" + serverConfig.Port,
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        private void toolStripMenuItemShutdown_Click(object sender, EventArgs e)
        {
            CloseTrayApplication();
        }

        private void toolStripMenuItemAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            AutoStart = toolStripMenuItemAutoStart.Checked;
        }

        private string ProgramTitle
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Name;
            }
        }

        private bool AutoStart
        {
            get
            {
                return File.Exists(ShortcutPath) || File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Jackett.lnk"));
            }
            set
            {
                if (value && !AutoStart)
                {
                    CreateShortcut();
                }
                else if (!value && AutoStart)
                {
                    File.Delete(ShortcutPath);
                }
            }
        }

        public string ShortcutPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Jackett.url");
            }
        }

        private void CreateShortcut()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                using (StreamWriter writer = new StreamWriter(ShortcutPath))
                {
                    var appPath = Assembly.GetExecutingAssembly().Location;
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine("URL=file:///" + appPath);
                    writer.WriteLine("IconIndex=0");
                    string icon = appPath.Replace('\\', '/');
                    writer.WriteLine("IconFile=" + icon);
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (windowsService.ServiceExists())
            {
                backgroundMenuItem.Visible = true;
                serviceControlMenuItem.Visible = true;
                toolStripSeparator1.Visible = true;
                toolStripSeparator2.Visible = true;

                if (windowsService.ServiceRunning())
                {
                    serviceControlMenuItem.Text = "Stop background service";
                    backgroundMenuItem.Text = "Jackett is running as a background service";
                    toolStripMenuItemWebUI.Enabled = true;
                }
                else
                {
                    serviceControlMenuItem.Text = "Start background service";
                    backgroundMenuItem.Text = "Jackett will run as a background service";
                    toolStripMenuItemWebUI.Enabled = false;
                }

                toolStripMenuItemShutdown.Text = "Close tray icon";
            }
            else
            {
                backgroundMenuItem.Visible = false;
                serviceControlMenuItem.Visible = false;
                toolStripSeparator1.Visible = false;
                toolStripSeparator2.Visible = false;
                toolStripMenuItemShutdown.Text = "Shutdown";
            }
        }

        private void serviceControlMenuItem_Click(object sender, EventArgs e)
        {
            var consolePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "JackettConsole.exe");

            if (windowsService.ServiceRunning())
            {
                if (ServerUtil.IsUserAdministrator())
                {
                    windowsService.Stop();
                }
                else
                {
                    try
                    {
                        processService.StartProcessAndLog(consolePath, "--Stop", true);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to get admin rights to stop the service.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                if (ServerUtil.IsUserAdministrator())
                {
                    windowsService.Start();
                }
                else
                {
                    try
                    {
                        processService.StartProcessAndLog(consolePath, "--Start", true);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to get admin rights to start the service.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CloseTrayApplication()
        {
            closeApplicationInitiated = true;

            logger.Info("Close of tray application initiated");

            //Clears notify icon, otherwise icon will still appear on taskbar until you hover the mouse over
            notifyIcon1.Icon = null;
            notifyIcon1.Dispose();
            Application.DoEvents();

            if (consoleProcess != null && !consoleProcess.HasExited)
            {
                consoleProcess.StandardInput.Close();
                System.Threading.Thread.Sleep(1000);
                if (consoleProcess != null && !consoleProcess.HasExited)
                {
                    consoleProcess.Kill();
                }
            }

            Application.Exit();
        }

        private void StartConsoleApplication()
        {
            string applicationFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            var exePath = Path.Combine(applicationFolder, "JackettConsole.exe");

            var startInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = exePath,
                RedirectStandardInput = true,
                RedirectStandardError = true
            };

            consoleProcess = Process.Start(startInfo);
            consoleProcess.EnableRaisingEvents = true;
            consoleProcess.Exited += ProcessExited;
            consoleProcess.ErrorDataReceived += ProcessErrorDataReceived;
        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.Error(e.Data);
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            if (!closeApplicationInitiated)
            {
                logger.Info("Tray icon not responsible for process exit");
                CloseTrayApplication();
            }
        }
    }
}
