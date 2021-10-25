using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
#region variables

    public Transform target;

    [Space(15)]
    [SerializeField] private float _xOffset = 0.0f;
    [SerializeField] private float _yOffset = 0.0f;
    [SerializeField] private float _zOffset = 0.0f;
    [SerializeField] private float _smoothTime = 2.0f;

    private Vector3 _wantedPosition = Vector3.zero;
    private GameManager _gameManager;

#endregion

    private void Start() {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void FixedUpdate() {
        LookToTarget();

        if(_gameManager.isGameEnd) return;

        FollowTheTarget();
    }

    private void LookToTarget(){
        Vector3 direction = target.position - transform.position;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), _smoothTime * Time.deltaTime);        
    }

    private void FollowTheTarget(){
        _wantedPosition = new Vector3(target.position.x + _xOffset, target.position.y + _yOffset, target.position.z + _zOffset);
        transform.position = Vector3.Lerp(transform.position, _wantedPosition, _smoothTime * Time.deltaTime);
    }
}
