using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Transform holdPos;
    [SerializeField] private GameObject rightArmModel;

    [SerializeField] private Vector3 paintballGunPos;

    public float throwForce = 500f;
    public float pickUpRange = 5f;
    private GameObject heldObj;
    private Rigidbody heldObjRb;
    private bool canDrop = true;
    private int LayerNumber;
    private bool holdingPaintGun = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObj == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
                {
                    if (hit.transform.gameObject.tag == "canPickUp")
                    {
                        bool isPaintGun = hit.transform.GetComponent<PaintGun>() != null;

                        if (isPaintGun)
                        {
                            rightArmModel.SetActive(false);
                            holdingPaintGun = true;
                        }

                        PickUpObject(hit.transform.gameObject);
                    }
                }
            }
            else
            {
                if (canDrop == true)
                {
                    StopClipping();
                    DropObject();
                }
            }
        }

        
        if (Input.GetKeyDown(KeyCode.Mouse0)) 
        {
            if (heldObj != null && !holdingPaintGun) 
            {
                StopClipping();
                ThrowObject();
            }
        }

        if (heldObj != null)
        {
            MoveObject();

            if (holdingPaintGun && Input.GetKeyDown(KeyCode.Mouse0))
            {
                PaintGun paintGun = heldObj.GetComponent<PaintGun>();
                if (paintGun != null)
                {
                    paintGun.Shoot();
                }
            }
        }
    }

    void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj.GetComponent<Rigidbody>())
        {
            heldObj = pickUpObj;
            heldObjRb = pickUpObj.GetComponent<Rigidbody>();
            heldObjRb.isKinematic = true;
            heldObjRb.transform.parent = holdPos.transform;

            if (holdingPaintGun)
            {
                heldObj.transform.localRotation = Quaternion.identity;
                heldObj.transform.localPosition = paintballGunPos;
            }

            Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), true);
        }
    }

    void DropObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;

        if (holdingPaintGun)
        {
            rightArmModel.SetActive(true);
            holdingPaintGun = false;
        }

        heldObj = null;
    }

    void MoveObject()
    {
        if (!holdingPaintGun)
        {
            heldObj.transform.position = holdPos.transform.position;
        }
    }

    void ThrowObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObjRb.AddForce(transform.forward * throwForce);
        heldObj = null;
    }

    void StopClipping()
    {
        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);

        if (hits.Length > 1)
        {
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f);
        }
    }
}