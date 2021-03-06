﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemInfo.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The System Information.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.ApplicationServices.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management;
    using System.Text.RegularExpressions;
    using System.Windows.Documents;
    using System.Windows.Forms;

    using HandBrake.Interop.HbLib;

    using Microsoft.Win32;

    /// <summary>
    /// The System Information.
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// Gets the total physical ram in a system
        /// </summary>
        /// <returns>The total memory in the system</returns>
        public static ulong TotalPhysicalMemory
        {
            get
            {
                Win32.MEMORYSTATUSEX memStat = new Win32.MEMORYSTATUSEX { dwLength = 64 };
                Win32.GlobalMemoryStatusEx(ref memStat);

                ulong value = memStat.ullTotalPhys / 1024 / 1024;
                return value;
            }
        }

        /// <summary>
        /// Gets the number of CPU Cores
        /// </summary>
        /// <returns>Object</returns>
        public static object GetCpuCount
        {
            get
            {
                RegistryKey regKey = Registry.LocalMachine;
                regKey = regKey.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0");
                return regKey == null ? 0 : regKey.GetValue("ProcessorNameString");
            }
        }

        /// <summary>
        /// Gets the System screen size information.
        /// </summary>
        /// <returns>System.Windows.Forms.Scree</returns>
        public static Screen ScreenBounds
        {
            get { return Screen.PrimaryScreen; }
        }

        /// <summary>
        /// Gets a value indicating whether is qsv available.
        /// </summary>
        public static bool IsQsvAvailable
        {
            get
            {
                try
                {
                    if (!GeneralUtilities.IsLibHbPresent)
                    {
                        return false; // Feature is disabled.
                    }

                    return HBFunctions.hb_qsv_available() == 1;
                }
                catch (Exception)
                {
                    // Silent failure. Typically this means the dll hasn't been built with --enable-qsv
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether is hsw or newer.
        /// </summary>
        public static bool IsHswOrNewer
        {
            get
            {
                // TODO replace with a call to libhb
                string cpu = GetCpuCount.ToString();
                if (cpu.Contains("Intel"))
                {
                    Match match = Regex.Match(cpu, "([0-9]{4})");
                    if (match.Success)
                    {
                        string cpuId = match.Groups[0].ToString();
                        int cpuNumber;
                        if (int.TryParse(cpuId, out cpuNumber))
                        {
                            if (cpuNumber > 4000)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the get gpu driver version.
        /// </summary>
        public static List<string> GetGPUInfo
        {
            get
            {
                List<string> gpuInfo = new List<string>();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "select * from " + "Win32_VideoController");
                foreach (ManagementObject share in searcher.Get())
                {
                    string gpu = string.Empty, version = string.Empty;

                    foreach (PropertyData PC in share.Properties)
                    {
                        if (PC.Name.Equals("DriverVersion")) version = PC.Value.ToString();


                        if (PC.Name.Equals("Name"))
                            gpu = PC.Value.ToString();
                    }

                    gpuInfo.Add(string.Format("{0} - {1}", gpu, version));
                }
                return gpuInfo;
            }
        }
    }
}