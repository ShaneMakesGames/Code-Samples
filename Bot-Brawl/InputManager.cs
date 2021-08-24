using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEditor;
using System;

public class InputManager : MonoBehaviour
{

    #region Variables

    private Gamepad gamepad; 
    private PlayerControls controls;
    [Header("Input")]
    public TextMeshProUGUI inputText;
    public TextMeshProUGUI xInputText;
    public TextMeshProUGUI yInputText;
    public string currentDirectionalInput; // The direction the player is holding on their thumbstick
    public string currentButtonInput; // The button the player pushed
    [Header("Character Data")] // Used to import character specific data
    public CharacterData characterData;
    [Header("Normals")] // Lists of the character's normal attacks
    private List<string> AllNormalNames = new List<string>();
    private List<NormalAttack> AllNormals = new List<NormalAttack>();
    public List<NormalAttack> LightNormals = new List<NormalAttack>();
    public List<NormalAttack> MediumNormals = new List<NormalAttack>();
    public List<NormalAttack> HeavyNormals = new List<NormalAttack>();
    [Header("Uniques")]
    public List<UniqueMove> Uniques = new List<UniqueMove>();
    [Header("Specials")]
    public List<SpecialMove> Specials = new List<SpecialMove>();
    [Header("State Info")]
    public bool isAttacking;
    public bool isRunning;
    public bool SpecialMoveActivated; // The motion for a special move has been completed
    public bool UniqueActivated;
    public bool inHitstun;
    private bool normalChained; // Used to chain normal attacks into other normal attacks
    private bool uniqueChained; // Used to chain normal attacks into unique

    [Header("Air")]
    public float yVelocity;
    public bool isJumping;
    public bool inAir;
    public bool isFalling;

    [Header("Transform")]
    public Rigidbody2D charRB;
    private float xVector, yVector;
    private Vector2 moveVector;
    private bool collidingWithOpponent;
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

    private NormalAttack currentNormal;

    [Header("Animation")]
    public Animator anim;

    public GameObject hitboxPrefab;
    private Keyboard keyboard;

    #endregion

    void Awake()
    {
        keyboard = Keyboard.current;
        gamepad = Gamepad.current;
        QualitySettings.vSyncCount = 0;  // Disables VSync
        Application.targetFrameRate = 60; // Sets target frame rate to 60 fps

        controls = new PlayerControls();
        controls.Gameplay.Movement.performed += ctx => CheckInputDirection();
        //controls.Gameplay.KeyboardMovement.started += ctx => KeyboardCheckInputDirection();
        controls.Gameplay.Button.performed += ctx => CheckButtonInput();

        AddCharacterData(characterData);

        Specials[0].SpecialMoveFunction += DPTest;
        Specials[1].SpecialMoveFunction += SpecialMoveTest;
        Uniques[2].UniqueMoveFunction += SquallDash;
    }

    /// <summary>
    /// Adds character specific data as variables 
    /// </summary>
    /// <param name="charData"></param>
    private void AddCharacterData(CharacterData charData)
    {
        LightNormals = charData.LightNormals;
        AddNormalsToList(LightNormals);
        MediumNormals = charData.MediumNormals;
        AddNormalsToList(MediumNormals);
        HeavyNormals = charData.HeavyNormals;
        AddNormalsToList(HeavyNormals);
        Uniques = charData.Uniques;
        Specials = charData.Specials;
    }

    private void AddNormalsToList(List<NormalAttack> normals)
    {
        for (int i = 0; i < normals.Count; i++)
        {
            AllNormals.Add(normals[i]);
            AllNormalNames.Add(normals[i].MoveName);
        }
    }

    private void Update()
    {
        KeyboardCheckInputDirection();
        //KeyboardCheckForNoInput();
        //CheckInputDirection();
        //CheckButtonInput();

        Movement();
    }

    private void Movement()
    {
        transform.position = new Vector2(charRB.position.x, charRB.position.y - 1f);

        if (!isAttacking && !SpecialMoveActivated && !UniqueActivated)
        {
            Vector2 moveDir = new Vector2(xVector, yVector);

            if (collidingWithOpponent && currentDirectionalInput == "Forward")
            {
                moveDir.x *= movementMultiplier;
            }

            //charRB.MovePosition(charRB.position + moveDir * Time.deltaTime);
            charRB.position += moveDir;
        }
    }

