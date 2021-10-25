using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
#region variables

    public Transform stick;
    public Transform gfx;
    public MeshRenderer[] weightsMeshRenderers;

    /*
        weight 0 -> left 1 (farest one to player)
        weight 1 -> left 2
        weight 2 -> left 3
        weight 3 -> left 4 (closest one to player)
        weight 4 -> right 1 (closest one to player)
        weight 5 -> right 2
        weight 6 -> right 3
        weight 7 -> right 4 (farest one to player)
    */

    [Header("Input")] [Space(15)]
    [SerializeField] private float _touchFactor = 1.8f;

    [Header("Move")] [Space(15)]
    [SerializeField] private float _speed = 1.0f;
    [SerializeField] private float _fallSpeed = 4.25f;
    [SerializeField] private float _yOffset = 0.0f;
    [SerializeField] private float _zOffset = 0.0f;
    [SerializeField] private float _radius = 0.3f;
    [SerializeField] private LayerMask _checkLayer;

    [Header("Fall")] [Space(15)]
    [SerializeField] private float _fallAngle = 50.0f;
    [SerializeField] private float _rotationTimeTreshold = 3.25f;
    [SerializeField] private float _minRotationSpeed = 0.5f;
    [SerializeField] private float _maxRotationSpeed = 2.0f;

    private float _rotationSpeed = 0.5f;
    private float _rotationTime = 0.0f;
    private float _targetAngle = 0.0f;
    private bool _fallFlag = false;

    private CharacterController _controller;
    private Rigidbody _gfxRB;
    private Rigidbody _stickRB;
    private GameManager _gameManager;
    private bool _gameOverFlag = false;
    private bool _isPlayerBendingRight = false;

#endregion

    private void Start() {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _controller = GetComponent<CharacterController>();
        _gfxRB = gfx.GetComponent<Rigidbody>();
        _stickRB = stick.GetComponent<Rigidbody>();
    }

    private void Update() {
        if(_gameManager.isGameEnd){
            if(!_gameManager.isWin && _gameOverFlag){
                Die();
                _gameOverFlag = false;
            }
            else if(_gameManager.isWin){
                gfx.rotation = Quaternion.Slerp(gfx.rotation, Quaternion.identity, 3.5f * Time.deltaTime);
                stick.rotation = Quaternion.Slerp(stick.rotation, Quaternion.identity, 3.5f * Time.deltaTime);

                _gfxRB.velocity = Vector3.zero;
                Invoke("ResetWeightMeshRenderer", 2.0f);
            }
            return;
        }

        CheckLand();
        CheckFall();        
    }

    private void FixedUpdate() {
        if(_gameManager.isGameEnd) return;

        if(!_gameManager.isGameJustStart) FallNew();
        MoveForward();
    }

