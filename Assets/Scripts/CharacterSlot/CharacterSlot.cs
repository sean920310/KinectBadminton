using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSlot : MonoBehaviour
{
    [SerializeField] HatSO[] hatList;
    public static HatSO[] HatList;

    [SerializeField] public static int player1currentHatIdx;
    [SerializeField] public static int player2currentHatIdx;

    [SerializeField] CharacterSlotUI p1SlotUI;
    [SerializeField] CharacterSlotUI p2SlotUI;

    private void Start()
    {
        HatList = hatList;

        LoadSettings();

        p1SlotUI.UIUpdate(player1currentHatIdx);
        p2SlotUI.UIUpdate(player2currentHatIdx);
    }
    public void LoadSettings()
    {
        player1currentHatIdx = PlayerPrefs.HasKey("player1HatIdx") ? PlayerPrefs.GetInt("P1Bot") : 0;
        player2currentHatIdx = PlayerPrefs.HasKey("player2HatIdx") ? PlayerPrefs.GetInt("P2Bot") : 0;
        PlayerPrefs.Save();
    }
    public void ResetSettings()
    {
        player1currentHatIdx = 0;
        player2currentHatIdx = 0;

        p1SlotUI.UIUpdate(player1currentHatIdx);
        p2SlotUI.UIUpdate(player2currentHatIdx);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("player1HatIdx", player1currentHatIdx);
        PlayerPrefs.SetInt("player2HatIdx", player2currentHatIdx);
        PlayerPrefs.Save();
    }
    public void OnLeftBtnClick(int player)
    {
        if (player == 1)
        {
            player1currentHatIdx--;
            Mathf.Clamp(player2currentHatIdx, 0, HatList.Length - 1);
            p1SlotUI.UIUpdate(player1currentHatIdx);
        }
        else if (player == 2)
        {
            player2currentHatIdx--;
            Mathf.Clamp(player2currentHatIdx, 0, HatList.Length - 1);
            p2SlotUI.UIUpdate(player2currentHatIdx);
        }

    }
    public void OnRightBtnClick(int player)
    {
        if (player == 1)
        {
            player1currentHatIdx++;
            Mathf.Clamp(player1currentHatIdx, 0, HatList.Length - 1);
            p1SlotUI.UIUpdate(player1currentHatIdx);
        }
        else if (player == 2)
        {
            player2currentHatIdx++;
            Mathf.Clamp(player2currentHatIdx, 0 , HatList.Length - 1);
            p2SlotUI.UIUpdate(player2currentHatIdx);
        }

    }
}

