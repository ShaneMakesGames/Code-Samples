using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class UniqueMove
{
    [Header("Name")]
    public string MoveName; // The move name, used for multi-hitting normals

    // TODO : Replace with numpad notation (Ex. "Forward" == 6)
    [Header("Input")]
    public string DirectionalInput; // The directional input(s) to activate the move

    [Header("Follow-Up")]
    public List<NormalAttack> FollowUpAttacks = new List<NormalAttack>(); // Follow up attacks that can be activated with an additional input

    [Header("Player Frame Data")]
    public int StartUpFrames; // How many frames for the move to become active
    public int ActiveFrames; // How many frames the move is active for
    public int RecoveryFrames; // How many frames after the move when you cannot perform an action

    [Header("Opponent Frame Data")]
    public int HitStopFrames; // How many frames the opponent is in hit stop for
    public int BlockStunFrames; // How many frames the opponent is in blockstun
    public int HitStunFrames; // How many frames the opponent is in hitstun for

    [Header("Properties")]
    public bool hitsHigh; // Is it an overhead
    public bool hitsLow; // Is it a low

    [Header("Hitbox Position")]
    public bool hasHitbox; // Is the move an attack?
    public Vector2 HitboxOffset; // The offset to be passed into the hitbox's BoxColl
    public Vector2 HitboxSize; // The size to be passed into the hitbox's BoxCollider

    [Header("Active")]
    public bool active; // If the move is active
    public bool followUpActive; // If the follow up is active
    public bool checkRunning; // If the motion is being checked for completion

    public delegate void UniqueMoveDelegate();
    public UniqueMoveDelegate UniqueMoveFunction; // A function that is called upon activation of the unique move
}