using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
// Photon 用の名前空間を参照する
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Serialization;

/// <summary>
/// Photon に接続するためのコンポーネント
/// </summary>

public class NetworkManager : MonoBehaviourPunCallbacks // Photon Realtime 用のクラスを継承する
{
    public int MaxPlayer { get; set; }  = 1;

    /// <summary>プレイヤーのプレハブの名前</summary>
    [SerializeField] string _playerPrefabName = "Prefab";
    /// <summary>プレイヤーを生成する場所を示すアンカーのオブジェクト</summary>
    Transform[] _spawnPositions = default;
    public static NetworkManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            PhotonNetwork.AutomaticallySyncScene = false;
        }
        else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Photonに接続する
    /// </summary>
    public async UniTask Connect(string gameVersion , int maxPlayer)
    {
        MaxPlayer = maxPlayer;
        PhotonNetwork.Disconnect();
        await UniTask.WaitUntil(() => PhotonNetwork.IsConnected == false);
        if (maxPlayer == 1)
        {
            PhotonNetwork.OfflineMode = true;
        }
        else
        {
            Debug.Log("MultiPlayStart!!");
            PhotonNetwork.OfflineMode = false;
            PhotonNetwork.GameVersion = gameVersion;    // 同じバージョンを指定したもの同士が接続できる
            PhotonNetwork.ConnectUsingSettings();
        }

    }

    /// <summary>
    /// ニックネームを付ける
    /// </summary>
    private void SetMyNickName(string nickName)
    {
        if (PhotonNetwork.IsConnected)
        {
            //Debug.Log("nickName: " + nickName);
            PhotonNetwork.LocalPlayer.NickName = nickName;
        }
    }
    

    /// <summary>
    /// 既に存在する部屋に参加する
    /// </summary>
    private void JoinExistingRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }
    /// <summary>ランダムな部屋への入室に失敗した時</summary>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRandomRoom();
    }
    /// <summary>
    /// ランダムな名前のルームを作って参加する
    /// </summary>
    private void CreateRandomRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("CreateRandomRoom");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;   // 誰でも参加できるようにする
            roomOptions.MaxPlayers = MaxPlayer;
            PhotonNetwork.CreateRoom(null, roomOptions); 
        }
    }
    

    /// <summary>
    /// プレイヤーを生成する
    /// </summary>
    [PunRPC]
    public void SpawnPlayer()
    {
        _spawnPositions = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponentsInChildren<Transform>();
        // プレイヤーをどこに spawn させるか決める
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;    // 自分の ActorNumber を取得する。なお ActorNumber は「1から」入室順に振られる。
        if (_playerPrefabName.Length > 0)
        {
            Transform spawnPoint = _spawnPositions[actorNumber];
            PhotonNetwork.Instantiate(_playerPrefabName +" " + actorNumber, spawnPoint.position, spawnPoint.rotation);
        }   
    }

    /// <summary>Photon に接続した時</summary>
    public override void OnConnected()
    {
        SetMyNickName(System.Environment.UserName + "@" + System.Environment.MachineName);
    }

    /// <summary>Photon との接続が切れた時</summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnDisconnected");
    }

    /// <summary>マスターサーバーに接続した時</summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connect TO Master");
        if (PhotonNetwork.OfflineMode)
        {
            Debug.Log("CreateSoloRoom");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 1;
            PhotonNetwork.CreateRoom("SoloRoom", roomOptions);
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinLobby();
            }
        }
        
    }

    /// <summary>ロビーに参加した時</summary>
    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinRoom");
        JoinExistingRoom();
    }

    /// <summary>ロビーから出た時</summary>
    public override void OnLeftLobby()
    {
        //Debug.Log("OnLeftLobby");
    }

    /// <summary>部屋を作成した時</summary>
    public override void OnCreatedRoom()
    {
        //true
        //Debug.Log("OnCreatedRoom");
        // if (PhotonNetwork.OfflineMode) return;
        // if ( PhotonNetwork.LocalPlayer.ActorNumber > PhotonNetwork.CurrentRoom.MaxPlayers - 1)
        // {
        //     photonView.RPC(nameof(MasterGameManager.Instance.CallStartTitles),RpcTarget.MasterClient);
        //     PhotonNetwork.CurrentRoom.IsOpen = false;
        // }
    }

    /// <summary>部屋の作成に失敗した時</summary>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        
        //Debug.Log("OnCreateRoomFailed: " + message);
    }

    /// <summary>部屋に入室した時</summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");

        
    }
#region 未使用
 /// <summary>指定した部屋への入室に失敗した時</summary>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        //Debug.Log("OnJoinRoomFailed: " + message);
    }



    /// <summary>部屋から退室した時</summary>
    public override void OnLeftRoom()
    {
        //Debug.Log("OnLeftRoom");
    }

    /// <summary>自分のいる部屋に他のプレイヤーが入室してきた時</summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("OnPlayerEnteredRoom: " + newPlayer.NickName);
    }

    /// <summary>自分のいる部屋から他のプレイヤーが退室した時</summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        photonView.RPC(nameof(MasterGameManager.Instance.JoinSoloGame), RpcTarget.All);
        _ = SceneUIManager.Instance.ShowPlayerLeftText(3000);
    }

    /// <summary>マスタークライアントが変わった時</summary>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //Debug.Log("OnMasterClientSwitched to: " + newMasterClient.NickName);
    }

    /// <summary>ロビー情報に更新があった時</summary>
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        //Debug.Log("OnLobbyStatisticsUpdate");
    }

    /// <summary>ルームリストに更新があった時</summary>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //Debug.Log("OnRoomListUpdate");
    }

    /// <summary>ルームプロパティが更新された時</summary>
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        //Debug.Log("OnRoomPropertiesUpdate");
    }

    /// <summary>プレイヤープロパティが更新された時</summary>
    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        //Debug.Log("OnPlayerPropertiesUpdate");
    }
#endregion
   
}
