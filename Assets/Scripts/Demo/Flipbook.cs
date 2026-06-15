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
    [SerializeField] GameObject? _land;

    [SerializeField] GameObject? _stunned;
    [SerializeField] GameObject? _dead;
    [Space(10f)]
    [SerializeField] PlayerInput _input;
    [SerializeField] Rigidbody _rb;
    [SerializeField] FiniteStateMachine _fsm;
    [SerializeField] Collider _weapon;
    [SerializeField] DamagableBase _damagable;

    float _hp = 0;

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
        if (_weapon.enabled)
        {
            EnableOnly(_attack);
            return;
        }
        if (_damagable.CurrentHealth < _hp)
        {
            if(_damagable.CurrentHealth == 0)
            {
                EnableOnly(_dead);
                return;
            }
            else
            {
                EnableOnly(_stunned);
                return;
            }
        }
        if (_fsm._currentStateType == typeof(Character_Airborne))
        {
            if (_rb.linearVelocity.y < 0) EnableOnly(_land);
            else EnableOnly(_jump);
            return;
        }
        if(_fsm._currentStateType == typeof(Character_Ground))
        {
            if (_rb.linearVelocity.sqrMagnitude > 1) EnableOnly(_run);
            else EnableOnly(_idle);
            return;
        }

        _hp = _damagable.CurrentHealth;
    }

    void Activate(GameObject? gameObject, bool activate)
    {
        if (gameObject == null) return;
        gameObject.SetActive(activate);
    }

    void EnableOnly(GameObject? pose)
    {
        Activate(_attack, false);
        Activate(_run, false);
        Activate(_jump, false);
        Activate(_idle, false);
        Activate(_land, false);
        Activate(_stunned, false);
        Activate(_dead, false);

        Activate(pose, true);
    }

    public void SetAttack() { EnableOnly(_attack); }

    public void SetStunned() { EnableOnly(_stunned); }

    public void SetDead() { EnableOnly(_dead); }

    public void SetBase() { EnableOnly(_idle); }
}
