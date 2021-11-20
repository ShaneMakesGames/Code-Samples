using UnityEngine;

[System.Serializable]
public class SpecialMove
{
    [Header("Name")]
    public string MoveName; // The move name

    [Header("Input")]
    public string MotionInputName; // The motion input name (Ex. "Quarter Circle Forward")
    public string[] MotionInput; // The directional inputs that make up the motion input
    public string[] ButtonInput; // the button input to be pressed after the motion is completed

    [Header("Player Frame Data")]
    public int StartUpFrames; // How many frames for the move to become active
    public int ActiveFrames;  // How many frames the move is active for
    public int RecoveryFrames; // How many frames after the move when you cannot perform an action

    [Header("Opponent Frame Data")]
    public int HitStopFrames; // How many frames the opponent is in hit stop for
    public int BlockStunFrames; // How many frames the opponent is in blockstun
    public int HitStunFrames; // How many frames the opponent is in hitstun for

    [Header("Properties")]
    public bool hitsHigh; // Is it an overhead?
    public bool hitsLow; // Is it a low?
    public bool jumpCancelable; // Can it be jump canceled?
    public bool holdOk; // Can the move be held to delay its startup or change its properties?
    public bool airOk; // Can the move be used in mid air?

    [Header("Launcher/Knockdown")]
    public bool isKnockdown; // Does the move knockdown the opponent on hit?
    public bool isLauncher; // Does the move launch the opponent on hit?

    [Header("Pushback")]
    public float BlockPushBack; // How much the move pushes the opponent back on block
    public float HitPushBack; // How much the move pushes the opponent back on hit

    [Header("Hitbox Position")]
    public Vector2 HitboxOffset; // The offset to be passed into the hitbox's BoxCollider
    public Vector2 HitboxSize; // The size to be passed into the hitbox's BoxCollider

    [Header("Check Running")]
    public bool checkRunning; // If the motion is being checked for completion

    public delegate void SpecialMoveDelegate();
    public SpecialMoveDelegate SpecialMoveFunction; // A function that is called upon activation of the special move
}