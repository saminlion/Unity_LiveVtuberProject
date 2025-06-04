using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class DraggableCharacterBase : MonoBehaviour
{
    public string userId;
    protected Transform dragTarget; // 실질적으로 움직일 오브젝트 (보통 Root 또는 parent)
    private bool isDragging = false;
    private float zOffset;
    private Vector3 dragOffset;

    protected virtual void Awake()
    {
        if (dragTarget == null)
            dragTarget = transform;
    }

    void OnMouseDown()
    {
        isDragging = true;

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(dragTarget.position);
        zOffset = screenPoint.z;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zOffset));
        dragOffset = dragTarget.position - mouseWorld;
    }

    void OnMouseUp()
    {
        isDragging = false;
        CharacterPositionStorage.SavePosition(userId, transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDragging || dragTarget == null) return;

        Vector3 mouseScreen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zOffset);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        dragTarget.position = new Vector3(mouseWorld.x + dragOffset.x, mouseWorld.y + dragOffset.y, dragTarget.position.z);
    }
}
