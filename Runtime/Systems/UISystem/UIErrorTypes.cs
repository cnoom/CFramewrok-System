using System;
using System.Threading;
using System.Threading.Tasks;

namespace CFramework.Systems.UISystem
{
    /// <summary>
    ///     UI错误严重程度。
    /// </summary>
    public enum UIErrorSeverity
    {
        /// <summary>可恢复错误（如动画失败，可跳过动画继续执行）</summary>
        Recoverable,

        /// <summary>可重试错误（如加载超时，可自动重试）</summary>
        Retryable,

        /// <summary>致命错误（如预制丢失，必须中止操作）</summary>
        Fatal
    }

    /// <summary>
    ///     错误处理结果。
    /// </summary>
    public enum ErrorHandlingResult
    {
        /// <summary>继续执行（忽略错误）</summary>
        Continue,

        /// <summary>重试操作</summary>
        Retry,

        /// <summary>中止操作</summary>
        Abort
    }

    /// <summary>
    ///     UI错误上下文信息。
    /// </summary>
    public readonly struct UIErrorContext
    {
        /// <summary>操作名称（如"OpenView"）</summary>
        public readonly string Operation;

        /// <summary>视图键（Addressables key）</summary>
        public readonly string ViewKey;

        /// <summary>原始异常</summary>
        public readonly Exception Exception;

        /// <summary>错误严重程度</summary>
        public readonly UIErrorSeverity Severity;

        /// <summary>额外上下文数据</summary>
        public readonly object Context;

        public UIErrorContext(string operation, string viewKey, Exception exception,
            UIErrorSeverity severity, object context = null)
        {
            Operation = operation;
            ViewKey = viewKey;
            Exception = exception;
            Severity = severity;
            Context = context;
        }
    }

    /// <summary>
    ///     UI错误处理器接口。
    /// </summary>
    /// <remarks>
    ///     实现此接口可自定义UI系统的错误处理逻辑。
    ///     通过CF.RegisterHandler注册的模块中实现此接口会自动被UISystemModule使用。
    /// </remarks>
    public interface IUIErrorHandler
    {
        /// <summary>
        ///     处理UI错误。
        /// </summary>
        /// <param name="error">错误上下文</param>
        /// <param name="token">取消令牌</param>
        /// <returns>错误处理结果</returns>
        Task<ErrorHandlingResult> HandleError(UIErrorContext error, CancellationToken token);
    }
}