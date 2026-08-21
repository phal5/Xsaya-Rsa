using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    //Move
    [field: SerializeField] public InputActionReference move_move;
    [field: SerializeField] public InputActionReference move_look;
    [field: SerializeField] public InputActionReference move_jump;
    [field: SerializeField] public InputActionReference move_dash;
    [field: SerializeField] public InputActionReference move_skill;
    [field: SerializeField] public InputActionReference move_interact;
    [field: SerializeField] public InputActionReference move_crouch;
    [Tooltip("턱을 타고 올라가는 키. W / ↑ 에 걸려 있다.")]
    [field: SerializeField] public InputActionReference move_climbUp;
    [Tooltip("턱을 타고 내려가거나 놓는 키. ↓ 에 걸려 있다.")]
    [field: SerializeField] public InputActionReference move_climbDown;

    public static Vector3 CharacterMove { get; private set; }
    public static Vector2 CharacterLook { get; private set; }
    public static bool CharacterJump;
    public static bool CharacterDash;
    public static bool CharacterSkill;
    public static bool CharacterInteract;
    public static bool CharacterCrouch;


    public static InputManager instance;

    private void Awake()
    {
        if(instance == null) { instance = this; }
        else
        {
            Debug.LogError($"Multiple instances of Inputmanager detected. Deleting one under {gameObject.name}...");
            Debug.LogError($"Original Instance instanced under {instance.gameObject.name}.");
            Destroy(this);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        move_jump.action.performed += jumpTrue => Set(out CharacterJump, true);
        move_jump.action.canceled += jumpFalse => Set(out CharacterJump, false);

        move_dash.action.performed += dashTrue => Set(out CharacterDash, true);
        move_dash.action.canceled += dashFalse => Set(out CharacterDash, false);

        move_skill.action.performed += skillTrue => Set(out CharacterSkill, true);
        move_skill.action.canceled += skillFalse => Set(out CharacterSkill, false);

        move_interact.action.performed += interactTrue => Set(out CharacterInteract, true);
        move_interact.action.canceled += interactFalse => Set(out CharacterInteract, false);
    }

    // Update is called once per frame
    void Update()
    {
        CharacterMove = ToHorizontal(move_move.action.ReadValue<Vector2>());
        CharacterLook = move_look.action.ReadValue<Vector2>();
    }

    private void LateUpdate()
    {
        
    }

    private void Set<T>(out T value, T to)
    {
        value = to;
    }

    public void Rewire()
    {

    }

    private static Vector3 ToHorizontal(Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }
}
