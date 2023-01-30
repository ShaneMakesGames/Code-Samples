using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum CyberneticTriggers
{
    CYBERNETIC_EQUIPPED,
    ENEMY_HIT,
    ENEMY_KILLED,
    CYBERNETIC_DROPPED,
    PLAYER_TAKES_DAMAGE,
    PLAYER_DASHES,
    COUNT
}

public enum CyberneticSlot
{
    WEAPON,
    ARMS,
    BODY,
    LEGS,
    COUNT
}

public enum WeaponType
{
    BATTLE_AXE,
    DAGGERS,
    GAUNTLETS,
    COUNT
}

public delegate void CyberneticFunction(EnemyDemo enemy = null);

public class Cybernetic : ScriptableObject
{
    public bool[] triggers = new bool[(int)CyberneticTriggers.COUNT];
    public CyberneticFunction[] triggerFunctions = new CyberneticFunction[(int)CyberneticTriggers.COUNT];

    [Header("Visuals")]
    public string cybernerticName;
    public CyberneticSlot slot;
    public string description;
    public WeaponType weaponType;

    public string spriteRef;

    public virtual void AssignTriggers()
    {
        triggers = new bool[(int)CyberneticTriggers.COUNT];
        triggerFunctions = new CyberneticFunction[(int)CyberneticTriggers.COUNT];
    }
}