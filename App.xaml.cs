using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace NetSpector
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 应用程序启动事件响应
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            //全局异常捕捉，出现未处理的异常时自动退出
            DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(App_UnhandledException);
        }

        /// <summary>
        /// 主线程异常捕捉
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">时间参数</param>
        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                MessageBox.Show(string.Format("程序出现未处理异常，异常信息：{0}{1}", Environment.NewLine, e.ExceptionObject), "程序异常", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {

            }
            Environment.Exit(0);
        }

        /// <summary>
        /// UI线程异常捕捉
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">时间参数</param>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                MessageBox.Show(string.Format("程序出现未处理异常，异常信息：{0}{1}", Environment.NewLine, e.Exception), "程序异常", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {

            }
            // 标记为已处理
            e.Handled = true;
            Environment.Exit(0);
        }
    }
}
