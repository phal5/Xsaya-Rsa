using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class Flipbook : MonoBehaviour
{
#nullable enable
    [SerializeField] GameObject? _attack;
    [SerializeField] GameObject? _run;
    [SerializeField] GameObject? _idle;

    [SerializeField] GameObject? _jump;
    [SerializeField] GameObject? _fall;

    [SerializeField] GameObject? _stunned;
    [SerializeField] GameObject? _dead;
    [Space(10f)]
    [SerializeField] PlayerInput _input;
    [SerializeField] Rigidbody _rb;
    [SerializeField] FiniteStateMachine _fsm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputManager.instance.move_jump.action.performed += jump => EnableOnly(_jump);
        InputManager.instance.move_skill.action.performed += attack => EnableOnly(_attack);
        InputManager.instance.move_move.action.performed += move => EnableOnly(_run);
    }

    // Update is called once per frame
    void Update()
    {
        if(_fsm._currentStateType == typeof(Character_Airborne) && _rb.linearVelocity.y < 0) EnableOnly(_fall);
        else
        {
            bool hasInput = _input.currentActionMap.actions.Any(action => action.IsPressed() || action.triggered);
            if (!hasInput)
            {
                EnableOnly(_idle);
            }
        }
    }

    void EnableOnly(GameObject? pose)
    {
        print(pose?.name);
        Activate(_attack, false);
        Activate(_run, false);
        Activate(_jump, false);
        Activate(_idle, false);
        Activate(_fall, false);
        Activate(_stunned, false);
        Activate(_dead, false);

        Activate(pose, true);
    }

    void Activate(GameObject? gameObject, bool activate)
    {
        if (gameObject == null) return;
        gameObject.SetActive(activate);
    }
}
