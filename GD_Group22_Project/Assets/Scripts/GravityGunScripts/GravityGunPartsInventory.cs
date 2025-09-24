using UnityEngine;
using System.Collections.Generic;

public class GravityGunPartsInventory : MonoBehaviour
{
    [SerializeField] private List<int> collectedPartIds = new List<int>();
    private List<GravityGunPart> collectedParts = new List<GravityGunPart>();
    
    // Event to notify when all parts are collected
    public delegate void AllPartsCollectedHandler();
    public static event AllPartsCollectedHandler OnAllPartsCollected;
    
    public static GravityGunPartsInventory Instance { get; private set; }
    
    // Total number of parts needed to build the gravity gun
    [SerializeField] private int totalPartsRequired = 3;
    
    public bool HasAllParts { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPart(GravityGunPart part)
    {
        if (part == null)
        {
            Debug.LogError("Tried to add a null part!");
            return;
        }

        if (!collectedPartIds.Contains(part.partId))
        {
            collectedPartIds.Add(part.partId);
            collectedParts.Add(part);
            Debug.Log($"Gravity gun part added: {part.partName} (ID: {part.partId})");
            
            // Update UI with current count
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddGravityGunPartToUI(part, collectedParts.Count, totalPartsRequired);
            }
            
            // Check if all parts are collected
            CheckIfAllPartsCollected();
        }
        else
        {
            Debug.Log($"Part already collected: {part.partName}");
        }
    }

    private void CheckIfAllPartsCollected()
    {
        if (collectedParts.Count >= totalPartsRequired)
        {
            HasAllParts = true;
            Debug.Log("All gravity gun parts collected! The gravity gun can now be picked up.");
            
            // Notify other scripts that all parts are collected
            OnAllPartsCollected?.Invoke();
        }
    }

    public void RemovePart(GravityGunPart part)
    {
        if (part != null && collectedPartIds.Contains(part.partId))
        {
            collectedPartIds.Remove(part.partId);
            collectedParts.Remove(part);
            
            // Update UI with current count
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RemoveGravityGunPartFromUI(part, collectedParts.Count, totalPartsRequired);
            }
            
            Debug.Log("Removed part from inventory: " + part.partName);
            
            // Update the all parts collected status
            HasAllParts = collectedParts.Count >= totalPartsRequired;
        }
    }

    public bool HasPart(GravityGunPart part)
    {
        return part != null && collectedPartIds.Contains(part.partId);
    }

    public int GetCollectedPartsCount()
    {
        return collectedParts.Count;
    }

    public void ClearInventory()
    {
        collectedPartIds.Clear();
        collectedParts.Clear();
        HasAllParts = false;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClearAllGravityGunParts();
        }
        
        Debug.Log("Cleared all gravity gun parts from inventory");
    }
}