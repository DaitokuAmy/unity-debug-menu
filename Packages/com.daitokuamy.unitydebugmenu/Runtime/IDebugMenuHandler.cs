namespace UnityDebugMenu {
    /// <summary>
    /// DebugMenuハンドリング用インターフェース
    /// </summary>
    public interface IDebugMenuHandler {
        /// <summary>
        /// 表示状態の切り替えチェック
        /// </summary>
        bool CheckVisible(DebugMenu menu, float deltaTime);
    }
}