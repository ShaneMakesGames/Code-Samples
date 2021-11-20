using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NormalAttack
{
    [Header("Name")]
    public string MoveName; // The move name, used for multi-hitting normals

    [Header("Input")]
    public string DirectionalInput; // The directional input(s) to activate the move
    public string ButtonInput; // The button input to activate the move

    [Header("Player Frame Data")]
    public int StartUpFrames; // How many frames for the attack to become active
    public int ActiveFrames; // How many frames the attack is active for
    public int RecoveryFrames; // How many frames after the attack when you cannot perform an action

    [Header("Opponent Frame Data")]
    public int HitStopFrames; // How many frames the opponent is in hit stop for
    public int BlockStunFrames; // How many frames the opponent is in blockstun for
    public int HitStunFrames; // How many frames the opponent is in hitstun for

    [Header("Properties")]
    public bool hitsHigh; // Is it an overhead?
    public bool hitsLow; // Is it a low?
    public bool specialCancelable; // Can it be special canceled?
    public bool jumpCancelable; // Can it be jump canceled?

    [Header("Launcher/Knockdown")]
    public bool isKnockdown; // Does the move knockdown the opponent on hit?
    public bool isLauncher; // Does the move launch the opponent on hit?

    [Header("Availability")]
    public int usedAmount; // How many times the move has been used in the current combo
    public int availableAmount; // How many times the move can be used in a single combo (unless the count is reset)

    [Header("Pushback")]
    public float BlockPushBack; // How much the move pushes the opponent back on block
    public float HitPushBack; // How much the move pushes the opponent back on hit

    [Header("Airborne")]
    public Vector2 airborneMovement;

    [Header("Hitbox Position")]
    public AttackHitbox AttackHitbox;
    public Vector2 HitboxOffset; // The offset to be passed into the hitbox's BoxCollider
    public Vector2 HitboxSize; // The size to be passed into the hitbox's BoxCollider

    [Header("Cancelable Into")]
    public List<string> ChainableNormals = new List<string>(); // The normals that can be chained into from this move on hit
    //public List<string> ChainableNormalsOnBlock = new List<string>(); // The normals that can be chained into from this move on block 

    [Header("Multi-Hitting")]
    public MultiHitData multiHitData;

    public delegate void DelegateFunction();
    public DelegateFunction OnStartUp;
    public DelegateFunction OnActive;
    public DelegateFunction OnRecovery;
    public DelegateFunction OnBlocked;
    public DelegateFunction OnHit;
}