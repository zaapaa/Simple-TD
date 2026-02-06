using UnityEngine;

public class RotateY : MonoBehaviour
{
    public float rotationSpeed;
    public bool randomDirection = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (randomDirection && UnityEngine.Random.value < 0.5f)
        {
            rotationSpeed *= -1f;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
