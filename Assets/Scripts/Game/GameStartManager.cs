using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class GameStartManager : MonoBehaviour
{
    [SerializeField] public Toggle P1BotToggle;
    [SerializeField] public Toggle P2BotToggle;
    [SerializeField] public TMP_Dropdown scoreToWin;
    [SerializeField] public TMP_InputField Player1NameInput;
    [SerializeField] public TMP_InputField Player2NameInput;

    [SerializeField] public CharacterSlot characterSlot;

    private void Start()
    {
        LoadSettings();
    }

    public void SaveSettings()
    {

        PlayerPrefs.SetInt("P1Bot", Convert.ToInt32(P1BotToggle.isOn));
        PlayerPrefs.SetInt("P2Bot", Convert.ToInt32(P2BotToggle.isOn));
        PlayerPrefs.SetInt("ScoreToWin", scoreToWin.value);
        PlayerPrefs.SetString("Player1Name", Player1NameInput.text);
        PlayerPrefs.SetString("Player2Name", Player2NameInput.text);

        PlayerPrefs.Save();
    }
    public void LoadSettings()
    {
        if(PlayerPrefs.HasKey("P1Bot"))
            P1BotToggle.isOn = PlayerPrefs.GetInt("P1Bot") > 0 ? true : false;
        if (PlayerPrefs.HasKey("P2Bot"))
            P2BotToggle.isOn = PlayerPrefs.GetInt("P2Bot") > 0 ? true : false;
        if (PlayerPrefs.HasKey("ScoreToWin"))
            scoreToWin.value = PlayerPrefs.GetInt("ScoreToWin");
        if (PlayerPrefs.HasKey("Player1Name"))
            Player1NameInput.text = PlayerPrefs.GetString("Player1Name");
        if (PlayerPrefs.HasKey("Player2Name"))
            Player2NameInput.text = PlayerPrefs.GetString("Player2Name");
    }

    public void OnResetClick()
    {
        ResetSettings();
    }

    private void ResetSettings()
    {
        characterSlot.ResetSettings();
        P1BotToggle.isOn = P2BotToggle.isOn = false;
        scoreToWin.value = 0;
        Player1NameInput.text = "Player1";
        Player2NameInput.text = "Player2";
    }
}
