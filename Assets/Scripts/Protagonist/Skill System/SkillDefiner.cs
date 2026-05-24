using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillDefiner : MonoBehaviour
{
    [SerializeField] List<ComboEvent> combos;
    
    ComboEvaluator evaluator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        evaluator = new(combos);
    }

    public void Evaluate(List<Key> keys)
    {
        evaluator.EvaluateAndInvoke(keys);
    }
}
