using UnityEngine;

public class DraggableCharacterVRM : DraggableCharacterBase
{
    public Transform target;
    protected override void Awake()
    {
        dragTarget = target != null ? target : transform;

        base.Awake();
    }
}
