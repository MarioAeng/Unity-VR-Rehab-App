using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class RuntimeInputAssetAssigner : MonoBehaviour
{
    public InputActionAsset inputAsset; // Assign in Inspector

    void Awake()
    {
        var manager = GetComponent<InputActionManager>();
        if (manager == null)
        {
            Debug.LogError("[Fix] InputActionManager not found.");
            return;
        }

        if (inputAsset == null)
        {
            Debug.LogError("[Fix] InputActionAsset not assigned.");
            return;
        }

        manager.actionAssets = new List<InputActionAsset> { inputAsset };
        manager.EnableInput();
        Debug.Log("[Fix] InputActionAsset assigned at runtime.");
    }
}