using Photon.Pun;
using System;
using Unity.Cinemachine;
using UnityEngine;

public class RabbitControl : MonoBehaviour
{
    [Header("PlayerState")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotSpeed;
    [SerializeField] private float _jumpForce;

    [Header("Component")]
    [SerializeField] private CinemachineCamera _cineCamere;
    [SerializeField] private Rigidbody _rig;
    [SerializeField] private Animator _anim;
    [SerializeField] private PhotonView _pv;
    

    private float h => Input.GetAxis("Horizontal");
    private float v => Input.GetAxis("Vertical");


    private void Start()
    {
        _rig = GetComponent<Rigidbody>();
        _cineCamere = FindFirstObjectByType<CinemachineCamera>();
        
        _anim = GetComponent<Animator>();
        _pv= GetComponent<PhotonView>();

        _rig.isKinematic = !_pv.IsMine;
        if(_pv.IsMine)
        {
            _cineCamere.Target.TrackingTarget = this.transform;
        }
    }


    private void Update()
    {
        if (!_pv.IsMine)
            return;
        Move();
        Jump();
    }

    [PunRPC]
    private void Jump()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            _rig.AddForce(Vector3.up*_jumpForce,ForceMode.Force);
    }

    private void Move()
    {
        Vector3 dir = Vector3.forward * v * _moveSpeed * Time.deltaTime;
        _anim.SetFloat("Move", dir.magnitude);
        transform.Translate(dir);
        transform.Rotate(Vector3.up * h * Time.deltaTime * _rotSpeed);
    }
}
