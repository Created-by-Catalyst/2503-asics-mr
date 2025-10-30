using UnityEngine;

public class ShoeManager : MonoBehaviour
{

    bool resettingLocation = false;

    public void LetGo()
    {
        resettingLocation = true;
    }

    void ResetPosition()
    {

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    float lerpSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        if (resettingLocation)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                Vector3.zero,
                Time.deltaTime * lerpSpeed
            );

            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                Quaternion.identity,
                Time.deltaTime * lerpSpeed
            );


            if (Vector3.Distance(transform.localPosition, Vector3.zero) < 0.01f && Quaternion.Angle(transform.localRotation, Quaternion.identity) < 0.01f)
            {
                // Snap it to the final spot for perfect accuracy
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                // Stop the movement
                resettingLocation = false;
            }
        }

    }
}
