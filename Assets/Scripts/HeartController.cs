using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    public PLayerController player;  // Reference to the player script
    public Transform heartsParent;   // The UI parent where hearts are displayed
    public GameObject heartPrefab;   // Prefab for a single heart

    private List<Image> heartFills = new List<Image>();  // List to hold the hearts images

    private void Start()
    {
        player = PLayerController.Instance;
        UpdateHearts();
    }

    private void Update()
    {
        UpdateHearts();
    }

    private void UpdateHearts()
    {
        // Adjust the number of heart images based on player's max health
        while (heartFills.Count < player.maxHealth)
        {
            GameObject heart = Instantiate(heartPrefab, heartsParent);
            heartFills.Add(heart.GetComponent<Image>());
        }

        // Remove extra hearts if max health is decreased
        while (heartFills.Count > player.maxHealth)
        {
            Destroy(heartFills[heartFills.Count - 1].gameObject);
            heartFills.RemoveAt(heartFills.Count - 1);
        }

        // Update each heart's fill amount based on current health
        for (int i = 0; i < heartFills.Count; i++)
        {
            if (i < player.Health)
            {
                heartFills[i].fillAmount = 1;  // Full heart
            }
            else
            {
                heartFills[i].fillAmount = 0;  // Empty heart
            }
        }
    }
}