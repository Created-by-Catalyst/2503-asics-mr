using UnityEngine;


public class TransformFollow : MonoBehaviour
{

    [SerializeField]
    Transform point;


    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, point.position, Time.deltaTime * 1);
        transform.rotation = Quaternion.Lerp(transform.rotation, point.rotation, Time.deltaTime * 1);
    }

}

