using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [HideInInspector]
    public Transform player;
    float rotationSpeed = 5f;


    private void Start()
    {
        player = GameManager.instance.player.transform;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (player != null)
        {
            // Get target position at the same height (ignore vertical)
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y, player.position.z);

            // Compute direction
            Vector3 direction = (targetPosition - transform.position).normalized;

            if (direction.sqrMagnitude > 0.001f)
            {
                // Calculate target rotation only on Y axis
                //-direction to invert it
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Optional: Smooth rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

                // Or instant rotation:
                // transform.rotation = targetRotation;
            }
        }
    }
}
