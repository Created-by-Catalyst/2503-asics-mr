using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    public GameObject playerCamera;

    public Transform startingPoint;

    [Tooltip("The root object you move: [BuildingBlock] Camera Rig")]
    public Transform playerRigRoot;

    [Tooltip("The actual headset tracker: CenterEyeAnchor")]
    public Transform centerEyeAnchor;

    [Tooltip("The empty GameObject defining the desired final position and orientation.")]
    public Transform resetTarget;

    private Transform _playerRigTransform;

    public AudioSource audioSource;

    public AudioClip[] sounds;

    private void Awake()
    {
        instance = this;

        _playerRigTransform = playerRigRoot;

        // 2. Locate the CenterEyeAnchor if not set (more reliable if it's the right type)
        if (centerEyeAnchor == null)
        {
            // You can also manually drag the CenterEyeAnchor into the Inspector slot.
            Transform trackingSpace = _playerRigTransform.Find("TrackingSpace");
            if (trackingSpace != null)
            {
                centerEyeAnchor = trackingSpace.Find("CenterEyeAnchor");
            }

            if (centerEyeAnchor == null)
            {
                Debug.LogError("CenterEyeAnchor not found. Please assign it manually.");
            }
        }


        OVRManager.HMDUnmounted += onRemovedHeadset;
    }

    bool[] shoesTouched = new bool[] {false,false,false};

    public GameObject removeHeadsetUI;


    public GameObject instructions;

    public void ShoeInitialGrab(int id)
    {
        if (shoesTouched[id] == false)
        {
            audioSource.clip = sounds[id];
            audioSource.Play();
        }
    }

    public void ShoeTouched(int id)
    {

            shoesTouched[id] = true;

        if(shoesTouched.All(b => b == true))
        {
            instructions.SetActive(false);
            removeHeadsetUI.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        OVRManager.HMDUnmounted -= onRemovedHeadset;
    }

    void onRemovedHeadset()
    {
        SceneManager.LoadScene(0);
    }

    public void ResetVRPosition()
    {
        RecenterPlayer();
    }

    public void RecenterPlayer()
    {
        if (_playerRigTransform == null || centerEyeAnchor == null || resetTarget == null)
        {
            Debug.LogError("Recenter setup is incomplete. Check the Inspector assignments.");
            return;
        }

        // ----------------------------------------------------
        // Step 1: Compensate for current ROTATION (Yaw only)
        // ----------------------------------------------------

        // A. Get the current forward direction (XZ plane only)
        Vector3 currentForward = centerEyeAnchor.forward;
        currentForward.y = 0;
        currentForward.Normalize();

        // B. Get the target forward direction (XZ plane only)
        Vector3 targetForward = resetTarget.forward;
        targetForward.y = 0;
        targetForward.Normalize();

        // C. Calculate the rotation difference needed for the root rig.
        // This is the rotation that maps the current direction to the target direction.
        Quaternion rotationDelta = Quaternion.FromToRotation(currentForward, targetForward);

        // D. Apply the rotation to the root rig.
        _playerRigTransform.rotation *= rotationDelta;


        // ----------------------------------------------------
        // Step 2: Compensate for current POSITION (XZ plane only)
        // ----------------------------------------------------

        // E. The CenterEyeAnchor is now facing the correct direction, but is offset.
        // Calculate the vector needed to move the CenterEyeAnchor to the ResetTarget's position.
        Vector3 positionDelta = resetTarget.position - centerEyeAnchor.position;

        // F. Ignore the Y-axis (vertical) to respect the user's floor/eye-level setting.
        positionDelta.y = 0;

        // G. Apply the position delta to the root rig.
        _playerRigTransform.position += positionDelta;

        Debug.Log("Manually recentered player rig to: " + resetTarget.name);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
