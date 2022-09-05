using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Configuration;
using System.Xml.Serialization;

namespace NetSpector
{
    /// <summary>
    /// 主窗体类
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 网络配置信息对象
        /// </summary>
        private readonly NetworkSetting setting;
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            setting = NetworkSetting.GetInstance();

            InitializeComponent();

            InitializeSettings();
        }
        /// <summary>
        /// 初始化配置信息
        /// </summary>
        public void InitializeSettings()
        {
            setting.MachineName = Environment.MachineName;

            setting.WorkGroupName = GetWorkgroup();

            RefreshNetworkInterfaces(true);

            LoadPresets();

            setting.Refresh();

            setting.ConfigPath = ConfigurationManager.AppSettings["ini_path"];
            RefreshConfig();
            RefreshFirewall();
            setting.RefreshConfig();
        }
        /// <summary>
        /// 从XML文件加载预设配置列表
        /// </summary>
        public void LoadPresets()
        {
            try
            {
                using (StreamReader sr = new StreamReader("preset.xml", Encoding.UTF8))
                {
                    var reader = new XmlSerializer(typeof(List<StationPreset>));
                    setting.PresetList = (List<StationPreset>)reader.Deserialize(sr);
                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// 加载某项网络配置信息
        /// </summary>
        /// <param name="preset">预设网络配置对象</param>
        public void LoadPreset(StationPreset preset)
        {
            if (preset.MachineName != null && preset.MachineName.Trim().Length > 0)
            {
                setting.MachineName = preset.MachineName;
            }

            if (preset.Workgroup != null && preset.Workgroup.Trim().Length > 0)
            {
                setting.WorkGroupName = preset.Workgroup;
            }

            if (preset.Adapter != null && preset.Adapter.Trim().Length > 0)
            {
                try
                {
                    Regex regex = new Regex(preset.Adapter);
                    NetworkInterface network = setting.NetworkInterfaceList.Where(item => regex.IsMatch(item.Description)).FirstOrDefault();
                    if (network != null)
                    {
                        setting.NetworkSelectionIndex = setting.NetworkInterfaceList.IndexOf(network);
                        RefreshNetworkProperties(network);
                    }
                }
                catch
                {

                }
            }

            setting.IP = preset.IP;
            setting.Mask = preset.Mask;
            setting.Gateway = preset.Gateway;
            setting.DNS = preset.DNS;
            setting.IsSettingUpdated = true;

            setting.Refresh();

            setting.RefreshNetwork();
        }
        /// <summary>
        /// 应用当前设置
        /// </summary>
        public void ApplySettings()
        {
            Cursor = Cursors.Wait;

            bool isRebootRequired = false;

            try
            {
                if (setting.MachineName != Environment.MachineName)
                {
                    SetMachineName(setting.MachineName);
                    isRebootRequired = true;
                }

                if (setting.WorkGroupName != GetWorkgroup())
                {
                    SetWorkgroupName(setting.WorkGroupName);
                    isRebootRequired = true;
                }

                ApplyNetworkProperties();

                RefreshNetworkInterfaces();

                SaveConfig();

                RefreshConfig();
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("应用当前设置时出现错误，错误信息：{0}{1}", Environment.NewLine, e.Message), "消息警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            Cursor = Cursors.Arrow;

            if (isRebootRequired)
            {
                MessageBoxResult result = MessageBox.Show("当前设置将在计算机重启后生效，是否现在重启？", "消息提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    Reboot();
                    Close();
                }
            }
            else
            {
                MessageBox.Show("已成功应用当前网络设置！", "消息提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        /// <summary>
        /// 获取本地计算机工作组名称
        /// </summary>
        /// <returns>工作组名称</returns>
        private string GetWorkgroup()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", @"SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["Domain"] as string;
            }
            return null;
        }
        /// <summary>
        /// 使用CMD执行某段命令
        /// </summary>
        /// <param name="sh">执行命令</param>
        /// <param name="path">工作目录</param>
        /// <returns>执行结束后所返回的文本信息</returns>
        private string RunCommand(string sh, string path = null)
        {
            Console.WriteLine(sh);
            ProcessStartInfo info = new ProcessStartInfo
            {
                Arguments = sh,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            if (path != null)
            {
                info.WorkingDirectory = path;
            }
            Process process = Process.Start(info);
            string result = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            bool isComplete = process.WaitForExit(30000);
            Console.WriteLine(result);
            if (error != null && error.Trim().Length > 0)
            {
                throw new Exception(error);
            }
            else if (!isComplete)
            {
                throw new Exception("网络设置指令已超时！");
            }
            return result;
        }
        /// <summary>
        /// 重启本地计算机
        /// </summary>
        public void Reboot()
        {
            string sh = "/C shutdown -r -t 5";
            RunCommand(sh);
        }
        /// <summary>
        /// 设置本地计算机名称
        /// </summary>
        /// <param name="name">计算机名称</param>
        public void SetMachineName(string name)
        {
            string sh = string.Format("/C WMIC computersystem where caption=\"%computername%\" rename \"{0}\" & exit", name);
            RunCommand(sh);
        }
        /// <summary>
        /// 设置本地工作组名称
        /// </summary>
        /// <param name="name">工作组名称</param>
        public void SetWorkgroupName(string name)
        {
            string sh = string.Format("/C WMIC computersystem where name=\"%computername%\" call joindomainorworkgroup name=\"{0}\" & exit", name);
            RunCommand(sh);
        }
        /// <summary>
        /// 应用网络适配器设置
        /// </summary>
        public void ApplyNetworkProperties()
        {
            if (!setting.IsSettingUpdated)
            {
                return;
            }
            NetworkInterface network = setting.NetworkInterfaceList[setting.NetworkSelectionIndex];
            string sh;
            if (setting.Gateway != null && setting.Gateway.Trim().Length > 0)
            {
                sh = string.Format("/C netsh interface ip set address \"{0}\" static {1} {2} {3} 1", network.Name, setting.IP, setting.Mask, setting.Gateway);
            }
            else
            {
                sh = string.Format("/C netsh interface ip set address \"{0}\" static {1} {2} none", network.Name, setting.IP, setting.Mask);
            }
            if (setting.DNS != null && setting.DNS.Trim().Length > 0)
            {
                if (Environment.OSVersion.Version.Major > 5)
                {
                    sh += string.Format(" & netsh interface ip set dns \"{0}\" static {1} primary validate=no & exit", network.Name, setting.DNS);
                }
                else
                {
                    sh += string.Format(" & netsh interface ip set dns \"{0}\" static {1} primary & exit", network.Name, setting.DNS);
                }
            }
            else
            {
                sh += string.Format(" & netsh interface ip set dns \"{0}\" static none & exit", network.Name);
            }

            RunCommand(sh);
        }
        /// <summary>
        /// 刷新网络接口列表
        /// </summary>
        /// <param name="isInitializing">是否程序正在初始化过程中</param>
        private void RefreshNetworkInterfaces(bool isInitializing = false)
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            List<NetworkInterface> interfaces = nics.Where(item => item.Supports(NetworkInterfaceComponent.IPv4) && item.NetworkInterfaceType != NetworkInterfaceType.Loopback).ToList();
            setting.NetworkInterfaceList = interfaces;
            setting.NetworkNameList = interfaces.Select(item => item.Name + " - " + item.Description).ToList();

            if (isInitializing)
            {
                try
                {
                    string pattern = ConfigurationManager.AppSettings["adapter_pattern"];
                    if (pattern != null && pattern.Length > 0)
                    {
                        Regex regex = new Regex(pattern);
                        NetworkInterface network = interfaces.Where(item => regex.IsMatch(item.Description)).FirstOrDefault();
                        if (network != null)
                        {
                            setting.NetworkSelectionIndex = interfaces.IndexOf(network);
                        }
                    }
                }
                catch
                {

                }

            }

            if (interfaces.Count > setting.NetworkSelectionIndex)
            {
                RefreshNetworkProperties(setting.NetworkInterfaceList[setting.NetworkSelectionIndex]);
            }
            else
            {
                RefreshNetworkProperties(null);
            }
        }
        /// <summary>
        /// 刷新单个网络接口信息
        /// </summary>
        /// <param name="network">网络接口对象</param>
        private void RefreshNetworkProperties(NetworkInterface network)
        {
            if (network == null)
            {
                setting.NetworkStats = null;
                setting.IP = null;
                setting.Mask = null;
                setting.Gateway = null;
                setting.DNS = null;
                setting.IsSettingUpdated = false;
            }
            else
            {
                IPInterfaceProperties properties = network.GetIPProperties();
                UnicastIPAddressInformation ip = properties.UnicastAddresses.Where(item => item.Address.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
                setting.IsNetworkOn = network.OperationalStatus == OperationalStatus.Up;
                setting.NetworkStats = string.Format("状态：{0}  带宽：{1:f1}Mbps  MAC地址：{2}", network.OperationalStatus == OperationalStatus.Up ? "在线" : "离线", Math.Max(network.Speed, 0) / 1000 / 1000, string.Join(":", network.GetPhysicalAddress().GetAddressBytes().Select(item => item.ToString("x2"))).ToUpper());
                setting.IP = ip?.Address.ToString();
                setting.Mask = ip?.IPv4Mask.ToString();
                setting.Gateway = properties.GatewayAddresses.Where(item => item.Address.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault()?.Address.ToString();
                setting.DNS = properties.DnsAddresses.Where(item => item.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault()?.ToString();
                setting.IsSettingUpdated = false;
            }
            setting.RefreshNetwork();
        }
        /// <summary>
        /// 刷新INI配置信息
        /// </summary>
        /// <returns>是否刷新成功</returns>
        private bool RefreshConfig()
        {
            NetworkConfig config = new NetworkConfig();

            try
            {
                INI.IniFilePath = setting.ConfigPath;
                config.Key = INI.ReadIni<string>("SETUP", "ME");
                config.Neighbour = INI.ReadIni<string>("SETUP", "Neighbor");
                config.IP = INI.ReadIni<string>("SETUP", "IP");
                config.MaintenanceIP = INI.ReadIni<string>("SETUP", "MaintanceStation");
                config.IsLoaded = true;
                using (StreamReader sr = new StreamReader(setting.ConfigPath, ConfigurationManager.AppSettings["encoding_ini"] == "UTF-8" ? Encoding.UTF8:Encoding.Default))
                {
                    config.Text = sr.ReadToEnd();
                }
            }
            catch
            {
                return false;
            }

            setting.IsConfigUpdated = false;
            setting.Config = config;
            setting.RefreshConfig();
            return true;
        }
        /// <summary>
        /// 刷新防火墙状态
        /// </summary>
        /// <returns>是否刷新成功</returns>
        private bool RefreshFirewall()
        {
            FirewallState firewall = new FirewallState();

            try
            {
                string sh = "/C netsh firewall show state";
                string result = RunCommand(sh);
                firewall.StateText = result;
                Regex regex = new Regex("操作模式(.*)= 启用");
                if (regex.IsMatch(result))
                {
                    firewall.IsEnabled = true;
                }
                else
                {
                    firewall.IsEnabled = false;
                }
            }
            catch
            {
                return false;
            }

            setting.Firewall = firewall;
            setting.RefreshFirewall();
            return true;
        }
        /// <summary>
        /// 保存INI配置信息
        /// </summary>
        private void SaveConfig()
        {
            if (setting.IsConfigUpdated)
            {
                INI.WriteIni("SETUP", "ME", setting.Config.Key);
                INI.WriteIni("SETUP", "Neighbor", setting.Config.Neighbour);
                INI.WriteIni("SETUP", "IP", setting.Config.IP);
                INI.WriteIni("SETUP", "MaintanceStation", setting.Config.MaintenanceIP);
            }
        }
        /// <summary>
        /// 应用INI配置预设信息
        /// </summary>
        /// <param name="key">主机标识</param>
        public void ApplyConfigPreset(string key)
        {
            if (setting.PresetList == null)
            {
                return;
            }

            NetworkConfig config = new NetworkConfig
            {
                Text = setting.Config.Text
            };

            switch (key)
            {
                case "A":
                    config.Key = "A";
                    config.Neighbour = setting.PresetList.Where(item => item.Key == "B").FirstOrDefault()?.MachineName;
                    config.IP = setting.PresetList.Where(item => item.Key == "A").FirstOrDefault()?.IP;
                    config.MaintenanceIP = setting.PresetList.Where(item => item.Key == "M").FirstOrDefault()?.IP;
                    break;
                case "B":
                    config.Key = "B";
                    config.Neighbour = setting.PresetList.Where(item => item.Key == "A").FirstOrDefault()?.MachineName;
                    config.IP = setting.PresetList.Where(item => item.Key == "B").FirstOrDefault()?.IP;
                    config.MaintenanceIP = setting.PresetList.Where(item => item.Key == "M").FirstOrDefault()?.IP;
                    break;
            }

            setting.IsConfigUpdated = true;
            setting.Config = config;
            setting.RefreshConfig();
        }
        /// <summary>
        /// 网络接口选中项目更改事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void Interface_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            setting.NetworkSelectionIndex = comboBox.SelectedIndex;
            RefreshNetworkProperties(setting.NetworkInterfaceList[setting.NetworkSelectionIndex]);
        }
        /// <summary>
        /// 刷新网络接口按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void InterfaceRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshNetworkInterfaces();
        }
        /// <summary>
        /// 应用设置按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }
        /// <summary>
        /// 取消设置按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// 重启系统按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void Reboot_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("即将重启计算机，未保存的任务可能会丢失，是否确定？", "消息提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                Reboot();
                Close();
            }
        }
        /// <summary>
        /// 加载预设网络配置信息按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            LoadPreset(element.DataContext as StationPreset);
        }
        /// <summary>
        /// 加载INI配置文件按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void LoadConfig_Click(object sender, RoutedEventArgs e)
        {
            bool result = RefreshConfig();
            if (!result)
            {
                MessageBox.Show("加载配置文件时出现错误！", "消息警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        /// <summary>
        /// 加载INI预设配置按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void LoadConfigPreset_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ApplyConfigPreset(element.Tag?.ToString());
        }
        /// <summary>
        /// 网络配置文本框修改事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void NetworkSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            setting.IsSettingUpdated = true;
        }
        /// <summary>
        /// INI配置文本框修改事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void NetworkConfig_TextChanged(object sender, TextChangedEventArgs e)
        {
            setting.IsConfigUpdated = true;
        }
        /// <summary>
        /// 启用防火墙按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void StartFirewall_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            try
            {
                string sh = "/C netsh firewall set opmode ENABLE";
                RunCommand(sh);
                MessageBox.Show("已成功开启防火墙！", "消息提示", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("开启防火墙时出现错误，错误信息：{0}{1}", Environment.NewLine, ex.Message), "消息警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            RefreshFirewall();

            Cursor = Cursors.Arrow;
        }
        /// <summary>
        /// 关闭防火墙按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void StopFirewall_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            try
            {
                string sh = "/C netsh firewall set opmode DISABLE";
                RunCommand(sh);
                MessageBox.Show("已成功关闭防火墙！", "消息提示", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("关闭防火墙时出现错误，错误信息：{0}{1}", Environment.NewLine, ex.Message), "消息警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            RefreshFirewall();

            Cursor = Cursors.Arrow;
        }
        /// <summary>
        /// 禁用端口按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void BlockPort_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            try
            {
                string sh;
                if (Environment.OSVersion.Version.Major > 5)
                {
                    sh = "/C \"" + AppDomain.CurrentDomain.BaseDirectory + "bat\\blockport_win7.bat\"";
                }
                else
                {
                    sh = "/C \"" + AppDomain.CurrentDomain.BaseDirectory + "bat\\blockport_xp.bat\"";
                }
                RunCommand(sh, AppDomain.CurrentDomain.BaseDirectory + "bat");
                MessageBox.Show("已成功禁用敏感端口！", "消息提示", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("禁用端口时出现错误，错误信息：{0}{1}", Environment.NewLine, ex.Message), "消息警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            RefreshFirewall();

            Cursor = Cursors.Arrow;
        }
        /// <summary>
        /// 关闭安全中心
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void StopSecurityCenter_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            try
            {
                string sh = "/C sc config wscsvc start= disabled & sc stop wscsvc";
                RunCommand(sh);
                MessageBox.Show("已成功关闭安全中心！", "消息提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("关闭安全中心时出现错误，错误信息：{0}{1}", Environment.NewLine, ex.Message), "消息警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            Cursor = Cursors.Arrow;
        }
    }
}
