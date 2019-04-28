using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gameBoard : MonoBehaviour
{
    private string[] players;
    private string[] cardDeck;

    private bool[] readyPlayers;

    private LinkedList<string> deck;

    public gameBoard(string[] players)
    {
        this.players = players;
        readyPlayers = new bool[players.Length];

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
        System.Random rndm = new System.Random();
        for (int i=0; i<cardDeck.Length-2; i++)
        {
            int j = rndm.Next(0, cardDeck.Length);
            swap(i, j);
        }

        deck = new LinkedList<string>();

        foreach (string card in cardDeck)
            deck.AddLast(card);
    }

    public string[] dealMiddleCards()
    {
        Debug.Log(deck.Count);
        string[] midCards = new string[3];
        for (int i=0; i<3; i++)
        {
            midCards[i] = deck.First.Value;
            deck.RemoveFirst();
        }
        Debug.Log(deck.Count);
        return midCards;
    }

    public string dealPlayerCard()
    {
        string card = deck.First.Value;
        deck.RemoveFirst();
        Debug.Log(deck.Count);
        return card;
    }

    public void readyUp()
    {
        for (int i=0; i<readyPlayers.Length; i++)
        {
            if (!readyPlayers[i])
            {
                readyPlayers[i] = true;
                break;
            }
        }

        int count = 0;
        Debug.Log("NUM READY PLAYERS");
        foreach (bool b in readyPlayers)
        {
            if (b)
                count++;
        }
        Debug.Log(count);
    }

}
