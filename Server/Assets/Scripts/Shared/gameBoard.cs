using System;
using System.Collections;
using UnityEngine;

public class gameBoard : MonoBehaviour
{

    private string[] cardDeck;
    private string[] players;

    public gameBoard(string[] players)
    {
        this.players = players;
        cardDeck = new string[] { "Villager",
                                  "Villager",
                                  "Villager",
                                  "Werewolf",
                                  "Werewolf",
                                  "Seer",
                                  "Robber",
                                  "Troublemaker",
                                  "Tanner",
                                  "Drunk",
                                  "Hunter",
                                  "Mason",
                                  "Mason",
                                  "Insomniac",
                                  "Minion",
                                  "Doppelganger"};
    }

    private void swap(int first, int second)
    {
        string temp = cardDeck[first];
        cardDeck[first] = cardDeck[second];
        cardDeck[second] = temp;
    }


    public void shuffle()
    {
        Debug.Log("Before shuffle");
        foreach (string cards in cardDeck)
        {
            Debug.Log(cards);
        }


        System.Random rndm = new System.Random();
        for (int i=0; i<cardDeck.Length-2; i++)
        {
            int j = rndm.Next(0, cardDeck.Length);
            swap(i, j);
        }


        Debug.Log("\n\nAfter shuffle");
        foreach (string cards in cardDeck)
        {
            Debug.Log(cards);
        }

    }
}
