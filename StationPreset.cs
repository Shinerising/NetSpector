namespace NetSpector
{
    /// <summary>
    /// 用于记录预设网络配置信息的类
    /// </summary>
    public class StationPreset
    {
        /// <summary>
        /// 预设配置标识
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 预设配置名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 预设配置说明
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 计算机名称
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// 工作组名称
        /// </summary>
        public string Workgroup { get; set; }
        /// <summary>
        /// 网络适配器选择文本
        /// </summary>
        public string Adapter { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 子网掩码
        /// </summary>
        public string Mask { get; set; }
        /// <summary>
        /// 网关地址
        /// </summary>
        public string Gateway { get; set; }
        /// <summary>
        /// DNS服务器
        /// </summary>
        public string DNS { get; set; }
        /// <summary>
        /// 附加参数
        /// </summary>
        public string Param { get; set; }
    }
}
