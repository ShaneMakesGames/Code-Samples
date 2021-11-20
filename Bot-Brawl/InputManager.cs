using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEditor;
using System;

public abstract class InputManager : MonoBehaviour
{

    #region Variables

    private Gamepad gamepad; 
    private PlayerControls controls;
    [Header("Input")]
    public int previousDirectionalInput;
    public int currentDirectionalInput; // The direction the player is holding on their thumbstick
    public string previousButtonInput;
    public string currentButtonInput; // The button the player pushed
    [Header("Character Data")] // Used to import character specific data
    public CharacterData characterData;
    [Header("Normals")] // Lists of the character's normal attacks
    public List<NormalAttack> AllNormals = new List<NormalAttack>();
    private List<string> AllNormalNames = new List<string>();
    [Header("Uniques")]
    public List<UniqueMove> Uniques = new List<UniqueMove>();
    [Header("Specials")]
    public List<SpecialMove> Specials = new List<SpecialMove>();
    [Header("Hurtbox States")]
    public bool FullInvulnerability;
    public bool StrikeInvulnerability;
    public bool ThrowInvulnerability;
    public bool ProjectileInvulnerability;
    public bool inBlockstun;
    public bool inHitstun;
    [Header("Input States")]
    public bool canBlock;
    public bool canAttack;
    public bool canMove;
    [Header("State Info")]
    public bool isAttacking;
    public bool isGroundDashing;
    public bool SpecialMoveActivated; // The motion for a special move has been completed
    public bool UniqueActivated;
    private bool normalChainBuffered; // Used to chain normal attacks into other normal attacks
    private bool uniqueChained; // Used to chain normal attacks into unique

    [Header("Air")]
    public float yVelocity;
    public bool inPreJump;
    public bool isJumping;
    public bool inAir;
    public bool isFalling;
    protected bool canAirDash;
    public bool isAirDashing;
    protected bool airDashBuffered;

    [Header("Transform")]
    public Rigidbody2D charRB;
    protected float xVector, yVector;
    protected Vector2 moveVector;
    protected bool collidingWithOpponent;
    public float movementMultiplier = 1f;

    public int totalFrames;

    // The different states of a normal attack
    public enum NormalAttackState
    {
        StartUp,
        Active,
        Recovery
    }

    // The current state of your normal attack
    public NormalAttackState normalAttackState;

    public NormalAttack currentNormal;
    public NormalAttack bufferedNormal;

    [Header("Animation")]
    public Animator anim;

    [Header("Hitboxes")]
    protected AttackHitbox activeHitbox;
    public Transform hitboxParent;

    protected Keyboard keyboard;
    public Player2Test player2;

    protected List<int> rightDirectionalInputs = new List<int>();
    protected List<int> leftDirectionalInputs = new List<int>();

    [Header("Display")]
    public TextMeshProUGUI directionalInputText;
    public List<Image> ButtonInputMasks = new List<Image>();

    #endregion

    protected virtual void Awake()
    {
        keyboard = Keyboard.current;
        gamepad = Gamepad.current;
        QualitySettings.vSyncCount = 0;  // Disables VSync
        Application.targetFrameRate = 60; // Sets target frame rate to 60 fps

        controls = new PlayerControls();
        controls.Gameplay.Movement.performed += ctx => CheckInputDirection();
        //controls.Gameplay.KeyboardMovement.started += ctx => KeyboardCheckInputDirection();
        //controls.Gameplay.Button.performed += ctx => CheckButtonInput();

        AddCharacterData(characterData);
        SetupHitboxes();

        leftDirectionalInputs.Add(4);
        leftDirectionalInputs.Add(7);

        rightDirectionalInputs.Add(6);
        rightDirectionalInputs.Add(9);
    }

    public void SetupHitboxes()
    {
        for (int i = 0; i < AllNormals.Count; i++)
        {
            GameObject hitboxOBJ = hitboxParent.GetChild(i).gameObject;
            AttackHitbox hitbox = hitboxParent.GetChild(i).GetComponent<AttackHitbox>();
            AllNormals[i].AttackHitbox = hitbox;
            hitbox.inputManager = this;
            hitboxOBJ.SetActive(false);
        }
    }


    /// <summary>
    /// Adds character specific data as variables 
    /// </summary>
    /// <param name="charData"></param>
    private void AddCharacterData(CharacterData charData)
    {
        AddNormalsToList(charData.LightNormals);
        AddNormalsToList(charData.MediumNormals);
        AddNormalsToList(charData.HeavyNormals);
        AddNormalsToList(charData.AirNormals);
        Uniques = charData.Uniques;
        AddNormalsFromUniques(characterData.Uniques);
        Specials = charData.Specials;
    }

    private void AddNormalsToList(List<NormalAttack> normals)
    {
        for (int i = 0; i < normals.Count; i++)
        {
            AllNormals.Add(normals[i]);
            AllNormalNames.Add(normals[i].MoveName);
            if (normals[i].multiHitData != null) // If it is multi-hitting
            {
                for (int multiInt = 0; multiInt < normals[i].multiHitData.multiHits.Count; multiInt++) // Adds all multi-hits to normal lists
                {
                    AllNormals.Add(normals[i].multiHitData.multiHits[multiInt]);
                    AllNormalNames.Add(normals[i].multiHitData.multiHits[multiInt].MoveName);
                }
            }
        }
    }

