using System.Xml.Serialization;
/// <summary>
/// 一時停止・再開機能を実装するインターフェイス
/// </summary>
interface IPause
{
    /// <summary>自身をPauseManagerに追加する/// </summary>
    void AddPauseScript();
    /// <summary>一時停止のための処理を実装する</summary>
    void Pause();
    /// <summary>再開のための処理を実装する</summary>
    void Resume();
}
