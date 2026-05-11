using UnityEngine;
using System.Collections;

public class AutoTarget : MonoBehaviour
{
    [SerializeField] private string _enemyLayerName = "Enemies";
    [SerializeField] private float _interval = 0.1f;
    [SerializeField] private float _range = 10f;

    private bool _active = true;
    [SerializeField]private int _layerMask = 0;

    private static AutoTarget _instance = null;

    [field:SerializeField] public Vector3 _targetPosition { get; private set; }
    public Transform _targetTransform {  get; private set; }
    public float _targetDistance { get; private set; }
    public bool _targetSet {  get; private set; }

    private void Start()
    {
        if (_instance == null) _instance = this;
        else Destroy(this);

        _layerMask = LayerMask.GetMask(_enemyLayerName);
        StartCoroutine(NearestEnemySearch());
    }

    private IEnumerator NearestEnemySearch()
    {
        while (true)
        {
            if (_active)
            {
                Collider[] enemies = Physics.OverlapSphere(transform.position, _range, _layerMask);
                
                if (enemies.Length > 0)
                {
                    Collider nearestEnemyCollider = NearestAmong(enemies, out float sqrDist);
                    SetTarget(nearestEnemyCollider.transform, sqrDist);
                    _targetSet = true;
                }
                else
                {
                    SetTarget(null, 0f);
                    _targetSet = false;
                }
            }
            yield return new WaitForSeconds(_interval);
        }
    }

    public void Toggle(bool run)
    {
        _active = run;
    }

    private Collider NearestAmong(Collider[] enemyColliders, out float sqrDistance)
    {
        sqrDistance = _range * _range;
        Vector3 position = transform.position;
        Collider result = null;

        foreach (Collider collider in enemyColliders)
        {
            Vector3 point = collider.ClosestPoint(position);
            float sqrMagnitude = (position - point).sqrMagnitude;

            if (sqrDistance > sqrMagnitude)
            {
                sqrDistance = sqrMagnitude;
                result = collider;
            }
        }

        return result;
    }

    private void SetTarget(Transform targetTransform, float sqrTargetDistance)
    {
        _targetTransform = targetTransform;
        _targetPosition = (targetTransform == null)? transform.position : targetTransform.position;
        _targetDistance = Mathf.Sqrt(sqrTargetDistance);
    }
}
