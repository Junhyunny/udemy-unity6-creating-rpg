
using UnityEngine;

public class EntityAnimationTriggers : MonoBehaviour
{
    private Entity entity;

    public void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }

    public void CurrentStateTrigger()
    {
        entity.CurrentStateAnimationTrigger();
    }
}