#region Main Functions

    private void MoveForward(){
        // _controller.Move(transform.forward * _speed * Time.deltaTime);
        _gfxRB.velocity = transform.forward * _speed;
    }

    private void CheckLand(){
        Vector3 wantedPosition = new Vector3(gfx.position.x, gfx.position.y + _yOffset, gfx.position.z + _zOffset);
        
        if(Physics.CheckSphere(wantedPosition, _radius, _checkLayer, QueryTriggerInteraction.Ignore)){
            _gameManager.isGameEnd = true;
            _gameManager.isWin = true;
        }
    }

    private void FallNew(){
        if(!_fallFlag){
            _targetAngle = Random.Range(-1, 1);
            if(_targetAngle < 0) {
                _isPlayerBendingRight = true;
            }
            else {
                _isPlayerBendingRight = false;
            }

            _rotationSpeed = Random.Range(_minRotationSpeed, _maxRotationSpeed);

            SummonStickWeights(_rotationSpeed);

            _fallFlag = true;
        }

        _rotationTime += Time.deltaTime;

        if(_rotationTime >= _rotationTimeTreshold){
            _fallFlag = false;
            _rotationTime = 0.0f;
            return;
        }

        if(_isPlayerBendingRight) {
            _gfxRB.AddTorque(-gfx.forward * _rotationSpeed, ForceMode.Force);
            _stickRB.AddTorque(-gfx.forward * _rotationSpeed * 1.25f, ForceMode.Force);
        }
        else {
            _gfxRB.AddTorque(gfx.forward * _rotationSpeed, ForceMode.Force);
            _stickRB.AddTorque(gfx.forward * _rotationSpeed * 1.25f, ForceMode.Force);
        }
        Debug.Log(_rotationSpeed); /// !!!!!!!!!!!!
    }

    private void CheckFall(){
        if((gfx.eulerAngles.z < 360.0f - _fallAngle && gfx.eulerAngles.z > 260.0f) || (gfx.eulerAngles.z > _fallAngle && gfx.eulerAngles.z < 100.0f)){
            _gameManager.isGameEnd = true;
            _gameOverFlag = true;
            _gameManager.isWin = false;
        }
    }

    private void Die(){
        _gfxRB.isKinematic = false;
        _gfxRB.useGravity = true;
        _gfxRB.constraints = RigidbodyConstraints.None;

        // lying right
        Vector3 forceDir = new Vector3(1.0f, 1.0f, 0.0f);
        Vector3 torquedir = new Vector3(0.0f, 0.0f, -1.0f);

        // Lying left
        if(!_isPlayerBendingRight){
            forceDir = new Vector3(-1.0f, 1.0f, 0.0f);
            torquedir = new Vector3(0.0f, 0.0f, 1.0f);
        }
        
        _gfxRB.AddForce(forceDir.normalized * _fallSpeed, ForceMode.Impulse);
        _gfxRB.AddTorque(torquedir.normalized * _fallSpeed / 1.5f, ForceMode.Impulse);

        // Stick
        stick.parent = null;
        Rigidbody stickRb = stick.GetComponent<Rigidbody>();
        stickRb.isKinematic = false;
        stickRb.useGravity = true;
        stickRb.constraints = RigidbodyConstraints.None;

        // lying right
        Vector3 stickForceDir = new Vector3(-1.0f, 1.0f, 0.0f);
        Vector3 stickTorquedir = new Vector3(0.0f, 0.0f, -1.0f);

        // Lying left
        if(!_isPlayerBendingRight){
            stickForceDir = new Vector3(1.0f, 1.0f, 0.0f);
            stickTorquedir = new Vector3(0.0f, 0.0f, 1.0f);
        }

        stickRb.AddForce(stickForceDir.normalized * _fallSpeed * 2.5f, ForceMode.Impulse);
        stickRb.AddTorque(stickTorquedir.normalized * _fallSpeed, ForceMode.Impulse);
    }

    private void SummonStickWeights(float rotationSmoothTime){
        // summon closest one
        if(rotationSmoothTime >= _minRotationSpeed && rotationSmoothTime < _minRotationSpeed + 0.3f){
            if(_isPlayerBendingRight){
                ResetWeightMeshRenderer();
                weightsMeshRenderers[4].enabled = true;
            }
            else{
                ResetWeightMeshRenderer();
                weightsMeshRenderers[3].enabled = true;
            }
        }
        // summon second closest one
        else if(rotationSmoothTime >= _minRotationSpeed + 0.3f && rotationSmoothTime < _minRotationSpeed + 0.6f){
            if(_isPlayerBendingRight){
                ResetWeightMeshRenderer();
                weightsMeshRenderers[5].enabled = true;
            }
            else{
                ResetWeightMeshRenderer();
                weightsMeshRenderers[2].enabled = true;
            }
        }
        // summon closest one and second closest one
        else if(rotationSmoothTime >= _minRotationSpeed + 0.6f && rotationSmoothTime < _minRotationSpeed + 0.9f){
            if(_isPlayerBendingRight){
                ResetWeightMeshRenderer();
                weightsMeshRenderers[4].enabled = true;
                weightsMeshRenderers[5].enabled = true;
            }
            else{
                ResetWeightMeshRenderer();
                weightsMeshRenderers[3].enabled = true;
                weightsMeshRenderers[2].enabled = true;
            }
        }
        // summon farest one
        else if(rotationSmoothTime >= _minRotationSpeed + 0.9f && rotationSmoothTime < _minRotationSpeed + 1.2f){
            if(_isPlayerBendingRight){
                ResetWeightMeshRenderer();
                weightsMeshRenderers[7].enabled = true;
            }
            else{
                ResetWeightMeshRenderer();
                weightsMeshRenderers[0].enabled = true;
            }
        }
        // summon farest one and second farest one
        else if(rotationSmoothTime >= _minRotationSpeed + 1.2f && rotationSmoothTime < _minRotationSpeed + 1.5f){
            if(_isPlayerBendingRight){
                ResetWeightMeshRenderer();
                weightsMeshRenderers[7].enabled = true;
                weightsMeshRenderers[6].enabled = true;
            }
            else{
                ResetWeightMeshRenderer();
                weightsMeshRenderers[0].enabled = true;
                weightsMeshRenderers[1].enabled = true;
            }
        }
        // summon all of them
        else if(rotationSmoothTime >= _minRotationSpeed + 1.5f){
            if(_isPlayerBendingRight){
                ResetWeightMeshRenderer();
                weightsMeshRenderers[4].enabled = true;
                weightsMeshRenderers[5].enabled = true;
                weightsMeshRenderers[6].enabled = true;
                weightsMeshRenderers[7].enabled = true;
            }
            else{
                ResetWeightMeshRenderer();
                weightsMeshRenderers[0].enabled = true;
                weightsMeshRenderers[1].enabled = true;
                weightsMeshRenderers[2].enabled = true;
                weightsMeshRenderers[3].enabled = true;
            }
        }
    }

    private void ResetWeightMeshRenderer(){
        foreach(MeshRenderer weight in weightsMeshRenderers){
                    weight.enabled = false;
                }
    }

#endregion

#region Control Buttons

    public void RightButton() => _gfxRB.AddTorque(gfx.forward * _touchFactor, ForceMode.Impulse);
    public void LeftButton() => _gfxRB.AddTorque(-gfx.forward * _touchFactor, ForceMode.Impulse);

#endregion

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        Vector3 wantedPosition = new Vector3(gfx.position.x, gfx.position.y + _yOffset, gfx.position.z + _zOffset);
        Gizmos.DrawWireSphere(wantedPosition, _radius);
    }
}
