using System.Xml.Serialization;
/// <summary>
/// �ꎞ��~�E�ĊJ�@�\����������C���^�[�t�F�C�X
/// </summary>
interface IPause
{
    /// <summary>���g��PauseManager�ɒǉ�����/// </summary>
    void AddPauseScript();
    /// <summary>�ꎞ��~�̂��߂̏�������������</summary>
    void Pause();
    /// <summary>�ĊJ�̂��߂̏�������������</summary>
    void Resume();
}