    private void SpecialMoveTest()
    {
        Debug.Log("Quarter Circle Forward Completed");
    }

    private void DPTest()
    {
        Debug.Log("Dragon Punch Completed");
    }

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

    /// <summary>
    /// Converts the vector2 value into a string (Ex. “DownForward”)
    /// </summary>
    /// <param name="vector2"></param>
    private void CalculateInputDirection(Vector2 vector2)
    {
        xVector = vector2.x * 0.02f;

        string xString = XDirection(vector2.x);
        string yString = YDirection(vector2.y);

        currentDirectionalInput = GetInputDirection(xString, yString);
        inputText.text = currentDirectionalInput;
        xInputText.text = "X Input : " + vector2.x;
        yInputText.text = "Y Input : " + vector2.y;

        bool canSpecial = false;
        if (isAttacking)
        {
            if (currentNormal.specialCancelable)
            {
                canSpecial = true;
            }
        }
        else
        {
            canSpecial = true;
        }

        switch (currentDirectionalInput)
        {
            case "Forward":
                if (canSpecial)
                {
                    TryCheckForMotionInput(Specials[0]);
                }
                break;
            case "Back":
                break;
            case "Down":
                if (canSpecial)
                {
                    TryCheckForMotionInput(Specials[1]);
                }
                break;
            case "Neutral":
                break;
        }

        if (currentDirectionalInput.Contains("Up") && !isJumping)
        {
            StartCoroutine(JumpCoroutine());
        }
    }

    private IEnumerator JumpCoroutine()
    {
        isJumping = true;
        int framesPassed = 0;
        float jumpAmount = 0.85f / 10;
        while (framesPassed < 10)
        {
            yield return null;
            yVector = jumpAmount;
            framesPassed++;
        }
        jumpAmount = 0.15f / 10;
        while (framesPassed < 20) // Start Up
        {
            yield return null;
            yVector = jumpAmount;
            framesPassed++;
        }
        yield return null;
        framesPassed = 0;
        while (charRB.position.y > 1.001f)
        {
            yield return null;
            yVector = -.042f;
            framesPassed++;
        }
        Debug.Log(20 + framesPassed);
        charRB.position = new Vector2(charRB.position.x, 1);
        isFalling = true;
        isJumping = false;
        yVector = 0;
    }

    /// <summary>
    /// Converts the float value of the x direction into the appropriate string
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    private string XDirection(float f)
    {
        if (-0.5f < f && f < 0.5f)
        {
            return "Neutral";
        }
        else if (f <= -0.5f)
        {
            return "Back";
        }
        else if (f >= 0.5f)
        {
            return "Forward";
        }
        else
        {
            return "Error";
        }
    }

    /// <summary>
    /// Converts the float value of the y direction into the appropriate string
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    private string YDirection(float f)
    {
        if (-0.5f < f && f < 0.5f)
        {
            return "Neutral";
        }
        else if (f <= -0.5f)
        {
            return "Down";
        }
        else if (f >= 0.5f)
        {
            return "Up";
        }
        else
        {
            return "Error";
        }
    }

