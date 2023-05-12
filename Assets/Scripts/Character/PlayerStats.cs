using UnityEngine;
using System.Collections;
using System;

public delegate void StatModifiedHandle();

public class PlayerStats : Singleton<PlayerStats>
{

    public float Health { get => health; set { health = Mathf.Clamp(value, 0f, maxHealth); OnHealthModified?.Invoke(); } }
    [SerializeField] public float maxHealth;
    [SerializeField] float health;
    public event StatModifiedHandle OnHealthModified;

    public float Mana { get => mana; set => mana = Mathf.Clamp(value, 0f, maxMana); }
    [SerializeField] public float maxMana;
    [SerializeField] float mana;


    public float Stamina { get => stamina; set => stamina = Mathf.Clamp(value, 0f, maxStamina); }
    [SerializeField] public float maxStamina;
    [SerializeField] float stamina;



    [field: SerializeField] public int Strength { get; set; }
    [field: SerializeField] public int Dexterity { get; set; }
    [field: SerializeField] public int Intelligence { get; set; }

    [field: SerializeField] public int Armor { get; set; }

    public int MeleeDamage { get => BaseMeleeDamage + MeleeBonus; }
    [SerializePropertyReadOnly("MeleeDamage")] public int BaseMeleeDamage;
    public int RangedDamage { get => BaseRangedDamage + RangedBonus; }
    [SerializePropertyReadOnly("RangedDamage")] public int BaseRangedDamage;
    public float CriticalChance { get => BaseCriticalChance + CriticalBonus; }
    [SerializePropertyReadOnly("CriticalChance")] public float BaseCriticalChance;

    public int MeleeBonus { get => Mathf.RoundToInt(Strength * 2.5f); }
    public int RangedBonus { get => Mathf.RoundToInt(Dexterity * 2.5f); }
    public float CriticalBonus { get => Intelligence * .4f; }

    public void Modify(Action operation)
    {
        operation();
    }

    public void ModifyTemporarily(Action doOperation, Action undoOperation, int duration)
    {
        doOperation();
        StartCoroutine(ExecuteOperation(undoOperation, duration));
    }

    private IEnumerator ExecuteOperation(Action operation, int delay = 0)
    {
        yield return new WaitForSeconds(delay);
        operation();
    }

    // Use it like: ModifyPerInterval(() => stat += amount, interval, duration);
    public void ModifyPerInterval(Action operation, int interval, int duration)
    {
        if (duration <= 0 || interval <= 0 || duration < interval) throw new Exception("Wrong values for duration or interval");
        StartCoroutine(ModifyPerIntervalRoutine(operation, interval, duration));
    }

    IEnumerator ModifyPerIntervalRoutine(Action operation, int interval, int duration)
    {
        for (int i = 0; i <= duration; i += interval)
        {
            operation();
            yield return new WaitForSeconds(interval);
        }
    }

    public override string ToString()
    {
        return string.Concat(
            "Strength: ", Strength,
            "\nDexterity: ", Dexterity,
            "\nIntelligence: ", Intelligence,
            "\nArmor: ", Armor);
    }

    // Test if stats based on others are correct after load!!!!!!

    #region Serialization

    public PlayerStatsData GetData()
    {
        return new PlayerStatsData(this);
    }

    public void SetData(PlayerStatsData playerStatsData)
    {
        Health = playerStatsData.health;
        Mana = playerStatsData.mana;
        Stamina = playerStatsData.stamina;
        maxHealth = playerStatsData.maxHealth;
        maxMana = playerStatsData.maxMana;
        maxStamina = playerStatsData.maxStamina;

        Strength = playerStatsData.strength;
        Dexterity = playerStatsData.dexterity;
        Intelligence = playerStatsData.intelligence;

        BaseMeleeDamage = playerStatsData.baseMeleeDamage;
        BaseRangedDamage = playerStatsData.baseRangedDamage;
        BaseCriticalChance = playerStatsData.baseCriticalChance;
    }
}

[Serializable]
public class PlayerStatsData
{
    public float health, mana, stamina,
        maxHealth, maxMana, maxStamina;

    public int strength, dexterity, intelligence;

    public int baseMeleeDamage;
    public int baseRangedDamage;
    public float baseCriticalChance;

    public PlayerStatsData(PlayerStats playerStats)
    {
        health = playerStats.Health;
        mana = playerStats.Mana;
        stamina = playerStats.Stamina;
        maxHealth = playerStats.maxHealth;
        maxMana = playerStats.maxMana;
        maxStamina = playerStats.maxStamina;

        strength = playerStats.Strength;
        dexterity = playerStats.Dexterity;
        intelligence = playerStats.Intelligence;

        baseMeleeDamage = playerStats.BaseMeleeDamage;
        baseRangedDamage = playerStats.BaseRangedDamage;
        baseCriticalChance = playerStats.BaseCriticalChance;
    }
}

#endregion