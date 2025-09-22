using UnityEngine;

public class GravityGunWorkbench : MonoBehaviour, IInteractable
{
    [Header("Workbench Settings")]
    [SerializeField] private Transform gravityGunPivot;
    [SerializeField] private Vector3 assembledPosition = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 disassembledPosition = new Vector3(0, 0, 0);
    [SerializeField] private float assemblySpeed = 2f;

    [Header("Assembly Requirements")]
    [SerializeField] private bool requiresParts = true;
    [SerializeField] private GravityGunPart[] requiredParts;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject assemblyPrompt;
    [SerializeField] private GameObject missingPartsPrompt;

    [Header("Gravity Gun Prefab")]
    [SerializeField] private GameObject gravityGunPrefab;
    [SerializeField] private Transform gunSpawnPoint;

    private bool isGunAssembled = false;
    private bool isAnimating = false;
    private Vector3 targetPosition;
    private GameObject spawnedGun;

    private void Start()
    {
        targetPosition = disassembledPosition;
        gravityGunPivot.localPosition = targetPosition;
        
        if (assemblyPrompt != null) assemblyPrompt.SetActive(false);
        if (missingPartsPrompt != null) missingPartsPrompt.SetActive(false);
    }

    private void Update()
    {
        if (isAnimating)
        {
            gravityGunPivot.localPosition = Vector3.MoveTowards(
                gravityGunPivot.localPosition,
                targetPosition,
                assemblySpeed * Time.deltaTime
            );

            if (Vector3.Distance(gravityGunPivot.localPosition, targetPosition) < 0.01f)
            {
                gravityGunPivot.localPosition = targetPosition;
                isAnimating = false;
                
                if (isGunAssembled)
                {
                    OnGravityGunAssembled();
                }
                else
                {
                    OnGravityGunDisassembled();
                }
            }
        }
    }

    public void Interact()
    {
        TryAssembleGun();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (CanAssembleGun())
            {
                ShowAssemblyPrompt();
            }
            else
            {
                ShowMissingPartsPrompt();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HidePrompts();
        }
    }

    public void TryAssembleGun()
    {
        if (isAnimating) return;

        if (!CanAssembleGun())
        {
            Debug.Log("Cannot assemble gravity gun. Missing required parts.");
            return;
        }

        isGunAssembled = !isGunAssembled;
        targetPosition = isGunAssembled ? assembledPosition : disassembledPosition;
        isAnimating = true;

        if (isGunAssembled && requiresParts)
        {
            ConsumeRequiredParts();
        }
        
        HidePrompts();
    }

    private bool CanAssembleGun()
    {
        if (!requiresParts) return true;
        
        if (GravityGunPartsInventory.Instance == null)
        {
            Debug.LogError("GravityGunPartsInventory instance not found!");
            return false;
        }

        foreach (GravityGunPart part in requiredParts)
        {
            if (!GravityGunPartsInventory.Instance.HasPart(part))
            {
                return false;
            }
        }

        return true;
    }

    private void ConsumeRequiredParts()
    {
        if (GravityGunPartsInventory.Instance == null) return;

        foreach (GravityGunPart part in requiredParts)
        {
            GravityGunPartsInventory.Instance.RemovePart(part);
        }

        Debug.Log("Consumed all required parts for gravity gun assembly.");
    }

    private void OnGravityGunAssembled()
    {
        Debug.Log("Gravity gun has been assembled and is ready for use!");
        
        if (gravityGunPrefab != null && gunSpawnPoint != null)
        {
            // Destroy any existing gun first
            if (spawnedGun != null)
            {
                Destroy(spawnedGun);
            }
            
            // Spawn the new gun
            spawnedGun = Instantiate(gravityGunPrefab, gunSpawnPoint.position, gunSpawnPoint.rotation);
            Debug.Log("Gravity gun spawned successfully!");
            
            // Make sure the gun has the necessary components
            SetupSpawnedGun(spawnedGun);
        }
        else
        {
            Debug.LogError("Gravity gun prefab or spawn point is not assigned in the inspector!");
        }
    }

    private void SetupSpawnedGun(GameObject gunObject)
    {
        // Add Rigidbody if missing
        if (gunObject.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gunObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        // Add Collider if missing
        if (gunObject.GetComponent<Collider>() == null)
        {
            gunObject.AddComponent<BoxCollider>();
        }

        // Add GravityGunPickup component if missing
        if (gunObject.GetComponent<GravityGunPickup>() == null)
        {
            gunObject.AddComponent<GravityGunPickup>();
        }

        // Make sure the gun has the GravityGun component
        if (gunObject.GetComponent<GravityGun>() == null)
        {
            Debug.LogWarning("Spawned gravity gun prefab is missing the GravityGun component!");
        }
    }

    private void OnGravityGunDisassembled()
    {
        Debug.Log("Gravity gun has been disassembled.");
        
        if (spawnedGun != null)
        {
            Destroy(spawnedGun);
            spawnedGun = null;
        }
    }

    private void ShowAssemblyPrompt()
    {
        if (assemblyPrompt != null)
        {
            assemblyPrompt.SetActive(true);
        }
        
        if (missingPartsPrompt != null)
        {
            missingPartsPrompt.SetActive(false);
        }
    }

    private void ShowMissingPartsPrompt()
    {
        if (missingPartsPrompt != null)
        {
            missingPartsPrompt.SetActive(true);
        }
        
        if (assemblyPrompt != null)
        {
            assemblyPrompt.SetActive(false);
        }
    }

    private void HidePrompts()
    {
        if (assemblyPrompt != null) assemblyPrompt.SetActive(false);
        if (missingPartsPrompt != null) missingPartsPrompt.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + assembledPosition, Vector3.one * 0.3f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + disassembledPosition, Vector3.one * 0.3f);
        
        // Draw spawn point
        if (gunSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(gunSpawnPoint.position, 0.2f);
        }
    }
}