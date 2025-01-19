using UnityEngine;
using TMPro;

public class ObjectInteraction : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Transform holdPos;
    [SerializeField] private GameObject rightArmModel;
    [SerializeField] private Vector3 paintballGunPos;
    [SerializeField] private TextMeshProUGUI interactionText;

    [SerializeField] private float throwForce = 500f;
    [SerializeField] private float pickUpRange = 5f;

    private GameObject heldObj;
    private Rigidbody heldObjRb;
    private bool canDrop = true;
    private bool holdingPaintGun = false;

    void Start()
    {
        // Vérification des références requises
        if (holdPos == null)
        {
            Debug.LogError("HoldPos is not assigned in the inspector!");
            enabled = false; // Désactive le script si holdPos n'est pas assigné
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player is not assigned in the inspector!");
            enabled = false;
            return;
        }

        // Cacher le texte d'interaction au démarrage
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (heldObj == null)
        {
            CheckForPickupObject();
        }
        else
        {
            UpdateHeldObjectUI();

            if (Input.GetKeyDown(KeyCode.E) && canDrop)
            {
                StopClipping();
                DropObject();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (holdingPaintGun)
                {
                    PaintGun paintGun = heldObj.GetComponent<PaintGun>();
                    if (paintGun != null)
                    {
                        paintGun.Shoot();
                    }
                }
                else
                {
                    StopClipping();
                    ThrowObject();
                }
            }

            if (heldObj != null && holdPos != null)
            {
                MoveObject();
            }
        }
    }

    void CheckForPickupObject()
    {
        if (holdPos == null) return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
        {
            if (hit.transform.gameObject.CompareTag("canPickUp"))
            {
                ShowInteractionText("Press E to interact");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    bool isPaintGun = hit.transform.GetComponent<PaintGun>() != null;

                    if (isPaintGun)
                    {
                        if (rightArmModel != null)
                            rightArmModel.SetActive(false);
                        holdingPaintGun = true;
                    }

                    PickUpObject(hit.transform.gameObject);
                }
            }
            else
            {
                HideInteractionText();
            }
        }
        else
        {
            HideInteractionText();
        }
    }

    void UpdateHeldObjectUI()
    {
        if (holdingPaintGun)
        {
            ShowInteractionText("Left Click to shoot");
        }
        else
        {
            ShowInteractionText("Left Click to throw");
        }
    }

    void ShowInteractionText(string message)
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(true);
            interactionText.text = message;
        }
    }

    void HideInteractionText()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj == null || holdPos == null) return;

        Rigidbody rb = pickUpObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            heldObj = pickUpObj;
            heldObjRb = rb;
            heldObjRb.isKinematic = true;
            heldObjRb.transform.parent = holdPos;

            IgnoreCollisionsWithPlayer(true);

            if (holdingPaintGun)
            {
                heldObj.transform.localRotation = Quaternion.identity;
                heldObj.transform.localPosition = paintballGunPos;
            }
        }
    }

    void DropObject()
    {
        if (heldObj == null) return;

        IgnoreCollisionsWithPlayer(false);
        heldObj.layer = 0;

        if (heldObjRb != null)
        {
            heldObjRb.isKinematic = false;
            heldObj.transform.parent = null;
        }

        if (holdingPaintGun && rightArmModel != null)
        {
            rightArmModel.SetActive(true);
            holdingPaintGun = false;
        }

        heldObj = null;
        heldObjRb = null;
    }

    void MoveObject()
    {
        if (heldObj == null || holdPos == null) return;

        if (!holdingPaintGun)
        {
            heldObj.transform.position = holdPos.position;
        }
    }

    void ThrowObject()
    {
        if (heldObj == null || heldObjRb == null) return;

        IgnoreCollisionsWithPlayer(false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObjRb.AddForce(transform.forward * throwForce);
        heldObj = null;
        heldObjRb = null;
    }

    void StopClipping()
    {
        if (heldObj == null) return;

        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);

        if (hits.Length > 1)
        {
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f);
        }
    }

    void IgnoreCollisionsWithPlayer(bool ignore)
    {
        if (heldObj == null || player == null) return;

        Collider[] playerColliders = player.GetComponents<Collider>();
        Collider[] objectColliders = heldObj.GetComponents<Collider>();

        foreach (Collider playerCol in playerColliders)
        {
            if (playerCol == null) continue;
            foreach (Collider objectCol in objectColliders)
            {
                if (objectCol == null) continue;
                Physics.IgnoreCollision(objectCol, playerCol, ignore);
            }
        }
    }
}