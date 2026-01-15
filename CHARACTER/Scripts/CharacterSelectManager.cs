using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharacterSelectManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        [TextArea] public string description;
        public GameObject characterPrefab;
        public List<string> goodProperties;
        public List<string> badProperties;
    }

    [Header("Character Data")]
    [SerializeField] private CharacterData[] characters;

    [Header("UI References")]
    [SerializeField] private GameObject uiRoot; // Assign the Canvas here
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI propertiesText; // Will accumulate both good and bad
    [SerializeField] private Button confirmButton;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;

    private int selectedIndex = -1;
    private WaveSpawner waveSpawner;
    private CameraFollow cameraFollow;

    private void Start()
    {
        // Find references if not assigned, though usually they should be assigned in Inspector
        waveSpawner = FindFirstObjectByType<WaveSpawner>();
        cameraFollow = FindFirstObjectByType<CameraFollow>();

        // Start with nothing selected
        ClearSelection();
        
        // Ensure buttons are hooked up? 
        // We assume the 4 buttons in the scene will call SelectCharacter(index) via OnClick events setup in Inspector.
    }

    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= characters.Length) return;

        selectedIndex = index;
        UpdateUI(characters[selectedIndex]);
        confirmButton.interactable = true;
    }

    private void UpdateUI(CharacterData data)
    {
        nameText.text = data.characterName;
        descriptionText.text = data.description;

        string props = "";

        foreach (var good in data.goodProperties)
        {
            props += $"<color=green>+ {good}</color>\n";
        }

        foreach (var bad in data.badProperties)
        {
            props += $"<color=red>- {bad}</color>\n";
        }

        propertiesText.text = props;
    }

    private void ClearSelection()
    {
        selectedIndex = -1;
        nameText.text = "> > > > > > > > >";
        descriptionText.text = "Select a character, and it's name, description, and stats will be shown here! They all function slightly differently.";
        propertiesText.text = "plzzz select one plz plz plz :3";
        confirmButton.interactable = false;
    }

    public void OnConfirm()
    {
        if (selectedIndex == -1) return;

        CharacterData data = characters[selectedIndex];
        
        // Spawn the player
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        GameObject playerInstance = Instantiate(data.characterPrefab, spawnPos, Quaternion.identity);

        // Update dependencies
        if (cameraFollow != null) cameraFollow.SetTarget(playerInstance.transform);
        if (waveSpawner != null) waveSpawner.SetPlayer(playerInstance.transform);

        // Destroy the UI
        if (uiRoot != null) Destroy(uiRoot);
        else Destroy(gameObject);
    }
}
