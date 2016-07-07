using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int NumRoundsToWin = 5;           
    public float StartDelay = 3f;            
    public float EndDelay = 3f;              
    public CameraControl CameraControl;      
    public Text MessageText;
    public Text ScoreText;            
    public GameObject TankPrefab;            
    public TankManager[] Tanks;              
    
    private int _roundNumber;                 
    private WaitForSeconds _startWait;        
    private WaitForSeconds _endWait;          
    private TankManager _roundWinner;         
    private TankManager _gameWinner;                    
    
    private void Start()
    {
        _startWait = new WaitForSeconds(StartDelay);
        _endWait = new WaitForSeconds(EndDelay);
        
        SpawnAllTanks();
        SetCameraTargets();
        StartCoroutine(GameLoop());
        ShowPionts();
    }
    
    private void SpawnAllTanks()
    {
        int i = 1;

        foreach (TankManager manager in Tanks)
        {
            manager.Instance = Instantiate(TankPrefab, manager.SpawnPoint.position, manager.SpawnPoint.rotation) as GameObject;
            manager.PlayerNumber = i++;
            manager.Setup();
        }
    }
    
    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[Tanks.Length];
        
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = Tanks[i].Instance.transform;
        }
       
        CameraControl.m_Targets = targets;
    }
    
    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());
        
        if (_gameWinner != null)
        {
            Application.LoadLevel(Application.loadedLevel);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }
    
    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();
       
        CameraControl.SetStartPositionAndSize();
        _roundNumber++;
        MessageText.text = "ROUND " + _roundNumber;
        
        yield return _startWait;
    }
    
    private IEnumerator RoundPlaying()
    {
        EnableTankControl();
        
        MessageText.text = string.Empty;
        
        while (!OneTankLeft())
        {
            yield return null;
        }
    }
    
    private IEnumerator RoundEnding()
    {
        DisableTankControl();
        
        _roundWinner = null;
        
        _roundWinner = GetRoundWinner();
        
        if (_roundWinner != null)
            _roundWinner.Wins++;
        
        _gameWinner = GetGameWinner();

        ShowPionts();
        MessageText.text =  EndMessage();
        
        yield return _endWait;
    }
    
    private bool OneTankLeft()
    {
        int numTanksLeft = 0;
        
        for (int i = 0; i < Tanks.Length; i++)
        {
            if (Tanks[i].Instance.activeSelf)
                numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            if (Tanks[i].Instance.activeSelf)
                return Tanks[i];
        }
        
        return null;
    }
    
    private TankManager GetGameWinner()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            if (Tanks[i].Wins == NumRoundsToWin)
                return Tanks[i];
        }
        
        return null;
    }
    
    private string EndMessage()
    {
        string message = "DRAW!";

        if (_roundWinner != null)
        {
            message = _roundWinner.ColoredPlayerText + " WINS THE ROUND!";
        }

        message += "\n\n\n\n";
        
        for (int i = 0; i < Tanks.Length; i++)
        {
            message += Tanks[i].ColoredPlayerText + ": " + Tanks[i].Wins + " WINS\n";
        }

        if (_gameWinner != null)
        {
            message = _gameWinner.ColoredPlayerText + " WINS THE GAME!";
        }

        return message;
    }

    private void ResetAllTanks()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            Tanks[i].Reset();
        }
    }
    
    private void EnableTankControl()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            Tanks[i].EnableControl();
        }
    }
    
    private void DisableTankControl()
    {
        for (int i = 0; i < Tanks.Length; i++)
        {
            Tanks[i].DisableControl();
        }
    }

    private void ShowPionts()
    {
        ScoreText.text = "ROUND: " + _roundNumber + "\n";

        foreach (TankManager manager in Tanks)
        {
            ScoreText.text += manager.ColoredPlayerText + ": " + manager.Wins + "\n";
        }
    }
}