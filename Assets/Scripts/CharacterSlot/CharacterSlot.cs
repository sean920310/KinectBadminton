using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSlot : MonoBehaviour
{
    [SerializeField] HatSO[] hatList;
    public static HatSO[] HatList;

    [SerializeField] public static int player1currentIdx;
    [SerializeField] public static int player2currentIdx;

    [SerializeField] CharacterSlotUI p1SlotUI;
    [SerializeField] CharacterSlotUI p2SlotUI;

    private void Start()
    {
        HatList = hatList;

        player1currentIdx = 0;
        player2currentIdx = 0;

        p1SlotUI.UIUpdate(0);
        p2SlotUI.UIUpdate(0);
    }

    public void OnLeftBtnClick(int player)
    {
        if (player == 1)
        {
            player1currentIdx--;
            Mathf.Clamp(player2currentIdx, 0, HatList.Length - 1);
            p1SlotUI.UIUpdate(player1currentIdx);
        }
        else if (player == 2)
        {
            player2currentIdx--;
            Mathf.Clamp(player2currentIdx, 0, HatList.Length - 1);
            p2SlotUI.UIUpdate(player2currentIdx);
        }

    }
    public void OnRightBtnClick(int player)
    {
        if (player == 1)
        {
            player1currentIdx++;
            Mathf.Clamp(player1currentIdx, 0, HatList.Length - 1);
            p1SlotUI.UIUpdate(player1currentIdx);
        }
        else if (player == 2)
        {
            player2currentIdx++;
            Mathf.Clamp(player2currentIdx, 0 , HatList.Length - 1);
            p2SlotUI.UIUpdate(player2currentIdx);
        }

    }
}

