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
        //���漭���� �������� ���� ������ �г���
        PhotonNetwork.GameVersion = _version;
        PhotonNetwork.NickName = _nickName;
        //������ ���� �ٲٸ� �ٰ��� �ٲ�� �ɼ�
        PhotonNetwork.AutomaticallySyncScene = true;

        //��Ʈ��ũ�� ��������� ������
        if (!PhotonNetwork.IsConnected)
        { //��Ʈ��ũ�� �����û
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

    //��Ʈ��ũ�� �����ҋ� ȣ���ϴ� �Լ�(�Ϲ������� ToMaster�� ����Ѵ�)
    public override void OnConnectedToMaster()
    {
        print("��������");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        print("�κ� ����");
        //PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print($"������ ���� : {returnCode} : {message}");
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
        print("����� �Ϸ�");
    }

    public override void OnJoinedRoom()
    {
        print("�� ���� �Ϸ�");
        print(PhotonNetwork.CurrentRoom.Name);
        SceneManager.LoadScene("03_GameScene");
    }

    private Dictionary<string,GameObject> _roomDic= new Dictionary<string,GameObject>();
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var room in roomList)
        {
            //������ ���� ���� �� ��Ͽ��� �����
            if(room.RemovedFromList==true)
            {
                if(_roomDic.TryGetValue(room.Name,out GameObject roomdel))
                {
                    Destroy(roomdel);
                    _roomDic.Remove(room.Name);
                }
                continue;
            }
            //�븮��Ʈ���� �ִµ� dic�� ���� �� (�űԷ�)
            if(_roomDic.ContainsKey(room.Name)==false)
            {
                var _room = Instantiate(_roomPrefab, _roomContent);
                _room.GetComponent<RoomData>().RoomInfo = room;
                _roomDic.Add(room.Name, _room);
            }
            //�̹� �ִµ� ���ڰ� ������ ���
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
