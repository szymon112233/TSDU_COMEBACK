﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameButtons
{
    NONE = 0,
    JUMP,
    ACTION
}

public enum GameAxis
{
    X_MOVEMENT,
    Y_MOVEMENT
}

public class GameInput : MonoBehaviour {

    #region singleton
    public static GameInput instance = null;

    //Awake is always called before any Start functions
    void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        Init();
    }

    #endregion

    public List<InputPlayer> players = new List<InputPlayer>();
    bool globalInputEnabled = true;

    private void Init()
    {
        PCPlayer player0 = new PCPlayer
        {
            id = 0,
            inputEnabled = true,
            jumpKey = KeyCode.UpArrow,
            throwKey = KeyCode.Space,
            xAxisName = "Horizontal",
            yAxisName = "Vertical"
        };

        players.Add(player0);

        PCPlayer player1 = new PCPlayer
        {
            id = 1,
            inputEnabled = true,
            jumpKey = KeyCode.W,
            throwKey = KeyCode.LeftControl,
            xAxisName = "Horizontal2",
            yAxisName = "Vertical2"
        };

        players.Add(player1);
    }


    public bool GetButton(GameButtons button, int player = -1)
    {
        if (player < 0)
        {
            if (globalInputEnabled)
                foreach(InputPlayer pl in players)
                {
                    if (pl.GetButton(button))
                        return true;
                }
        }
        else
        {
            if (players.Count > player)
                return players[player].inputEnabled && players[player].GetButton(button);
            else
                return false;
        }
            
        return false;
    }

    public bool GetButtonPressed(GameButtons button, int player = -1)
    {
        if (player < 0)
        {
            if (globalInputEnabled)
                foreach (InputPlayer pl in players)
                {
                    if (pl.GetButtonPressed(button))
                        return true;
                }
        }
        else
        {
            if (players.Count > player)
                return players[player].inputEnabled && players[player].GetButtonPressed(button);
            else
                return false;
        }

        return false;
    }

    public bool GetButtonReleased(GameButtons button, int player = -1)
    {
        if (player < 0)
        {
            if (globalInputEnabled)
                foreach (InputPlayer pl in players)
                {
                    if (pl.GetButtonReleased(button))
                        return true;
                }
        }
        else
        {
            if (players.Count > player)
                return players[player].inputEnabled && players[player].GetButtonReleased(button);
            else
                return false;
        }

        return false;
    }

    public float GetAxis(GameAxis axis, int player = -1)
    {
        if (player < 0)
        {
            if (!globalInputEnabled)
                return 0.0f;
            float sum = 0.0f;
            foreach (InputPlayer pl in players)
            {
                sum += pl.GetAxis(axis);
            }
            if (sum > 1.0f)
                sum = 1.0f;
            else if (sum < -1.0f)
                sum = -1.0f;

            return sum;
        }
        else
        {
            if (players.Count > player && players[player].inputEnabled)
                return players[player].GetAxis(axis);
            else
                return 0.0f;
        }

    }

    public void SetInputEnabled(bool enabled, int player = -1)
    {
        if (player == -1)
        {
            foreach (InputPlayer pl in players)
            {
                pl.inputEnabled = enabled;
            }
            globalInputEnabled = enabled;
        }
        else
        {
            if (players.Count > player)
                players[player].inputEnabled = enabled;
            else
                Debug.LogErrorFormat("player ({0}) is greater than or equal to players.Count ({1})", player, players.Count);
        }
    }
}

[System.Serializable]
public class InputPlayer
{
    public int id;
    public bool inputEnabled;


    public virtual bool GetButton(GameButtons button)
    {
        return false;
    }

    public virtual bool GetButtonPressed(GameButtons button)
    {
        return false;
    }

    public virtual bool GetButtonReleased(GameButtons button)
    {
        return false;
    }

    public virtual float GetAxis(GameAxis axis)
    {

        return 0.0f;
    }
}

[System.Serializable]
public class PCPlayer : InputPlayer
{
    public KeyCode jumpKey;
    public KeyCode throwKey;
    public string xAxisName;
    public string yAxisName;

    public override bool GetButton(GameButtons button)
    {
        switch (button)
        {
            case GameButtons.NONE:
                return Input.GetKey(KeyCode.None);

            case GameButtons.JUMP:
                return Input.GetKey(jumpKey);

            case GameButtons.ACTION:
                return Input.GetKey(throwKey);

        }

        return false;
    }

    public override bool GetButtonPressed(GameButtons button)
    {
        switch (button)
        {
            case GameButtons.NONE:
                return Input.GetKeyDown(KeyCode.None);

            case GameButtons.JUMP:
                return Input.GetKeyDown(jumpKey);

            case GameButtons.ACTION:
                return Input.GetKeyDown(throwKey);

        }

        return false;
    }

    public override bool GetButtonReleased(GameButtons button)
    {

        switch (button)
        {
            case GameButtons.NONE:
                return Input.GetKeyUp(KeyCode.None);

            case GameButtons.JUMP:
                return Input.GetKeyUp(jumpKey);

            case GameButtons.ACTION:
                return Input.GetKeyUp(throwKey);

        }

        return false;
    }

    public override float GetAxis(GameAxis axis)
    {
        switch (axis)
        {
            case GameAxis.X_MOVEMENT:
                return Input.GetAxis(xAxisName);

            case GameAxis.Y_MOVEMENT:
                return Input.GetAxis(yAxisName);
        }

        return 0.0f;
    }
}


