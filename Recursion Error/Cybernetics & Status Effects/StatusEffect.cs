using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum StatusType
{
    BURN,
    FREEZE,
    LIGHTNING,
    VOID
}

public enum StatusTriggers
{
    TRIGGERS_WHEN_APPLIED, // Triggers when a status is applied
    TRIGGERED_FROM_TIME, // Trigger after a certain amount of time
    TRIGGERED_FROM_STACKS, // Triggers after a certain amount of stacks
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

    public float timeSinceLastTrigger;
    public float totalTimePassed;

    public float timeToTrigger;
    public float timeToExpire;

    public bool[] triggers;

    public delegate void TriggerFunction();
    public TriggerFunction ApplicationTriggerFunction;
    public TriggerFunction TimeTriggerFunction;
    public TriggerFunction StackTriggerFunction;

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
                triggers[(int)StatusTriggers.TRIGGERED_FROM_STACKS] = true;
                triggers[(int)StatusTriggers.TRIGGERED_FROM_TIME] = true;
                TimeTriggerFunction += TakeBurnDamage;
                StackTriggerFunction += TakeScorchDamage;
                timeToTrigger = 1;
                timeToExpire = 5;
                stacksToTrigger = 5;
                break;
        }

        isEffectSetup = true;
    }

    public void StatusApplied(EnemyDemo enemy)
    {
        myEnemy = enemy;

        if (triggers[(int)StatusTriggers.TRIGGERS_WHEN_APPLIED])
        {
            if (ApplicationTriggerFunction != null) ApplicationTriggerFunction();
        }

        stacksApplied++;
        totalTimePassed = 0;
        stackVisual.SetStackCountText(stacksApplied);

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
                StackTriggerFunction -= TakeScorchDamage;
                break;
        }

        isEffectSetup = false;
    }

    public void OnDisable()
    {
        if (isEffectSetup) DisableTriggersByStatusType(statusType);
    }
}