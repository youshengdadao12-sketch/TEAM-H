using UnityEngine;
using UnityEngine.UI;

public sealed class HealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;

    private PlayerHealth health;

    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -= UpdateDisplay;
        }
    }

    public void Bind(PlayerHealth playerHealth)
    {
        if (health != null)
        {
            health.HealthChanged -= UpdateDisplay;
        }

        health = playerHealth;
        if (health != null)
        {
            health.HealthChanged += UpdateDisplay;
            UpdateDisplay(health.CurrentHealth, health.MaxHealth);
        }
    }

    private void UpdateDisplay(int current, int maximum)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maximum;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            healthText.text = $"{current} / {maximum}";
        }
    }
}
