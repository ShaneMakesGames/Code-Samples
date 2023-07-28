using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum StatusType
{
    BURN,
    LIGHTNING,
    VOID,
    COUNT
}

public enum StatusTriggers
{
    TRIGGERS_WHEN_APPLIED,
    TRIGGERED_FROM_TIME,
    TRIGGERED_FROM_STACKS,
    COUNT
}

public partial class StatusEffect
{
    public StatusType statusType;

    public bool isEffectSetup;
    public bool shouldExpire;

    public EnemyDemo myEnemy;

    public int stacksApplied;
    public int stacksToTrigger;
    public int maxStacks;

    public float timeSinceLastTrigger;
    public float totalTimePassed;

    public float timeToTrigger;
    public float timeToExpire;

    public bool[] triggers;

    public delegate void TriggerFunction();
    public TriggerFunction ApplicationTriggerFunction; // Triggers when status effect is first applied
    public TriggerFunction TimeTriggerFunction; // Triggers after a set amount of time
    public TriggerFunction StackTriggerFunction; // Triggers after a certain amount of stacks are applied

    public StackVisual stackVisual;

    public StatusEffect()
    {
        triggers = new bool[(int)StatusTriggers.COUNT];
    }

    public void SetupTriggersByStatusType(StatusType _statusType)
    {
        statusType = _statusType;

        switch (statusType)
        {
            case StatusType.BURN:
                triggers[(int)StatusTriggers.TRIGGERED_FROM_TIME] = true;
                TimeTriggerFunction += TakeBurnDamage;
                timeToTrigger = 1;
                maxStacks = 5;
                timeToExpire = 5;
                break;
            case StatusType.LIGHTNING:
                triggers[(int)StatusTriggers.TRIGGERS_WHEN_APPLIED] = true;
                ApplicationTriggerFunction += CreateLightningStrike;
                maxStacks = 3;
                timeToExpire = 5;
                break;
            case StatusType.VOID:
                triggers[(int)StatusTriggers.TRIGGERED_FROM_STACKS] = true;
                StackTriggerFunction += FreezeEnemy;
                maxStacks = 3;
                stacksToTrigger = 3;
                timeToExpire = 2;
                break;
        }

        isEffectSetup = true;
    }

    public void StatusApplied(EnemyDemo enemy)
    {
        myEnemy = enemy;

        stacksApplied++;
        totalTimePassed = 0;

        if (stacksApplied > maxStacks) stacksApplied = maxStacks;
        stackVisual.SetStackCountText(stacksApplied);

        if (triggers[(int)StatusTriggers.TRIGGERS_WHEN_APPLIED])
        {
            if (ApplicationTriggerFunction != null) ApplicationTriggerFunction();
        }

        if (stacksApplied >= stacksToTrigger) StacksTriggered();
    }

    public void TimeTriggered()
    {
        if (!triggers[(int)StatusTriggers.TRIGGERED_FROM_TIME]) return;

        if (TimeTriggerFunction != null) TimeTriggerFunction();
    }

    public void StacksTriggered()
    {
        if (!triggers[(int)StatusTriggers.TRIGGERED_FROM_STACKS]) return;

        if (StackTriggerFunction != null) StackTriggerFunction();
    }

    public void TickTimeUp(float deltaTime)
    {
        timeSinceLastTrigger += deltaTime;
        totalTimePassed += deltaTime;

        if (timeSinceLastTrigger >= timeToTrigger)
        {
            timeSinceLastTrigger = 0;
            TimeTriggered();
        }

        if (totalTimePassed >= timeToExpire)
        {
            shouldExpire = true;
        }
    }

    public void DisableTriggersByStatusType(StatusType _statusType)
    {
        statusType = _statusType;

        for (int i = 0; i < (int)StatusTriggers.COUNT; i++)
        {
            triggers[i] = false;
        }

        switch (statusType)
        {
            case StatusType.BURN:
                TimeTriggerFunction -= TakeBurnDamage;
                break;
            case StatusType.LIGHTNING:
                ApplicationTriggerFunction -= CreateLightningStrike;
                break;
            case StatusType.VOID:
                myEnemy.isFrozen = false;
                myEnemy.agent.isStopped = false;
                myEnemy.animator.enabled = true;
                myEnemy.ResetCurrentAttack();
                StackTriggerFunction -= FreezeEnemy;
                break;
        }

        isEffectSetup = false;
    }

    public void OnDisable()
    {
        if (isEffectSetup) DisableTriggersByStatusType(statusType);
    }
}