using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace NetSpector
{
    public class ServiceItem : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性变化时的事件处理
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知UI更新数据的方法
        /// </summary>
        /// <typeparam name="T">泛型参数</typeparam>
        /// <param name="obj">以待更新项目为属性的匿名类实例</param>
        protected void Notify<T>(T obj)
        {
            if (obj == null)
            {
                return;
            }
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property.Name));
            }
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ServiceName { get; set; }
        public bool IsValid { get; set; } = false;
        public bool IsEnabled { get; set; } = false;
        public int DefaultValue { get; set; }
        public void Refresh()
        {
            Notify(new { IsValid, IsEnabled });
        }
    }
    public static class SystemServiceManager
    {
        public static List<ServiceItem> ServiceList { get; set; } = new List<ServiceItem>()
        {
            new ServiceItem()
            {
                Name = "Windows Update",
                Description = "Windows 系统自动更新",
                ServiceName = "wuauserv",
                DefaultValue = 3,
            },
            new ServiceItem()
            {
                Name = "Security Center",
                Description = "系统安全中心",
                ServiceName = "wscsvc",
                DefaultValue = 2,
            },
            new ServiceItem()
            {
                Name = "Security Center Service",
                Description = "Windows 安全中心服务",
                ServiceName = "SecurityHealthService",
                DefaultValue = 3,
            },
            new ServiceItem()
            {
                Name = "Remote Registry",
                Description = "远程注册表编辑",
                ServiceName = "RemoteRegistry",
                DefaultValue = 3,
            },
            new ServiceItem()
            {
                Name = "Windows Error Reporting",
                Description = "Windows 错误报告",
                ServiceName = "WerSvc",
                DefaultValue = 3,
            },
            new ServiceItem()
            {
                Name = "Windows Defender Firewall",
                Description = "Windows 防护工具防火墙",
                ServiceName = "mpssvc",
                DefaultValue = 2,
            },
            new ServiceItem()
            {
                Name = "Windows Search",
                Description = "Windows 系统搜索",
                ServiceName = "WSearch",
                DefaultValue = 0,
            },
            new ServiceItem()
            {
                Name = "Windows Insider Program",
                Description = "Windows 预览体验成员服务",
                ServiceName = "wisvc",
                DefaultValue = 3,
            },
        };
        private static bool IsLoading = false;
        public static void RefreshAllServiceStatus()
        {
            if (IsLoading)
            {
                return;
            }
            Task.Factory.StartNew(() =>
            {
                IsLoading = true;
                foreach (var service in ServiceList)
                {
                    try
                    {
                        service.IsEnabled = !GetServiceDisabledStatus(service);
                        service.IsValid = true;
                    }
                    catch
                    {
                        service.IsEnabled = false;
                        service.IsValid = false;
                    }
                    service.Refresh();
                }
                IsLoading = false;
            });
        }
        public static void RefreshServiceStatus(ServiceItem service)
        {
            try
            {
                service.IsEnabled = !GetServiceDisabledStatus(service);
                service.IsValid = true;
            }
            catch
            {
                service.IsEnabled = false;
                service.IsValid = false;
            }
            service.Refresh();
        }
        public static bool GetServiceDisabledStatus(ServiceItem service)
        {
            string sh = $"/C sc qc {service.ServiceName} | findstr START_TYPE";
            string result = RunCommand(sh);
            if (result.Contains("4"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void EnableService(ServiceItem service)
        {
            try
            {
                string method = "auto";
                switch (service.DefaultValue)
                {
                    case 0:
                        method = "auto";
                        break;
                    case 1:
                        method = "boot";
                        break;
                    case 2:
                        method = "system";
                        break;
                    case 3:
                        method = "demand";
                        break;
                    case 4:
                        method = "disabled";
                        break;
                    case 5:
                        method = "delayed-auto";
                        break;
                    default:
                        break;
                }
                string sh = $"/C sc config {service.ServiceName} start= {method}";
                RunCommand(sh);
                MessageBox.Show($"已成功启用服务{service.Name}！", "消息提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("启用服务{2}时出现错误，错误信息：{0}{1}", Environment.NewLine, ex.Message, service.Name), "消息警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        public static void DisableService(ServiceItem service)
        {
            try
            {
                string sh = $"/C sc config {service.ServiceName} start= disabled & sc stop {service.ServiceName}";
                RunCommand(sh);
                MessageBox.Show($"已成功禁用服务{service.Name}！", "消息提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("禁用服务{2}时出现错误，错误信息：{0}{1}", Environment.NewLine, ex.Message, service.Name), "消息警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        private static string RunCommand(string sh)
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
    }
}
