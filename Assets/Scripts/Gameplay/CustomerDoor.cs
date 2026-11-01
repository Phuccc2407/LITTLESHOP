using System.Collections;
using UnityEngine;

public sealed class CustomerDoor : MonoBehaviour
{
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 openEulerAngles = new Vector3(0f, 90f, 0f);
    [SerializeField, Min(0.05f)] private float rotationDuration = 0.5f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine rotationRoutine;
    private int customersInside;

    private void Awake()
    {
        if (doorTransform == null)
        {
            doorTransform = transform;
        }

        closedRotation = doorTransform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(openEulerAngles);
    }

    public void OpenForEntry()
    {
        customersInside++;
        RotateTo(openRotation);
    }

    public void CloseAfterExit()
    {
        customersInside = Mathf.Max(0, customersInside - 1);
        if (customersInside > 0)
        {
            return;
        }

        RotateTo(closedRotation);
    }

    private void RotateTo(Quaternion target)
    {
        if (rotationRoutine != null)
        {
            StopCoroutine(rotationRoutine);
        }

        rotationRoutine = StartCoroutine(RotateRoutine(target));
    }

    private IEnumerator RotateRoutine(Quaternion target)
    {
        Quaternion start = doorTransform.localRotation;
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / rotationDuration);
            doorTransform.localRotation = Quaternion.Slerp(start, target, progress);
            yield return null;
        }

        doorTransform.localRotation = target;
        rotationRoutine = null;
    }
}
