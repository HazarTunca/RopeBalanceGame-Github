using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
#region variables

    public bool isGameEnd = false;
    public bool isGameJustStart = true;
    public bool isWin = false;

    [Header("Variable Management")] [Space(15)]
    [SerializeField] private float _gameStartTimeTreshold = 3.5f;

    [Header("UI")] [Space(15)]
    public GameObject winPanel;
    public GameObject losePanel;

    private float _gameStartTime = 0.0f;

#endregion

    private void Update() {
        if(isGameJustStart) SetGameStartTime();

        if(isGameEnd){
            if(isWin) EnableWinPanel();
            else EnableLosePanel();
        }
    }

#region UI

    public void EnableWinPanel() => winPanel.SetActive(true);
    public void EnableLosePanel() => losePanel.SetActive(true);

    public void AgainButton(){

    }

    public void NextLevelButton(){

    }

    public void ShopButton(){
        
    }

#endregion

    private void SetGameStartTime(){
        _gameStartTime += Time.deltaTime;

        if(_gameStartTime >= _gameStartTimeTreshold){
            _gameStartTime = 0.0f;
            isGameJustStart = false;
        }
    }
}