    private void AddNormalsFromUniques(List<UniqueMove> uniques)
    {
        for (int i = 0; i < uniques.Count; i++)
        {
            if (uniques[i].FollowUpAttacks.Count > 0) // If it is multi-hitting
            {
                for (int followUpNum = 0; followUpNum < uniques[i].FollowUpAttacks.Count; followUpNum++) // Adds all multi-hits to normal lists
                {
                    AllNormals.Add(uniques[i].FollowUpAttacks[followUpNum]);
                    AllNormalNames.Add(uniques[i].FollowUpAttacks[followUpNum].MoveName);
                }
            }
        }
    }

    private void Update()
    {
        //KeyboardCheckInputDirection();
        //KeyboardCheckForNoInput();
        CheckInputDirection();
        CheckButtonInput();

        FacingOpponentCheck();
        Movement();
    }

    #region Movement

    public string FacingDirection = "Right";
    private void FacingOpponentCheck()
    {
        if (charRB.position.x < player2.rb.position.x)
        {
            if (FacingDirection != "Right")
            {
                FacingDirection = "Right";
                charRB.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else
        {
            if (FacingDirection != "Left")
            {
                FacingDirection = "Left";
                charRB.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    protected virtual void Movement()
    {
        transform.position = new Vector2(charRB.position.x, charRB.position.y - 1f);

        if (isAttacking && !inAir || SpecialMoveActivated || UniqueActivated || isAirDashing || !canMove)
        {
            return;
        }

        Vector2 moveDir = new Vector2(xVector, yVector);

        if (isGroundDashing)
        {
            if (FacingDirection == "Right")
            {
                moveDir.x = 0.04f;
            }
            else
            {
                moveDir.x = -0.04f;
            }
        }

        if (collidingWithOpponent && currentDirectionalInput == 6) // Forward
        {
            moveDir.x *= movementMultiplier;
        }

        //charRB.MovePosition(charRB.position + moveDir * Time.deltaTime);
        if (currentDirectionalInput == 1 || currentDirectionalInput == 3)
        {
            moveDir.x = 0;
        }

        charRB.position += moveDir;

    }

    protected Coroutine jumpCoroutine;
    private IEnumerator JumpCoroutine()
    {
        inPreJump = true;
        canBlock = false;
        canAttack = false;
        ThrowInvulnerability = true;
        int framesPassed = 0;
        while (framesPassed < 4)
        {
            yield return null;
            framesPassed++;
        }
        framesPassed = 0;
        inPreJump = false;
        ThrowInvulnerability = false;
        canBlock = true;
        canAttack = true;
        canAirDash = false;
        inAir = true;
        isJumping = true;
        float jumpAmount = characterData.jumpHeight * 0.8f / 5;
        while (framesPassed < 5)
        {
            yield return null;
            yVector = jumpAmount;
            framesPassed++;
            if (framesPassed == 4)
            {
                canAirDash = true;
            }
        }
        framesPassed = 0;
        jumpAmount = characterData.jumpHeight * 0.2f / 6;
        while (framesPassed < 6)
        {
            yield return null;
            yVector = jumpAmount;
            framesPassed++;
        }
        yield return null;
        framesPassed = 0;
        isJumping = false;
        isFalling = true;
        while (charRB.position.y > 1.001f)
        {
            yield return null;
            yVector = -.065f;
            framesPassed++;
        }
        //Debug.Log(20 + framesPassed);
        OnLanding();
        jumpCoroutine = null;
    }

    private void OnLanding()
    {
        charRB.position = new Vector2(charRB.position.x, 1);
        isFalling = false;
        inAir = false;
        isJumping = false;
        yVector = 0;

        if (isAttacking)
        {
            StopCoroutine(normalCoroutine);
            activeHitbox.DisableHitbox();
            StartCoroutine(LandingRecoveryCoroutine(4, true));
        }
        else
        {
            StartCoroutine(LandingRecoveryCoroutine(2, false));
        }
    }

    private IEnumerator LandingRecoveryCoroutine(int recoveryFrames, bool wasAttacking)
    {
        canMove = false;
        if (wasAttacking)
        {
            isAttacking = false;
            canBlock = false;
            canAttack = false;
        }
        int framesPassed = 0;
        while (framesPassed < recoveryFrames)
        {
            yield return null;
            framesPassed++;
            if (wasAttacking && framesPassed < 2)
            {
                canBlock = true;
                canAttack = true;
            }
        }
        canMove = true;
    }

    private bool leftDashCheckRunning = false;
    private IEnumerator CheckForDashLeftCoroutine()
    {
        leftDashCheckRunning = true;

        string[] inputSequence = new string[3];
        inputSequence[0] = "4/7";
        inputSequence[1] = "1/2/3/5/6/8/9";
        inputSequence[2] = "4/7";
        for (int i = 0; i < inputSequence.Length; i++) // Loops through each input in the motion
        {
            // For multiple corect inputs, seperate each input with a '/'
            List<int> correctDirectionalInputs = ParseDirectionalInput(inputSequence[i]);
            CoroutineWithData _coroutineWithData = new CoroutineWithData(this, CompareDirectionalInputCoroutine(correctDirectionalInputs, 6));
            yield return _coroutineWithData.MyCoroutine;
            if (!(bool)_coroutineWithData.result)
            {
                leftDashCheckRunning = false;
                yield break;
            }
            yield return null;
        }

        if (inAir || inPreJump) // Forward Sprint
        {
            AttemptAirDash();
        }
        else // Forward Air Dash
        {
            AttemptGroundDash();
        }

        leftDashCheckRunning = false;
    }

    private bool rightDashCheckRunning = false;
    private IEnumerator CheckForDashRightCoroutine()
    {
        rightDashCheckRunning = true;

        string[] inputSequence = new string[3];
        inputSequence[0] = "6/9";
        inputSequence[1] = "1/2/3/4/5/7/8";
        inputSequence[2] = "6/9";
        for (int i = 0; i < inputSequence.Length; i++) // Loops through each input in the motion
        {
            // For multiple corect inputs, seperate each input with a '/'
            List<int> correctDirectionalInputs = ParseDirectionalInput(inputSequence[i]);
            CoroutineWithData _coroutineWithData = new CoroutineWithData(this, CompareDirectionalInputCoroutine(correctDirectionalInputs, 6));
            yield return _coroutineWithData.MyCoroutine;
            if (!(bool)_coroutineWithData.result)
            {
                rightDashCheckRunning = false;
                yield break;
            }
            yield return null;
        }

        if (inAir || inPreJump) // Forward Sprint
        {
            AttemptAirDash();
        }
        else // Forward Air Dash
        {
            AttemptGroundDash();
        }

        rightDashCheckRunning = false;
    }

    private void OnDashButton()
    {
        if (!isAttacking && !inBlockstun && !inHitstun)
        {
            if (inAir || inPreJump) // Air Dash
            {
                AttemptAirDash();
            }
            else // Ground Dash
            {
                AttemptGroundDash();
            }
        }
    }

    protected virtual bool CanGroundDash()
    {
        return !isGroundDashing;
    }

    protected void AttemptGroundDash()
    {
        if (!CanGroundDash())
        {
            return;
        }

        if (FacingDirection == "Right")
        {
            if (leftDirectionalInputs.Contains(currentDirectionalInput)) // Back
            {
                StartCoroutine(BackDashCoroutine());
            }
            else // Forward
            {
                StartCoroutine(ForwardDashCoroutine());
            }
        }
        else if (FacingDirection == "Left")
        {
            if (rightDirectionalInputs.Contains(currentDirectionalInput)) // Back
            {
                StartCoroutine(BackDashCoroutine());
            }
            else // Forward
            {
                StartCoroutine(ForwardDashCoroutine());
            }
        }
    }

    protected virtual IEnumerator BackDashCoroutine()
    {
        isGroundDashing = true;
        float directionToMove;
        if (FacingDirection == "Right")
        {
            directionToMove = -1;
        }
        else
        {
            directionToMove = 1;
        }

        int framesPassed = 0;
        float amountToMove = 0.75f;
        amountToMove /= 12;
        while (framesPassed < 12)
        {
            if (inBlockstun || inHitstun)
            {
                isGroundDashing = false;
                yield break;
            }
            yield return null;
            charRB.position += new Vector2(amountToMove * directionToMove, 0);
            framesPassed++;
        }
        isGroundDashing = false;
    }

    protected virtual IEnumerator ForwardDashCoroutine()
    {
        isGroundDashing = true;
        while (isGroundDashing)
        {
            if (FacingDirection == "Right")
            {
                if (!rightDirectionalInputs.Contains(currentDirectionalInput) && currentButtonInput != "Dash")
                {
                    isGroundDashing = false;
                    yield break;
                }
            }
            else if (FacingDirection == "Left")
            {
                if (!leftDirectionalInputs.Contains(currentDirectionalInput) && currentButtonInput != "Dash")
                {
                    isGroundDashing = false;
                    yield break;
                }
            }
            if (inBlockstun || inHitstun)
            {
                isGroundDashing = false;
                yield break;
            }
            yield return null;
        }
        isGroundDashing = false;
    }

    protected void AttemptAirDash()
    {
        if (characterData.AirDashesUsed >= characterData.AirDashesAvailable || previousButtonInput == "Dash" | isAirDashing | airDashBuffered)
        {
            return;
        }

        if (FacingDirection == "Right")
        {
            if (leftDirectionalInputs.Contains(currentDirectionalInput)) // Back
            {
                if (!canAirDash && !airDashBuffered)
                {
                    StartCoroutine(BufferAirDashCoroutine(AirDashBackCoroutine()));
                }
                else
                {
                    StartCoroutine(AirDashBackCoroutine());
                }
            }
            else // Forward
            {
                if (!canAirDash && !airDashBuffered)
                {
                    StartCoroutine(BufferAirDashCoroutine(AirDashForwardCoroutine()));
                }
                else
                {
                    StartCoroutine(AirDashForwardCoroutine());
                }
            }
        }
        else if (FacingDirection == "Left")
        {
            if (rightDirectionalInputs.Contains(currentDirectionalInput)) // Back
            {
                if (!canAirDash && !airDashBuffered)
                {
                    StartCoroutine(BufferAirDashCoroutine(AirDashBackCoroutine()));
                }
                else
                {
                    StartCoroutine(AirDashBackCoroutine());
                }
            }
            else // Forward
            {
                if (!canAirDash && !airDashBuffered)
                {
                    StartCoroutine(BufferAirDashCoroutine(AirDashForwardCoroutine()));
                }
                else
                {
                    StartCoroutine(AirDashForwardCoroutine());
                }
            }
        }
    }

    /// <summary>
    /// Starts an air dash on the first frame after a jump has reached it's apex
    /// </summary>
    /// <param name="normal"></param>
    /// <returns></returns>
    private IEnumerator BufferAirDashCoroutine(IEnumerator coroutine)
    {
        airDashBuffered = true;
        while (!inHitstun && !inBlockstun)
        {
            if (canAirDash && !inPreJump)
            {
                airDashBuffered = false;
                StartCoroutine(coroutine);
                break;
            }
            yield return null;
        }
    }


    protected virtual IEnumerator AirDashBackCoroutine()
    {
        canAirDash = false;

        isAirDashing = true;
        float directionToMove;
        if (FacingDirection == "Right")
        {
            directionToMove = -1;
        }
        else
        {
            directionToMove = 1;
        }

        // TODO : Air Dash Startup before movement
         
        int framesPassed = 0;
        while (framesPassed < 4) // Start-up
        {
            yield return null;
            framesPassed++;
        }
        framesPassed = 0;

        isJumping = false;
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
        }
        Debug.Log(charRB.position.y);

        float amountToMove = 1.25f;
        amountToMove /= 12;
        while (framesPassed < 12)
        {
            if (inBlockstun || inHitstun)
            {
                isAirDashing = false;
                yield break;
            }
            yield return null;
            charRB.position += new Vector2(amountToMove * directionToMove, 0);
            framesPassed++;
        }
        isAirDashing = false;
        while (charRB.position.y > 1.001f)
        {
            yield return null;
            yVector = -.084f;
        }
        OnLanding();
    }

    protected virtual IEnumerator AirDashForwardCoroutine()
    {
        canAirDash = false;
        isAirDashing = true;
        float directionToMove;
        if (FacingDirection == "Right")
        {
            directionToMove = 1;
        }
        else
        {
            directionToMove = -1;
        }

        int framesPassed = 0;
        while (framesPassed < 4) // Start-up
        {
            yield return null;
            framesPassed++;
        }
        framesPassed = 0;

        isJumping = false;
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
        }

        float amountToMove = 2.5f;
        amountToMove /= 16;
        while (framesPassed < 16)
        {
            if (inBlockstun || inHitstun)
            {
                isAirDashing = false;
                yield break;
            }
            yield return null;
            charRB.position += new Vector2(amountToMove * directionToMove, 0);
            framesPassed++;
        }
        isAirDashing = false;
        while (charRB.position.y > 1.001f)
        {
            yield return null;
            yVector = -.084f;
        }
        OnLanding();
    }

    #endregion

    #region Directional Input

    /// <summary>
    /// Input detection for controller
    /// </summary>
    private void CheckInputDirection()
    {
        Vector2 vector2 = gamepad.leftStick.ReadValue();
        CalculateInputDirection(vector2);
    }

    /// <summary>
    /// Input detection for keyboard
    /// </summary>
    private void KeyboardCheckInputDirection()
    {
        Vector2 vector2 = Vector2.zero;
        if (!keyboard.anyKey.isPressed)
        {
            CalculateInputDirection(vector2);
        }
        else
        {
            if (keyboard.wKey.isPressed && !keyboard.sKey.isPressed)
            {
                vector2.y = 1;
            }
            if (keyboard.sKey.isPressed && !keyboard.wKey.isPressed)
            {
                vector2.y = -1;
            }
            if (keyboard.aKey.isPressed && !keyboard.dKey.isPressed)
            {
                vector2.x = -1;
            }
            if (keyboard.dKey.isPressed && !keyboard.aKey.isPressed)
            {
                vector2.x = 1;
            }
            CalculateInputDirection(vector2);
        }
    }

    public float GetXMovementVector()
    {
        return xVector;
    }


    public Vector2 showDirection;
    /// <summary>
    /// Converts the vector2 value into a single int for numpad notation (Ex. 6)
    /// </summary>
    /// <param name="vector2"></param>
    private void CalculateInputDirection(Vector2 vector2)
    {
        xVector = vector2.x * 0.02f;

        showDirection = vector2;
        previousDirectionalInput = currentDirectionalInput;
        currentDirectionalInput = GetInputDirection(vector2);
        //directionalInputText.text = "" + currentDirectionalInput;

        if (isAttacking)
        {
            if (currentNormal.specialCancelable)
            {
                for (int i = 0; i < Specials.Count; i++)
                {
                    TryCheckForMotionInput(Specials[i], currentDirectionalInput);
                }
            }
        }

        if (currentDirectionalInput > 6 && !inPreJump && !isJumping)
        {
            if (!isAttacking || isAttacking && currentNormal.jumpCancelable && normalAttackState == NormalAttackState.Recovery)
            {
                jumpCoroutine = StartCoroutine(JumpCoroutine());
            }
        }

        if (!isAttacking && !inBlockstun && !inHitstun)
        {
            if (rightDirectionalInputs.Contains(currentDirectionalInput)) // Moving right
            {
                if (!rightDashCheckRunning)
                {
                    StartCoroutine(CheckForDashRightCoroutine());
                }
            }
            else if (leftDirectionalInputs.Contains(currentDirectionalInput)) // Moving left
            {
                if (!leftDashCheckRunning)
                {
                    StartCoroutine(CheckForDashLeftCoroutine());
                }
            }

            if (!isGroundDashing && !isAirDashing)
            {
                for (int i = 0; i < Specials.Count; i++)
                {
                    TryCheckForMotionInput(Specials[i], currentDirectionalInput);
                }
            }
        }

    }

    [SerializeField]
    private float deadZone = 0.35f;
    /// <summary>
    /// Returns the properly formatted input direction (Ex. 1)
    /// </summary>
    /// <param name="xString"></param>
    /// <param name="yString"></param>
    /// <returns></returns>
    private int GetInputDirection(Vector2 vector2)
    {
        if (vector2.y >= 0.5f)
        {
            if (vector2.x <= -deadZone) 
            {
                return 7; // UpBack
            }
            else if (vector2.x >= deadZone)
            {
                return 9; // UpForward
            }
            else 
            {
                return 8; // Up
            }
        }
        else if (vector2.y <= -deadZone)
        {
            if (vector2.x <= -deadZone)
            {
                return 1; // DownBack
            }
            else if (vector2.x >= deadZone)
            {
                return 3; // DownForward
            }
            else
            {
                return 2; // Down
            }
        }
        else
        {
            if (vector2.x <= -deadZone) 
            {
                return 4; // Back
            }
            else if (vector2.x >= deadZone) 
            {
                return 6; // Forward
            }
            else
            {
                return 5; // Neutral
            }
        }
    }

    #endregion

    #region Button Input

    public bool checkButtonRunning = false;
    /// <summary>
    /// Reads the button input and converts it into a string (Ex. “Light”)
    /// </summary>
    private void CheckButtonInput()
    {
        string buttonInput = "";
        if (gamepad != null)
        {
            if (gamepad.buttonWest.wasPressedThisFrame)
            {
                buttonInput = "Light";
            }
            else if (gamepad.buttonNorth.wasPressedThisFrame)
            {
                buttonInput = "Medium";
            }
            else if (gamepad.buttonEast.wasPressedThisFrame)
            {
                buttonInput = "Heavy";
            }
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                buttonInput = "Unique";
            }
            else if (gamepad.rightTrigger.isPressed)
            {
                buttonInput = "Dash";
            }
        }
        else if (keyboard != null)
        {
            if (keyboard.jKey.wasPressedThisFrame)
            {
                buttonInput = "Light";
            }
            else if (keyboard.iKey.wasPressedThisFrame)
            {
                buttonInput = "Medium";
            }
            else if (keyboard.kKey.wasPressedThisFrame)
            {
                buttonInput = "Heavy";
            }
            else if (keyboard.spaceKey.wasPressedThisFrame)
            {
                buttonInput = "Unique";
            }
            else if (keyboard.leftShiftKey.isPressed)
            {
                buttonInput = "Dash";
            }
        }
        StartCoroutine(DisplayButtonInputCoroutine(buttonInput));
    }


    private IEnumerator DisplayButtonInputCoroutine(string _buttonInput)
    {
        previousButtonInput = currentButtonInput;
        currentButtonInput = _buttonInput;
        if (_buttonInput == "Light" || _buttonInput == "Medium" || _buttonInput == "Heavy")
        {
            DisplayAttackButton(_buttonInput);
            AttemptNormalAttack();
        }
        else if (_buttonInput == "Unique")
        {
            AttemptUnique();
        }
        else if (_buttonInput == "Dash")
        {
            OnDashButton();
        }
        yield return null;
    }

    private void DisplayAttackButton(string _button)
    {
        switch (_button)
        {
            case "Light":
                StartCoroutine(DisplayAttackButtonCoroutine(ButtonInputMasks[0]));
                break;
            case "Medium":
                StartCoroutine(DisplayAttackButtonCoroutine(ButtonInputMasks[1]));
                break;
            case "Heavy":
                StartCoroutine(DisplayAttackButtonCoroutine(ButtonInputMasks[2]));
                break;
        }
    }

    private IEnumerator DisplayAttackButtonCoroutine(Image mask)
    {
        Color newColor = mask.color;
        newColor.a = 0;
        mask.color = newColor;

        while (mask.color.a < .6)
        {
            newColor.a = mask.color.a + Time.deltaTime;
            mask.color = newColor;
            yield return null;
        }
        newColor.a = .6f;
        mask.color = newColor;
    }

    #endregion

    #region Normals

    /// <summary>
    /// Allows you to use any normal again after a combo has ended
    /// </summary>
    public void ResetNormalsUsed()
    {
        for (int i = 0; i < AllNormals.Count; i++)
        {
            AllNormals[i].usedAmount = 0;
            if (AllNormals[i].multiHitData != null)
            {
                AllNormals[i].multiHitData.multiHits[0].usedAmount = 0;
            }
        }
    }

    /// <summary>
    /// Resets your aerial options upon landing
    /// </summary>
    public void ResetAirOptions()
    {
        characterData.AirDashesUsed = 0;
        characterData.JumpsUsed = 0;
    }

    /// <summary>
    /// Starts a normal attack if it can be used in the current situation
    /// </summary>
    private void AttemptNormalAttack()
    {
        if (SpecialMoveActivated || UniqueActivated)
        {
            return;
        }

        if (!isAttacking) // Normal attack from neutral
        {
            if (canAttack)
            {
                NormalAttack normal = GetNormalFromInput(currentButtonInput, currentDirectionalInput);
                if (normal != null)
                {
                    NormalAttackStartUp(normal);
                }
            }
            else
            {
                NormalAttack normal = GetNormalFromInput(currentButtonInput, currentDirectionalInput);
                if (normal != null)
                {
                    StartCoroutine(BufferNormalCoroutine(normal));
                }
            }
        }
        else if (!normalChainBuffered && !attemptBufferNormal)
        {
            StartCoroutine(AttemptBufferNormalCoroutine(currentNormal, currentButtonInput, currentDirectionalInput));
        }
        else
        {
            bufferedNormal = GetNormalFromInput(currentButtonInput, currentDirectionalInput);
        }
    }

    private bool attemptBufferNormal = false;
    private IEnumerator AttemptBufferNormalCoroutine(NormalAttack _currentNormal, string _buttonInput, int _directionalInput)
    {
        attemptBufferNormal = true;
        int framesPassed = 0;
        while (framesPassed < 6)
        {
            if (player2.inBlockstun || player2.inHitstun)
            {
                List<NormalAttack> Normals = GetNormalsFromNames(_currentNormal.ChainableNormals);
                NormalAttack normal = GetChainableNormal(Normals, _buttonInput, _directionalInput);
                if (normal != null)
                {
                    normalChainBuffered = true;
                    StartCoroutine(BufferChainCoroutine(normal));
                }
                break;
            }
            framesPassed++;
            yield return null;
        }
        attemptBufferNormal = false;
    }

    private IEnumerator BufferNormalCoroutine(NormalAttack normal)
    {
        while (!canAttack)
        {
            yield return null;
        }

        if (!inBlockstun || !inHitstun)
        {
            NormalAttackStartUp(normal);
        }
    }


    /// <summary>
    /// Starts a normal attack on the first frame after the current move has finished
    /// </summary>
    /// <param name="normal"></param>
    /// <returns></returns>
    private IEnumerator BufferChainCoroutine(NormalAttack normal)
    {
        while (isAttacking)
        {
            if (normalAttackState == NormalAttackState.Recovery)
            {
                normalChainBuffered = false;
                NormalAttackStartUp(normal);
                yield return null;
                BufferNextNormal();
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// If a button was pressed this frame attempt that normal, otherwise attempt the normal that was buffered previously
    /// </summary>
    protected void BufferNextNormal()
    {
        if (currentButtonInput != "")
        {
            List<NormalAttack> Normals = GetNormalsFromNames(currentNormal.ChainableNormals);
            NormalAttack normal = GetChainableNormal(Normals, currentButtonInput, currentDirectionalInput);
            if (normal != null)
            {
                normalChainBuffered = true;
                StartCoroutine(BufferChainCoroutine(normal));
                bufferedNormal = null;
            }
        }
        if (bufferedNormal != null)
        {
            normalChainBuffered = true;
            StartCoroutine(BufferChainCoroutine(bufferedNormal));
            bufferedNormal = null;
        }
    }

    /// <summary>
    /// Returns a normal attack if it is able to be chained into, otherwise returns null
    /// </summary>
    /// <param name="_buttonInput"></param>
    /// <returns></returns>
    private NormalAttack GetChainableNormal(List<NormalAttack> _normals, string _buttonInput, int _directionalInput)
    {
        for (int i = 0; i < _normals.Count; i++)
        {
            List<int> correctDirectionalInputs = ParseDirectionalInput(_normals[i].DirectionalInput);
            if (_normals[i].ButtonInput == _buttonInput && correctDirectionalInputs.Contains(_directionalInput))
            {
                return _normals[i];
            }
        }
        return null;
    }

    protected NormalAttack GetNormalFromName(string normalName)
    {
        if (AllNormalNames.Contains(normalName))
        {
            return AllNormals[AllNormalNames.IndexOf(normalName)];
        }
        else
        {
            return null;
        }
    }

    protected List<NormalAttack> GetNormalsFromNames(List<string> NormalNames)
    {
        List<NormalAttack> Normals = new List<NormalAttack>();
        for (int i = 0; i < NormalNames.Count; i++)
        {
            if (AllNormalNames.Contains(NormalNames[i]))
            {
                int index = AllNormalNames.IndexOf(NormalNames[i]);
                Normals.Add(AllNormals[index]);
            }
        }
        return Normals;
    }


    /// <summary>
    /// Returns a normal depending on the directional & button input
    /// </summary>
    /// <param name="_buttonInput"></param>
    /// <returns></returns>
    private NormalAttack GetNormalFromInput(string _buttonInput, int _directionalInput)
    {
        if (inAir)
        {
            for (int i = 0; i < characterData.AirNormals.Count; i++)
            {
                if (characterData.AirNormals[i].ButtonInput == _buttonInput)
                {
                    List<int> correctDirectionalInputs = ParseDirectionalInput(characterData.AirNormals[i].DirectionalInput);
                    if (correctDirectionalInputs.Contains(_directionalInput))
                    {
                        return characterData.AirNormals[i];
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < AllNormals.Count; i++)
            {
                if (AllNormals[i].ButtonInput == _buttonInput)
                {
                    List<int> correctDirectionalInputs = ParseDirectionalInput(AllNormals[i].DirectionalInput);
                    if (correctDirectionalInputs.Contains(_directionalInput))
                    {
                        return AllNormals[i];
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Converts the string into a list of ints
    /// </summary>
    /// <param name="_directionalInput"></param>
    /// <returns></returns>
    private List<int> ParseDirectionalInput(string _directionalInput)
    {
        List<int> ConvertedDirectionalInput = new List<int>();
        int intCheck = 0;
        string[] s = _directionalInput.Split('/');
        for (int i = 0; i < s.Length; i++)
        {
            intCheck = int.Parse(s[i]);
            if (intCheck != 0)
            {
                ConvertedDirectionalInput.Add(intCheck);
            }
        }
        return ConvertedDirectionalInput;
    }

    private Coroutine normalCoroutine;
    /// <summary>
    /// Plays an animation and triggers the normal attack’s startup
    /// </summary>
    /// <param name="normal"></param>
    private void NormalAttackStartUp(NormalAttack normal)
    {
        currentNormal = normal;
        if (normal.usedAmount < normal.availableAmount) // Can use normal
        {
            if (normal.MoveName == "Short Upper")
            {
                anim.Play("Uppercut");
            }
            else
            {
                anim.Play(normal.MoveName);
            }
            //GameObject obj = Instantiate(hitboxPrefab, new Vector2(charRB.position.x, charRB.position.y - 1), Quaternion.identity);
            //AttackHitbox hitbox = obj.GetComponent<AttackHitbox>();
            if (!isAttacking) // Normal out of neutral
            {
                normalCoroutine = StartCoroutine(NormalFramesCoroutine(normal));
            }
            else // Chaining a normal
            {
                activeHitbox.isActive = false;
                activeHitbox.gameObject.SetActive(false);
                StopCoroutine(normalCoroutine);
                normalCoroutine = StartCoroutine(NormalFramesCoroutine(normal));
            }
        }
        else
        {
            //Debug.Log("Run out of uses");
        }

    }

    public bool attackConnected;
    /// <summary>
    /// Cycles through the different states of a normal attack (startup, active, & recovery)
    /// </summary>
    /// <param name="hitbox"></param>
    /// <param name="normal"></param>
    /// <returns></returns>
    private IEnumerator NormalFramesCoroutine(NormalAttack normal)
    {
        //StartCoroutine(FrameCounterCoroutine());
        currentNormal = normal;
        isAttacking = true;
        normalAttackState = NormalAttackState.StartUp;
        totalFrames = 0;
        int framesPassed = 0;
        while (framesPassed < normal.StartUpFrames - 1) // Start Up
        {
            yield return null;
            framesPassed++;
            totalFrames++;
        }
        //Debug.Log("Start-Up Complete");
        normalAttackState = NormalAttackState.Active;
        framesPassed = 0;
        ActivateHitbox(normal);
        //hitbox.HitboxSetup(normal);
        while (framesPassed < normal.ActiveFrames) // Active Frames
        {
            yield return null;
            framesPassed++;
            totalFrames++;
            if (attackConnected)
            {
                int remainingFrames = normal.ActiveFrames - framesPassed;
                normalCoroutine = StartCoroutine(HitStopCoroutine(normal, remainingFrames));
                yield break;
            }
        }
        //Debug.Log("Continues Coroutine");
        //Debug.Log("Active Complete");
        if (normal.multiHitData != null) // Multi-hitting normal
        {
            activeHitbox.isActive = false;
            activeHitbox.gameObject.SetActive(false);
            NormalAttackStartUp(normal.multiHitData.multiHits[0]); // TODO : More than one multi hits
            yield break;
        }
        normalAttackState = NormalAttackState.Recovery;
        framesPassed = 0;
        if (activeHitbox != null)
        {
            activeHitbox.isActive = false;
        }
        normalChainBuffered = false;
        while (framesPassed < normal.RecoveryFrames) // RecoveryFrames
        {
            yield return null;
            framesPassed++;
            totalFrames++;
        }
        isAttacking = false;
        totalFrames = 0;
        currentNormal = null;
        if (activeHitbox != null)
        {
            activeHitbox.gameObject.SetActive(false);
        }
        //Debug.Log("Recovery Complete");
    }

    //private int hitboxIndex;
    //private bool hitboxFound = false;
    public void ActivateHitbox(NormalAttack normal)
    {
        normal.AttackHitbox.gameObject.SetActive(true);
        normal.AttackHitbox.HitboxSetup(normal, FacingDirection == "Right" ? 1 : -1);
        activeHitbox = normal.AttackHitbox;
        
        /*for (int i = 0; i < HitboxList.Count; i++) // Alternates through the pool of hitboxes
        {
            if (hitboxIndex >= HitboxList.Count)
            {
                hitboxIndex = 0;
            }
            activeHitbox = HitboxList[hitboxIndex];
            if (!activeHitbox.gameObject.activeSelf)
            {
                hitboxIndex++;
                hitboxFound = true;
                break;
            }
            else
            {
                hitboxIndex++;
            }
        }

        if (hitboxFound) // Activate hitbox
        {
            activeHitbox.gameObject.SetActive(true);
            activeHitbox.HitboxSetup(normal);
        }
        else
        {
            activeHitbox = null;
        }
        */
    }

    /// <summary>
    /// Puts both the player and opponent in hit stop for a given amount of frames
    /// </summary>
    /// <param name="normal"></param>
    /// <param name="remainingActiveFrames"></param>
    /// <returns></returns>
    private IEnumerator HitStopCoroutine(NormalAttack normal, int remainingActiveFrames)
    {
        anim.enabled = false;
        int framesPassed = 0;
        while (framesPassed < normal.HitStopFrames) // Hit Stop
        {
            yield return null;
            framesPassed++;
        }
        anim.enabled = true;
        if (remainingActiveFrames > 0) // Remaining active frames
        {
            normalAttackState = NormalAttackState.Active;
            framesPassed = 0;
            while (framesPassed < normal.ActiveFrames) // Active Frames
            {
                yield return null;
                framesPassed++;
                totalFrames++;
            }
            if (activeHitbox != null)
            {
                activeHitbox.isActive = false;
            }
            //Debug.Log("Active Complete");
        }
        attackConnected = false;
        if (normal.multiHitData != null) // Multi-hitting normal
        {
            activeHitbox.isActive = false;
            activeHitbox.gameObject.SetActive(false);
            NormalAttackStartUp(normal.multiHitData.multiHits[0]); // TODO : More than one multi hits
            yield break;
        }
        else
        {
            normalAttackState = NormalAttackState.Recovery;
            framesPassed = 0;
            normalChainBuffered = false;
            while (framesPassed < normal.RecoveryFrames) // RecoveryFrames
            {
                yield return null;
                framesPassed++;
                totalFrames++;
            }
            isAttacking = false;
            totalFrames = 0;
            currentNormal = null;
            if (activeHitbox != null)
            {
                activeHitbox.gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region Uniques

    /// <summary>
    /// Starts a unique move if it can be used in the current situation
    /// </summary>
    private void AttemptUnique()
    {
        if (!isAttacking)
        {
            for (int i = 0; i < Uniques.Count; i++)
            {
                List<int> correctDirectionalInputs = ParseDirectionalInput(Uniques[i].DirectionalInputs);
                if (correctDirectionalInputs.Contains(currentDirectionalInput))
                {
                    if (Uniques[i].UniqueMoveFunction != null)
                    {
                        UniqueActivated = true;
                        Uniques[i].UniqueMoveFunction();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks for a certain input to trigger a follow up attack while the unique move is still active
    /// </summary>
    /// <param name="unique"></param>
    /// <returns></returns>
    protected IEnumerator CheckForFollowUpAttackCoroutine(UniqueMove unique)
    {
        while (unique.active)
        {
            for (int i = 0; i < unique.FollowUpAttacks.Count; i++)
            {
                string[] correctButtonInputs = unique.FollowUpAttacks[i].ButtonInput.Split('/');
                /*if (ArrayUtility.Contains(correctButtonInputs, currentButtonInput))
                {
                    Debug.Log("Rekka");
                    unique.followUpActive = true;
                    unique.active = false;
                    UniqueActivated = false;
                    NormalAttackStartUp(unique.FollowUpAttacks[i]);
                }*/
            }
            yield return null;
        }
    }

    #endregion

    #region Specials

    /// <summary>
    /// If a check is not already running, check for the motion input.
    /// </summary>
    /// <param name="specialMove"></param>
    private void TryCheckForMotionInput(SpecialMove specialMove, int _directionalInput)
    {
        if (!specialMove.checkRunning && ParseDirectionalInput(specialMove.MotionInput[0]).Contains(_directionalInput))
        {
            StartCoroutine(CheckForMotionInputCoroutine(specialMove));
        }
    }    

    /// <summary>
    /// A coroutine which checks if each sequential input of the motion has been completed.
    /// </summary>
    /// <param name="specialMove"></param>
    /// <returns></returns>
    private IEnumerator CheckForMotionInputCoroutine(SpecialMove specialMove)
    {
        specialMove.checkRunning = true;

        for (int i = 0; i < specialMove.MotionInput.Length; i++) // Loops through each input in the motion
        {
            // For multiple corect inputs, seperate each input with a '/'
            List<int> correctDirectionalInputs = ParseDirectionalInput(specialMove.MotionInput[i]);
            CoroutineWithData _coroutineWithData = new CoroutineWithData(this, CompareDirectionalInputCoroutine(correctDirectionalInputs, 6));
            yield return _coroutineWithData.MyCoroutine;
            if (!(bool)_coroutineWithData.result)
            {
                yield return null;
                specialMove.checkRunning = false;
                yield break;
            }
            yield return null;
        }
        SpecialMoveActivated = true;
        string[] buttonInputs = specialMove.ButtonInput;
        CoroutineWithData coroutineWithData = new CoroutineWithData(this, CompareButtonInputCoroutine(buttonInputs, 9));
        yield return coroutineWithData.MyCoroutine;
        if (!(bool)coroutineWithData.result)
        {
            yield return null;
            specialMove.checkRunning = false;
            SpecialMoveActivated = false;
            yield break;
        }
        Debug.Log("Special Activated");
        specialMove.SpecialMoveFunction?.Invoke();
        yield return new WaitForSeconds(.1f);
        specialMove.checkRunning = false;
        SpecialMoveActivated = false; // Motion Input Completed Successfully
    }

    /// <summary>
    /// A coroutine which checks if the player's input matches that of the desired input.
    /// </summary>
    /// <param name="inputType"></param>
    /// <param name="_desiredInputs"></param>
    /// <param name="frameCount"></param>
    /// <returns></returns>
    private IEnumerator CompareButtonInputCoroutine(string[] _desiredInputs, int frameCount)
    {
        int framesPassed = 0;
        string currentInput;
        while (framesPassed < frameCount)
        {
            currentInput = currentButtonInput;
            /*if (ArrayUtility.Contains(_desiredInputs, currentInput)) // Correct input
            {
                yield return true;
                yield break;
            }*/
            yield return null;
            framesPassed++;
        }
        yield return false; // No correct input in time
    }

    /// <summary>
    /// A coroutine which checks if the player's input matches that of the desired input.
    /// </summary>
    /// <param name="inputType"></param>
    /// <param name="_desiredInput"></param>
    /// <param name="frameCount"></param>
    /// <returns></returns>
    private IEnumerator CompareDirectionalInputCoroutine(List<int> _desiredInput, int frameCount)
    {
        int framesPassed = 0;
        int currentInput;
        while (framesPassed < frameCount)
        {
            currentInput = currentDirectionalInput;
            if (_desiredInput.Contains(currentInput)) // Correct input
            {
                yield return true;
                yield break;
            }
            yield return null;
            framesPassed++;
        }
        yield return false; // No correct input in time
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player2"))
        {
            collidingWithOpponent = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player2"))
        {
            collidingWithOpponent = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, .25f);
        Gizmos.DrawCube(new Vector3(charRB.position.x + 0.1f, charRB.position.y - 0.1f, 0), new Vector2(0.535f, 1.6f));
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
        ResetAirOptions();
        ResetNormalsUsed();
    }
}