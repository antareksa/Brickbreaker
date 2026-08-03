using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Held inventory, not equipped slots -- a Consumable is bought, sits here until the player
// manually uses it (only during Aiming), then it's gone. Entirely separate slot count and list
// from PowerUpManager -- the two systems don't share capacity, and unlike PowerUp there's no
// unique-item restriction (holding/buying duplicates of the same Consumable is fine).
public class ConsumableManager : MonoBehaviour
{
    public static ConsumableManager Instance { get; private set; }

    public int MaxSlots = 2;

    // Every Consumable asset that can appear in the Shop -- same role as PowerUpManager.Roster.
    public List<BaseConsumable> Roster;

    public UnityEvent OnConsumablesChanged = new UnityEvent();

    private readonly List<BaseConsumable> _heldConsumables = new List<BaseConsumable>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IReadOnlyList<BaseConsumable> GetHeld() => _heldConsumables;

    public bool IsFull => _heldConsumables.Count >= MaxSlots;

    public bool TryAdd(BaseConsumable consumable)
    {
        if (IsFull) return false;

        _heldConsumables.Add(consumable);
        OnConsumablesChanged?.Invoke();
        return true;
    }

    // Only usable during Aiming -- a proactive choice made before committing to a shot, not a
    // reactive save mid-shot.
    public bool TryUse(BaseConsumable consumable)
    {
        if (GameManager.Instance.StateMachine.CurrentState != GameState.Aiming) return false;
        if (!_heldConsumables.Contains(consumable)) return false;

        _heldConsumables.Remove(consumable);
        OnConsumablesChanged?.Invoke();

        consumable.Effect?.Use();
        return true;
    }

    public void ResetConsumables()
    {
        _heldConsumables.Clear();
        OnConsumablesChanged?.Invoke();
    }
}
