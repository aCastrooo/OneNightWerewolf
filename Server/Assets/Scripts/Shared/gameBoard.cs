using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameBoard : MonoBehaviour
{
    private string[] players;
    private string[] cardDeck;

    private List<string> deck;

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

        deck = new List<string>();
        deck.AddRange(cardDeck);
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

    public string[] giveOutCards()
    {
        string[] midCards = new string[3];
        for (int i=0; i<3; i++)
        {
            midCards[i] = cardDeck[(cardDeck.Length-1) - i];

        }

        return null;
    }
}
