using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ChemicalInventory : MonoBehaviour
{
    public static ChemicalInventory Instance;
    
    [System.Serializable]
    public class InventorySlot
    {
        public ChemicalItem chemical;
        public int quantity;
    }
    
    [System.Serializable]
    public class ApparatusSlot
    {
        public ApparatusItem apparatus;
        public bool collected;
    }
    
    [Header("Inventory Contents")]
    public List<InventorySlot> chemicals = new List<InventorySlot>();
    public List<ApparatusSlot> apparatusList = new List<ApparatusSlot>();
    
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform chemicalsGrid;
    [SerializeField] private Transform apparatusGrid;
    [SerializeField] private GameObject chemicalSlotPrefab;
    [SerializeField] private GameObject apparatusSlotPrefab;
    [SerializeField] private TMP_Text chemicalsCountText;
    [SerializeField] private TMP_Text apparatusCountText;

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
    
    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        UpdateInventoryUI();
    }
    
    public void AddChemical(ChemicalItem chemical, int amount = 1)
    {
        InventorySlot existingSlot = chemicals.Find(slot => slot.chemical == chemical);
        
        if (existingSlot != null)
        {
            existingSlot.quantity += amount;
        }
        else
        {
            chemicals.Add(new InventorySlot { chemical = chemical, quantity = amount });
        }
        
        Debug.Log($"Added {amount} {chemical.chemicalName} to inventory. Total: {GetChemicalAmount(chemical)}");
        UpdateInventoryUI();
    }
    
    public void RemoveChemical(ChemicalItem chemical, int amount = 1)
    {
        InventorySlot existingSlot = chemicals.Find(slot => slot.chemical == chemical);
        
        if (existingSlot != null)
        {
            existingSlot.quantity -= amount;
            if (existingSlot.quantity <= 0)
            {
                chemicals.Remove(existingSlot);
            }
        }
        
        UpdateInventoryUI();
    }
    
    public bool HasChemical(ChemicalItem chemical, int requiredAmount = 1)
    {
        InventorySlot slot = chemicals.Find(s => s.chemical == chemical);
        return slot != null && slot.quantity >= requiredAmount;
    }
    
    public int GetChemicalAmount(ChemicalItem chemical)
    {
        InventorySlot slot = chemicals.Find(s => s.chemical == chemical);
        return slot?.quantity ?? 0;
    }
    
    public void AddApparatus(ApparatusItem apparatus)
    {
        ApparatusSlot existingSlot = apparatusList.Find(slot => slot.apparatus == apparatus);
        
        if (existingSlot != null)
        {
            existingSlot.collected = true;
        }
        else
        {
            apparatusList.Add(new ApparatusSlot { apparatus = apparatus, collected = true });
        }
        
        Debug.Log($"Added {apparatus.apparatusName} to inventory");
        UpdateInventoryUI();
    }
    
    public bool HasApparatus(ApparatusItem apparatus)
    {
        ApparatusSlot slot = apparatusList.Find(s => s.apparatus == apparatus);
        return slot != null && slot.collected;
    }
    
    private void UpdateInventoryUI()
    {
        UpdateChemicalsUI();
        UpdateApparatusUI();
        UpdateCountText();
    }
    
    private void UpdateChemicalsUI()
    {
        if (chemicalsGrid == null || chemicalSlotPrefab == null) return;
        
        // Clear existing slots
        foreach (Transform child in chemicalsGrid)
        {
            Destroy(child.gameObject);
        }
        
        // Create new slots
        foreach (InventorySlot slot in chemicals)
        {
            GameObject slotObj = Instantiate(chemicalSlotPrefab, chemicalsGrid);
            ChemicalSlotUI slotUI = slotObj.GetComponent<ChemicalSlotUI>();
            
            if (slotUI != null)
            {
                slotUI.SetChemical(slot.chemical, slot.quantity);
            }
        }
    }
    
    private void UpdateApparatusUI()
    {
        if (apparatusGrid == null || apparatusSlotPrefab == null) return;
        
        // Clear existing slots
        foreach (Transform child in apparatusGrid)
        {
            Destroy(child.gameObject);
        }
        
        // Create new slots
        foreach (ApparatusSlot slot in apparatusList)
        {
            if (slot.collected)
            {
                GameObject slotObj = Instantiate(apparatusSlotPrefab, apparatusGrid);
                ApparatusSlotUI slotUI = slotObj.GetComponent<ApparatusSlotUI>();
                
                if (slotUI != null)
                {
                    slotUI.SetApparatus(slot.apparatus);
                }
            }
        }
    }
    
    private void UpdateCountText()
    {
        if (chemicalsCountText != null)
        {
            chemicalsCountText.text = $"Chemicals: {chemicals.Count}";
        }
        
        if (apparatusCountText != null)
        {
            int collectedCount = apparatusList.FindAll(slot => slot.collected).Count;
            apparatusCountText.text = $"Apparatus: {collectedCount}/{apparatusList.Count}";
        }
    }
    
    private void Update()
    {
        // Toggle inventory UI with Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryPanel != null)
            {
                bool newState = !inventoryPanel.activeSelf;
                inventoryPanel.SetActive(newState);
                
                if (newState)
                {
                    UpdateInventoryUI();
                }
            }
        }
    }
    
    // Debug method to print current inventory
    public void PrintInventory()
    {
        Debug.Log("=== CHEMICAL INVENTORY ===");
        foreach (InventorySlot slot in chemicals)
        {
            Debug.Log($"{slot.chemical.chemicalName}: {slot.quantity}");
        }
        
        Debug.Log("=== APPARATUS INVENTORY ===");
        foreach (ApparatusSlot slot in apparatusList)
        {
            Debug.Log($"{slot.apparatus.apparatusName}: {(slot.collected ? "Collected" : "Missing")}");
        }
    }
}