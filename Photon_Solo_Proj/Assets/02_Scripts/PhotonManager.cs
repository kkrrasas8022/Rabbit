using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;

    [Header("Photon Basic field")]
    [SerializeField] private const  string _version = "1.0v";
    [SerializeField] private        string _nickName = "";
   

    [Header("UI")]
    [SerializeField] private Button _makeRoomBtn;
    [SerializeField] private Button _randomRoomBtn;
    [SerializeField] private TMP_InputField _nickNameIF;
    [SerializeField] private TMP_InputField _roomNameIF;
    [SerializeField] private GameObject _roomPrefab;
    [SerializeField] private Transform _roomContent;

    [SerializeField] private Slider[] colorSlider;

    [Header("Rabbit")]
    [SerializeField] private Material _rabbitMat;
 


    private void Awake()
    {
        Instance= this;
        //포톤서버에 들어가기위한 게임 버전과 닉네임
        PhotonNetwork.GameVersion = _version;
        PhotonNetwork.NickName = _nickName;
        //방장이 씬을 바꾸면 다같이 바뀌는 옵션
        PhotonNetwork.AutomaticallySyncScene = true;

        //네트워크에 연결되있지 않으면
        if (!PhotonNetwork.IsConnected)
        { //네트워크에 연결요청
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void Start()
    {
        _nickNameIF.text = PlayerPrefs.GetString("NICK_NAME", $"User_{Random.Range(0, 1001):0000}");
        _makeRoomBtn.onClick.AddListener(()=>MakeRoomBtnClick());
        _randomRoomBtn.onClick.AddListener(()=>PhotonNetwork.JoinRandomRoom());


        colorSlider[0].onValueChanged.AddListener((value) =>
        {
            _rabbitMat.color = new Color(value, _rabbitMat.color.g, _rabbitMat.color.b);
        }); 
        colorSlider[1].onValueChanged.AddListener((value) =>
        {
            _rabbitMat.color = new Color(_rabbitMat.color.r, value, _rabbitMat.color.b);
        }); 
        colorSlider[2].onValueChanged.AddListener((value) =>
        {
            _rabbitMat.color = new Color(_rabbitMat.color.r, _rabbitMat.color.g, value);
        });
    }

    private void MakeRoomBtnClick()
    {
        RoomOptions ro = new RoomOptions
        {
            MaxPlayers = 20,
            IsOpen = true,
            IsVisible = true,
        };
        SetNickName();
        PhotonNetwork.CreateRoom((
            string.IsNullOrEmpty(_roomNameIF.text) ? _nickNameIF.text + "'s": _roomNameIF.text)
            + "Room", ro);
    }

    public void SetNickName()
    {
        if (string.IsNullOrEmpty(_nickNameIF.text))
        {
            _nickName = $"User_{Random.Range(0, 1001):0000}";
            _nickNameIF.text = _nickName;
        }
        else
        {
            _nickName = _nickNameIF.text;
        }

        PhotonNetwork.NickName = _nickName;
        PlayerPrefs.SetString("NICK_NAME", _nickName);
    }

    #region PhotonCallBack

    //네트워크에 접속할떄 호출하는 함수(일반적으로 ToMaster를 사용한다)
    public override void OnConnectedToMaster()
    {
        print("서버연결");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        print("로비에 입장");
        //PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print($"방입장 실패 : {returnCode} : {message}");
        RoomOptions ro = new RoomOptions
        {
            MaxPlayers = 20,
            IsOpen = true,
            IsVisible = true,
        };
        SetNickName();
        PhotonNetwork.CreateRoom($"{_nickNameIF.text}'s Room", ro);
    }

    public override void OnCreatedRoom()
    {
        print("방생성 완료");
    }

    public override void OnJoinedRoom()
    {
        print("방 입장 완료");
        print(PhotonNetwork.CurrentRoom.Name);
        SceneManager.LoadScene("03_GameScene");
    }

    private Dictionary<string,GameObject> _roomDic= new Dictionary<string,GameObject>();
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var room in roomList)
        {
            //삭제된 룸이 있을 때 목록에서 지운다
            if(room.RemovedFromList==true)
            {
                if(_roomDic.TryGetValue(room.Name,out GameObject roomdel))
                {
                    Destroy(roomdel);
                    _roomDic.Remove(room.Name);
                }
                continue;
            }
            //룸리스트에는 있는데 dic에 없을 때 (신규룸)
            if(_roomDic.ContainsKey(room.Name)==false)
            {
                var _room = Instantiate(_roomPrefab, _roomContent);
                _room.GetComponent<RoomData>().RoomInfo = room;
                _roomDic.Add(room.Name, _room);
            }
            //이미 있는데 숫자가 변했을 경우
            if(_roomDic.ContainsKey(room.Name))
            {
                if (_roomDic[room.Name].GetComponent<RoomData>().RoomInfo.PlayerCount == room.PlayerCount)
                    continue;
                _roomDic[room.Name].GetComponent<RoomData>().RoomInfo = room;
            }
        }
    }


    #endregion
}