    /// <summary>
    /// Returns the properly formatted input direction (Ex. DownBack)
    /// </summary>
    /// <param name="xString"></param>
    /// <param name="yString"></param>
    /// <returns></returns>
    private string GetInputDirection(string xString, string yString)
    {
        if (xString == "Neutral" && yString == "Neutral") // Both x & y direction is neutral
        {
            return "Neutral";
        }
        else if (xString == "Neutral" && yString != "Neutral") // If x direction is neutral, only use y direction (Ex. Down)
        {
            return yString;
        }
        else if (yString == "Neutral" && xString != "Neutral") // If y direction is neutral, only use x direction (Ex. Forward)
        {
            return xString;
        }
        else // If neither x or y are neutral, use both directions (Ex. DownForward)
        {
            return yString + xString;
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
        if (!checkButtonRunning)
        {
            string buttonInput = "";
            if (gamepad != null)
            {
                if (gamepad.buttonSouth.isPressed)
                {
                    buttonInput = "Unique";
                }
                else if (gamepad.buttonWest.isPressed)
                {
                    buttonInput = "Light";
                }
                else if (gamepad.buttonNorth.isPressed)
                {
                    buttonInput = "Medium";
                }
                else if (gamepad.buttonEast.isPressed)
                {
                    buttonInput = "Heavy";
                }
                else if (gamepad.rightTrigger.isPressed)
                {
                    buttonInput = "Dash";
                }
            }
            else if (keyboard != null)
            {
                if (keyboard.jKey.isPressed)
                {
                    buttonInput = "Light";
                }
                else if (keyboard.iKey.isPressed)
                {
                    buttonInput = "Medium";
                }
                else if (keyboard.kKey.isPressed)
                {
                    buttonInput = "Heavy";
                }
                else if (keyboard.spaceKey.isPressed)
                {
                    buttonInput = "Unique";
                }
            }
            StartCoroutine(DisplayButtonInputCoroutine(buttonInput));
        }
    }

    private IEnumerator DisplayButtonInputCoroutine(string _buttonInput)
    {
        currentButtonInput = _buttonInput;
        checkButtonRunning = true;
        if (_buttonInput == "Light" || _buttonInput == "Medium" || _buttonInput == "Heavy")
        {
            AttemptNormalAttack();
        }
        else if (_buttonInput == "Unique")
        {
            AttemptUnique();
        }

        yield return new WaitForEndOfFrame();
        currentButtonInput = null;
        checkButtonRunning = false;
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
        }
    }

