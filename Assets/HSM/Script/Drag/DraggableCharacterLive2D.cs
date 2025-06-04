using Unity.VisualScripting;
using UnityEngine;

public class DraggableCharacterLive2D : DraggableCharacterBase
{
    protected override void Awake()
    {
        dragTarget = transform;
        
        base.Awake();
    }
}
