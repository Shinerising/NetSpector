using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reflection;

namespace NetSpector
{
    /// <summary>
    /// 网络配置信息类
    /// </summary>
    public class NetworkSetting : INotifyPropertyChanged
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
        /// <summary>
        /// 私有构造函数
        /// </summary>
        private NetworkSetting()
        {

        }
        /// <summary>
        /// 唯一实例
        /// </summary>
        private static NetworkSetting instance;
        /// <summary>
        /// 获取网络配置信息的实例
        /// </summary>
        public static NetworkSetting Instance => GetInstance();
        /// <summary>
        /// 用于设计的预览信息实例
        /// </summary>
        public static NetworkSetting PreviewInstance => new NetworkSetting()
        {
            MachineName = "CRSCD-HOST",
            WorkGroupName = "WORKGROUP",
            IP = "192.168.1.101",
            Mask = "255.255.255.0",
            Gateway = "192.168.1.1",
            DNS = "192.168.1.1"
        };
        /// <summary>
        /// 获取网络配置信息的实例
        /// </summary>
        /// <returns>当前实例</returns>
        public static NetworkSetting GetInstance()
        {
            if (instance == null)
            {
                instance = new NetworkSetting();
            }
            return instance;
        }
        /// <summary>
        /// 计算机名称
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// 工作组名称
        /// </summary>
        public string WorkGroupName { get; set; }
        /// <summary>
        /// 网络接口列表
        /// </summary>
        public List<NetworkInterface> NetworkInterfaceList { get; set; }
        /// <summary>
        /// 网络接口名称列表
        /// </summary>
        public List<string> NetworkNameList { get; set; }
        /// <summary>
        /// 网络接口选择ID
        /// </summary>
        public int NetworkSelectionIndex { get; set; }
        /// <summary>
        /// 当前网络接口是否已连接
        /// </summary>
        public bool IsNetworkOn { get; set; }
        /// <summary>
        /// 当前网络接口状态文本
        /// </summary>
        public string NetworkStats { get; set; }
        /// <summary>
        /// 当前网络接口静态IP地址文本
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 当前网络接口子网掩码文本
        /// </summary>
        public string Mask { get; set; }
        /// <summary>
        /// 当前网络接口网关地址文本
        /// </summary>
        public string Gateway { get; set; }
        /// <summary>
        /// 当前网络接口DNS服务器地址
        /// </summary>
        public string DNS { get; set; }
        /// <summary>
        /// 网络配置信息是否已更新
        /// </summary>
        public bool IsSettingUpdated { get; set; }
        /// <summary>
        /// INI配置文件是否已更新
        /// </summary>
        public bool IsConfigUpdated { get; set; }
        /// <summary>
        /// 网络预设配置列表
        /// </summary>
        public List<StationPreset> PresetList { get; set; }
        /// <summary>
        /// INI配置文件路径
        /// </summary>
        public string ConfigPath { get; set; }
        /// <summary>
        /// 当前与INI文件对应的网络配置
        /// </summary>
        public NetworkConfig Config { get; set; }
        /// <summary>
        /// 当前防火墙信息
        /// </summary>
        public FirewallState Firewall { get; set; }

        /// <summary>
        /// 刷新基本信息
        /// </summary>
        public void Refresh()
        {
            Notify(new { MachineName, WorkGroupName, PresetList });
        }

        /// <summary>
        /// 刷新网络信息
        /// </summary>
        public void RefreshNetwork()
        {
            Notify(new { NetworkSelectionIndex, NetworkNameList, IsNetworkOn, NetworkStats, IP, Mask, Gateway, DNS });
        }

        /// <summary>
        /// 刷新INI配置信息
        /// </summary>
        public void RefreshConfig()
        {
            Notify(new { ConfigPath, Config });
        }

        /// <summary>
        /// 刷新防火墙信息
        /// </summary>
        public void RefreshFirewall()
        {
            Notify(new { Firewall });
        }
    }

    /// <summary>
    /// 与INI配置文件所对应的网络配置信息
    /// </summary>
    public struct NetworkConfig
    {
        /// <summary>
        /// 本机标识
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 邻机标识
        /// </summary>
        public string Neighbour { get; set; }
        /// <summary>
        /// 本机IP地址
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 维护机IP地址
        /// </summary>
        public string MaintenanceIP { get; set; }
        /// <summary>
        /// INI配置文本
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool IsLoaded { get; set; }
    }

    /// <summary>
    /// 防火墙状态
    /// </summary>
    public struct FirewallState
    {
        /// <summary>
        /// 防火墙是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
        /// <summary>
        /// 防火墙状态文本
        /// </summary>
        public string StateText { get; set; }
    }
}