    /// <summary>
    /// Starts a normal attack if it can be used in the current situation
    /// </summary>
    private void AttemptNormalAttack()
    {
        if (!SpecialMoveActivated && !UniqueActivated)
        {
            if (!isAttacking) // Normal attack from neutral
            {
                NormalAttack normal = GetNormalFromInput(currentButtonInput);
                if (normal != null)
                {
                    NormalAttackStartUp(normal);
                }
            }
            else if (!normalChained)
            {
                if (normalAttackState != NormalAttackState.Recovery) // Buffer chain if not in recovery
                {
                    NormalAttack normal = GetChainableNormal(currentButtonInput); // Chain normal immediately if in recovery
                    if (normal != null)
                    {
                        //Debug.Log("Chained Buffered");
                        StartCoroutine(BufferChainCoroutine(normal));
                    }
                }
                else
                {
                    NormalAttack normal = GetChainableNormal(currentButtonInput); // Chain normal immediately if in recovery
                    if (normal != null)
                    {
                        //Debug.Log("Chained In Recovery");
                        NormalAttackStartUp(normal);
                    }
                }
            }
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
                NormalAttackStartUp(normal);
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Returns a normal attack if it is able to be chained into, otherwise returns null
    /// </summary>
    /// <param name="_input"></param>
    /// <returns></returns>
    private NormalAttack GetChainableNormal(string _input)
    {
        switch (_input)
        {
            case "Light":
                for (int i = 0; i < LightNormals.Count; i++)
                {
                    string[] correctDirectionalInputs = LightNormals[i].DirectionalInput.Split('/');
                    if (ArrayUtility.Contains(correctDirectionalInputs, currentDirectionalInput))
                    {
                        if (currentNormal.ChainableNormals.Contains(LightNormals[i].MoveName))
                        {
                            return LightNormals[i];
                        }
                        break;
                    }
                }
                return null;
            case "Medium":
                for (int i = 0; i < MediumNormals.Count; i++)
                {
                    string[] correctDirectionalInputs = MediumNormals[i].DirectionalInput.Split('/');
                    if (ArrayUtility.Contains(correctDirectionalInputs, currentDirectionalInput))
                    {
                        if (currentNormal.ChainableNormals.Contains(MediumNormals[i].MoveName))
                        {
                            return MediumNormals[i];
                        }
                        break;
                    }
                }
                return null;
            case "Heavy":
                for (int i = 0; i < HeavyNormals.Count; i++)
                {
                    string[] correctDirectionalInputs = HeavyNormals[i].DirectionalInput.Split('/');
                    if (ArrayUtility.Contains(correctDirectionalInputs, currentDirectionalInput))
                    {
                        if (currentNormal.ChainableNormals.Contains(HeavyNormals[i].MoveName))
                        {
                            return HeavyNormals[i];
                        }
                        break;
                    }
                }
                return null;
        }
        return null;
    }

    /// <summary>
    /// Returns a normal depending on the directional & button input
    /// </summary>
    /// <param name="_input"></param>
    /// <returns></returns>
    private NormalAttack GetNormalFromInput(string _input)
    {
        switch (_input)
        {
            case "Light":
                for (int i = 0; i < LightNormals.Count; i++)
                {
                    string[] correctDirectionalInputs = LightNormals[i].DirectionalInput.Split('/');
                    if (ArrayUtility.Contains(correctDirectionalInputs, currentDirectionalInput))
                    {
                        return LightNormals[i];
                    }
                }
                break;
            case "Medium":
                for (int i = 0; i < MediumNormals.Count; i++)
                {
                    string[] correctDirectionalInputs = MediumNormals[i].DirectionalInput.Split('/');
                    if (ArrayUtility.Contains(correctDirectionalInputs, currentDirectionalInput))
                    {
                        return MediumNormals[i];
                    }
                }
                break;
            case "Heavy":
                for (int i = 0; i < HeavyNormals.Count; i++)
                {
                    string[] correctDirectionalInputs = HeavyNormals[i].DirectionalInput.Split('/');
                    if (ArrayUtility.Contains(correctDirectionalInputs, currentDirectionalInput))
                    {
                        return HeavyNormals[i];
                    }
                }
                break;
        }
        return null;
    }

    private Coroutine normalCoroutine;
    /// <summary>
    /// Plays an animation and triggers the normal attack’s startup
    /// </summary>
    /// <param name="normal"></param>
    private void NormalAttackStartUp(NormalAttack normal)
    {
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
            GameObject obj = Instantiate(hitboxPrefab, new Vector2(charRB.position.x, charRB.position.y - 1), Quaternion.identity);
            HitboxTest hitbox = obj.GetComponent<HitboxTest>();
            if (!isAttacking) // Normal out of neutral
            {
                normalCoroutine = StartCoroutine(NormalFramesCoroutine(hitbox, normal));
            }
            else // Chaining a normal
            {
                StopCoroutine(normalCoroutine);
                normalCoroutine = StartCoroutine(NormalFramesCoroutine(hitbox, normal));
            }
        }
        else
        {
            Debug.Log("Run out of uses");
        }

    }

    public bool attackConnected;
    /// <summary>
    /// Cycles through the different states of a normal attack (startup, active, & recovery)
    /// </summary>
    /// <param name="hitbox"></param>
    /// <param name="normal"></param>
    /// <returns></returns>
    private IEnumerator NormalFramesCoroutine(HitboxTest hitbox, NormalAttack normal)
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
        hitbox.HitboxSetup(normal, this);
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
        normalAttackState = NormalAttackState.Recovery;
        framesPassed = 0;
        hitbox.isActive = false;
        normalChained = false;
        while (framesPassed < normal.RecoveryFrames) // RecoveryFrames
        {
            yield return null;
            framesPassed++;
            totalFrames++;
        }
        isAttacking = false;
        totalFrames = 0;
        currentNormal = null;
        //Debug.Log("Recovery Complete");
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
            //Debug.Log("Active Complete");
        }
        attackConnected = false;
        if (normal.isMultiHitting) // Multi-hitting normal
        {
            if (AllNormalNames.Contains(normal.multiHitNormalName))
            {
                int index = AllNormalNames.IndexOf(normal.multiHitNormalName);
                NormalAttackStartUp(AllNormals[index]);
            }
            yield break;
        }
        else
        {
            normalAttackState = NormalAttackState.Recovery;
            framesPassed = 0;
            normalChained = false;
            while (framesPassed < normal.RecoveryFrames) // RecoveryFrames
            {
                yield return null;
                framesPassed++;
                totalFrames++;
            }
            isAttacking = false;
            totalFrames = 0;
            currentNormal = null;
        }
    }

    private IEnumerator FrameCounterCoroutine()
    {
        int _totalFrames = 0;
        while (enabled)
        {
            yield return null;
            _totalFrames++;
            Debug.Log(_totalFrames);
        }
    }

    #endregion

    #region Uniques

    /// <summary>
    /// Starts a unique move if it can be used in the current situation
    /// </summary>
    private void AttemptUnique()
    {
        string d = currentDirectionalInput;
        if (!isAttacking)
        {
            for (int i = 0; i < Uniques.Count; i++)
            {
                string[] correctDirectionalInputs = Uniques[i].DirectionalInput.Split('/');
                if (ArrayUtility.Contains(correctDirectionalInputs, currentDirectionalInput))
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

    public bool isDashing;
    private void SquallDash()
    {
        if (!Uniques[2].active)
        {
            StartCoroutine(SquallDashCoroutine(Uniques[2]));
        }
    }

    private IEnumerator SquallDashCoroutine(UniqueMove unique)
    {
        isDashing = true;
        unique.active = true;
        StartCoroutine(CheckForFollowUpAttackCoroutine(Uniques[2]));
        int framesPassed = 0;
        while (framesPassed < 5)
        {
            yield return null;
            charRB.MovePosition(new Vector2(charRB.position.x + .08f, charRB.position.y));
            framesPassed++;
        }
        while (framesPassed < 20) 
        {
            if (unique.followUpActive) // Stops Unique Move if Follow Up is Used
            {
                unique.followUpActive = false;
                yield break;
            }
            yield return null;
            charRB.MovePosition(new Vector2(charRB.position.x + .04f, charRB.position.y));
            framesPassed++;
        }
        unique.active = false;
        UniqueActivated = false;
        unique.followUpActive = false;
        isDashing = false;
    }

    /// <summary>
    /// Checks for a certain input to trigger a follow up attack while the unique move is still active
    /// </summary>
    /// <param name="unique"></param>
    /// <returns></returns>
    private IEnumerator CheckForFollowUpAttackCoroutine(UniqueMove unique)
    {
        while (unique.active)
        {
            for (int i = 0; i < unique.FollowUpAttacks.Count; i++)
            {
                string[] correctButtonInputs = unique.FollowUpAttacks[i].ButtonInput.Split('/');
                if (ArrayUtility.Contains(correctButtonInputs, currentButtonInput))
                {
                    Debug.Log("Rekka");
                    unique.followUpActive = true;
                    unique.active = false;
                    UniqueActivated = false;
                    isDashing = false;
                    NormalAttackStartUp(unique.FollowUpAttacks[i]);
                }
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
    private void TryCheckForMotionInput(SpecialMove specialMove)
    {
        if (!specialMove.checkRunning)
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
            string[] correctInputs = specialMove.MotionInput[i].Split('/');
            CoroutineWithData _coroutineWithData = new CoroutineWithData(this, CompareInputCoroutine("Direction", correctInputs, 6));
            yield return _coroutineWithData.MyCoroutine;
            if (!(bool)_coroutineWithData.result)
            {
                specialMove.checkRunning = false;
                yield break;
            }
            yield return null;
        }
        SpecialMoveActivated = true;
        string[] buttonInputs = specialMove.ButtonInput;
        CoroutineWithData coroutineWithData = new CoroutineWithData(this, CompareInputCoroutine("Button", buttonInputs, 6));
        yield return coroutineWithData.MyCoroutine;
        if (!(bool)coroutineWithData.result)
        {
            specialMove.checkRunning = false;
            SpecialMoveActivated = false;
            yield break;
        }
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
    private IEnumerator CompareInputCoroutine(string inputType, string[] _desiredInputs, int frameCount)
    {
        int framesPassed = 0;
        string currentInput = "";
        while (framesPassed < frameCount)
        {
            switch (inputType)
            {
                case "Direction":
                    currentInput = currentDirectionalInput;
                    break;
                case "Button":
                    currentInput = currentButtonInput;
                    break;
            }
            if (ArrayUtility.Contains(_desiredInputs, currentInput)) // Correct input
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
        Gizmos.DrawCube(new Vector3(charRB.position.x + 0.1f, charRB.position.y - 0.3f, 0), new Vector2(0.6f, 1.25f));
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void OnDestroy()
    {
        Specials[0].SpecialMoveFunction -= DPTest;
        Specials[1].SpecialMoveFunction -= SpecialMoveTest;
        Uniques[2].UniqueMoveFunction -= SquallDash;
    }
}