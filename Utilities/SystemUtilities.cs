using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace WindowLocker.Utilities
{
    public static class SystemUtilities
    {

        /// <summary>
        /// Temporarily blocks all user input
        /// </summary>
        /// <param name="block">True to block, false to unblock</param>
        /// <returns>Success status</returns>
        public static bool BlockUserInput(bool block)
        {
            return Win32Api.BlockInput(block);
        }

        /// <summary>
        /// Locks the workstation
        /// </summary>
        /// <returns>Success status</returns>
        public static bool LockComputer()
        {
            return Win32Api.LockWorkStation();
        }

        /// <summary>
        /// Executes a PowerShell command with elevated privileges
        /// </summary>
        /// <param name="command">The PowerShell command to execute</param>
        /// <returns>The output of the command</returns>
        public static string ExecutePowerShellCommand(string command)
        {
            string output = string.Empty;

            try
            {
                // Create process info
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Start the process
                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception($"PowerShell Error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing PowerShell command: {ex.Message}");
            }

            return output;
        }

        /// <summary>
        /// Checks if the current process is running with administrator privileges
        /// </summary>
        /// <returns>True if running as admin, otherwise false</returns>
        public static bool IsRunningAsAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// Restarts the current application with administrator privileges
        /// </summary>
        public static void RestartAsAdmin()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas" // This requests elevation
            };

            try
            {
                Process.Start(startInfo);
                Environment.Exit(0); // Exit the current process
            }
            catch (Exception)
            {
                // User cancelled the UAC prompt
            }
        }

        /// <summary>
        /// Creates a scheduled task to run the application at startup
        /// </summary>
        /// <param name="taskName">Name of the task</param>
        /// <returns>Success status</returns>
        public static bool CreateStartupTask(string taskName)
        {
            try
            {
                string appPath = Process.GetCurrentProcess().MainModule.FileName;
                string command = $"$action = New-ScheduledTaskAction -Execute '{appPath}'; " +
                                 $"$trigger = New-ScheduledTaskTrigger -AtLogon; " +
                                 $"$principal = New-ScheduledTaskPrincipal -UserId (Get-CimInstance -ClassName Win32_ComputerSystem | Select-Object -ExpandProperty UserName) -RunLevel Highest; " +
                                 $"Register-ScheduledTask -Action $action -Trigger $trigger -TaskName '{taskName}' -Principal $principal -Force";

                ExecutePowerShellCommand(command);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a scheduled task
        /// </summary>
        /// <param name="taskName">Name of the task to remove</param>
        /// <returns>Success status</returns>
        public static bool RemoveStartupTask(string taskName)
        {
            try
            {
                string command = $"Unregister-ScheduledTask -TaskName '{taskName}' -Confirm:$false";
                ExecutePowerShellCommand(command);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void ExtractResourceToFile(string resourceName, string outputPath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    throw new Exception($"리소스를 찾을 수 없습니다: {resourceName}");

                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }
    }
}