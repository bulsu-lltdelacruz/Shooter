using UnityEngine;

public class FinalDoor : Interactable
{
    [SerializeField] Vector3 direction;
    [SerializeField] float speed;
    [SerializeField] Transform targetTransform;
    [SerializeField] AudioClip sound;
    float deltaPosition;

    float targetDistance;
    float localHeight;
    bool isOpening;
    bool isOpened;
    void Start()
    {
        interactableType = ItemType.Door;
        MeshFilter meshFilter = targetTransform.GetComponent<MeshFilter>();

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Vector3 localSize = meshFilter.sharedMesh.bounds.size;
            targetDistance = localSize.x * transform.lossyScale.x;
        }
    }
    public void OpenDoor()
    {
        isOpening = true;
        SoundFXManager.instance.PlaySoundFXClip(sound, transform, 1, SoundType.SOUNDFX);
    }
    private void Update()
    {
        if (isOpened)
            return;
        if (isOpening)
        {
            deltaPosition += speed * Time.deltaTime;
            targetTransform.position += direction * speed * Time.deltaTime;
            if (deltaPosition >= targetDistance)
                isOpened = true;
        }
    }
}
