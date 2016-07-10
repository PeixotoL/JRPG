﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BattleManager : MonoBehaviour {

    enum BattlePhase
    {
        BattleStart,
        ChooseAction,
        DoAction,
        BattleEnd
    }
    BattlePhase currentBattlePhase = BattlePhase.BattleStart;

    // Stores battlers from greatest to least speed (determines order at start of battle)
    // Any action that changes the speed of a battler should resort
    GameObject[] battlers;

    class SpeedComparer : IComparer
    {
        int IComparer.Compare(System.Object x, System.Object y)
        {
            GameObject gameObj = (GameObject)x;
            int xSpeed = gameObj.GetComponent<Battler>().battleState.speed;

            gameObj = (GameObject)y;
            int ySpeed = gameObj.GetComponent<Battler>().battleState.speed;

            if (xSpeed > ySpeed) return -1;
            else if (xSpeed == ySpeed) return 0;
            else return 1;
        }
    }

    Battler activeBattler;
    int activeBattlerIndex = 0;

    bool choosingAction = false;
    bool doingAction = false;
    bool exitingBattle = false;

    // Use this for initialization
    void Start () {
        GameObject player = GameObject.FindGameObjectWithTag("Player").transform.parent.gameObject;
        player.GetComponent<PlayerBattleController>().battleState =
            PlayerStateManager.Instance.PlayerBattleState;

        battlers = GameObject.FindGameObjectsWithTag("Battler");
        IComparer speedComparer = new SpeedComparer();
        Array.Sort(battlers, speedComparer);

        activeBattler = battlers[activeBattlerIndex].GetComponent<Battler>();

    }
    
    // Update is called once per frame
    void Update () {
        switch (currentBattlePhase)
        {
            case BattlePhase.BattleStart:
				for(int j = 0; j < battlers.Length; j++)
				StartCoroutine(CombatUI.Instance.UpdateHealthBar((double)battlers[j].GetComponent<Battler>().battleState.currentHealth, 
					(double)battlers[j].GetComponent<Battler>().battleState.maximumHealth, battlers[j].name == "PlayerDuringBattle"));StartCoroutine(CombatUI.Instance.DisplayMessage("The battle has started.",3));
    
				currentBattlePhase = BattlePhase.ChooseAction;
                break;
            case BattlePhase.ChooseAction:
                if(!choosingAction)
                {
                    choosingAction = true;
                    StartCoroutine(activeBattler.ChooseAction(FinishChoosingAction));
                }
                break;
            case BattlePhase.DoAction:
                if(!doingAction)
                {
                    doingAction = true;
                    StartCoroutine(activeBattler.DoAction(FinishDoingAction));
                }
                break;
            case BattlePhase.BattleEnd:
                if(!exitingBattle)
                {
                    exitingBattle = true;
                    StartCoroutine(SceneTransitionManager.Instance.ExitBattle());
                }
                break;
            default:
                break;
        }
    }

    void FinishChoosingAction()
    {
        choosingAction = false;
        activeBattlerIndex++;
        if (activeBattlerIndex < battlers.Length)
        {
            activeBattler = battlers[activeBattlerIndex].GetComponent<Battler>();
        }
        else
        {
            activeBattlerIndex = 0;
            activeBattler = battlers[activeBattlerIndex].GetComponent<Battler>();
            currentBattlePhase = BattlePhase.DoAction;
        }
    }

    void FinishDoingAction(bool deathOccurred, bool speedChanged)
    {
        doingAction = false;

        if (deathOccurred)
        {
            currentBattlePhase = BattlePhase.BattleEnd;

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                StartCoroutine(CombatUI.Instance.DisplayMessage("You've been wrecked...", 3));
                return;
            }

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length == 0)
            {
                StartCoroutine(CombatUI.Instance.DisplayMessage("You've wonnerino!", 3));
                return;
            }
        }

        activeBattlerIndex++;
        if (activeBattlerIndex < battlers.Length)
        {
            activeBattler = battlers[activeBattlerIndex].GetComponent<Battler>();
        }
        else
        {
            activeBattlerIndex = 0;
            activeBattler = battlers[activeBattlerIndex].GetComponent<Battler>();
            currentBattlePhase = BattlePhase.ChooseAction;
        }
    }
}
