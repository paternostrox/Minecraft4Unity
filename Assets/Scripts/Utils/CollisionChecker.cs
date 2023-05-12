using UnityEngine;
using System.Collections;

public class CollisionChecker : MonoBehaviour
{
    public delegate void CheckEndHandler(bool isOverlapping);
    public event CheckEndHandler CheckEnded;

    bool isColliding = false;

    private void Awake()
    {
        StartCoroutine(CheckOnNextFrame());
    }

    IEnumerator CheckOnNextFrame()
    {
        yield return null;
        yield return null; // waits two frames

        CheckEnded?.Invoke(isColliding);

        print("IS COLLIDING = " + isColliding);

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        isColliding = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        isColliding = true;
    }

    private void OnTriggerStay(Collider other)
    {
        isColliding = true;
    }
}
