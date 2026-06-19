using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerAbility : MonoBehaviour
{
    [SerializeField, Min(0f)] private float cooldown = 3f;
    [SerializeField] private InputActionReference abilityAction;

    private float availableAt;

    public float CooldownRemaining => Mathf.Max(0f, availableAt - Time.time);
    public bool IsReady => CooldownRemaining <= 0f;
    public event Action AbilityUsed;

    private void OnEnable()
    {
        if (abilityAction != null)
        {
            abilityAction.action.Enable();
            abilityAction.action.performed += OnAbilityPerformed;
        }
    }

    private void OnDisable()
    {
        if (abilityAction != null)
        {
            abilityAction.action.performed -= OnAbilityPerformed;
            abilityAction.action.Disable();
        }
    }

    private void Update()
    {
        if (abilityAction == null && Keyboard.current?.kKey.wasPressedThisFrame == true)
        {
            TryUse();
        }
    }

    public bool TryUse()
    {
        if (!IsReady)
        {
            return false;
        }

        availableAt = Time.time + cooldown;
        AbilityUsed?.Invoke();
        return true;
    }

    private void OnAbilityPerformed(InputAction.CallbackContext context)
    {
        TryUse();
    }
}
