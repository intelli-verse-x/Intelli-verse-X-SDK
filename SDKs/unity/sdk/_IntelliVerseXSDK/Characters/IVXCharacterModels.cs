using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.Characters
{
    /// <summary>
    /// Stat block for a character.
    /// </summary>
    [Serializable]
    public class IVXCharacterStats
    {
        [JsonProperty("speed")] public float speed;
        [JsonProperty("power")] public float power;
        [JsonProperty("defense")] public float defense;
        [JsonProperty("luck")] public float luck;
    }

    /// <summary>
    /// Represents a collectible or playable character.
    /// </summary>
    [Serializable]
    public class IVXCharacter
    {
        [JsonProperty("character_id")] public string characterId;
        [JsonProperty("name")] public string name;
        [JsonProperty("description")] public string description;
        [JsonProperty("rarity")] public string rarity;
        [JsonProperty("unlocked")] public bool unlocked;
        [JsonProperty("unlock_cost")] public int unlockCost;
        [JsonProperty("unlock_currency")] public string unlockCurrency;
        [JsonProperty("stats")] public IVXCharacterStats stats;
    }

    /// <summary>
    /// Current character roster state for a player.
    /// </summary>
    [Serializable]
    public class IVXCharacterState
    {
        [JsonProperty("characters")] public List<IVXCharacter> characters;
        [JsonProperty("active_character_id")] public string activeCharacterId;
    }

    /// <summary>
    /// Response wrapper for character state.
    /// </summary>
    [Serializable]
    public class IVXCharacterStateResponse
    {
        [JsonProperty("state")] public IVXCharacterState state;
    }

    /// <summary>
    /// Request payload for character operations.
    /// </summary>
    [Serializable]
    public class IVXCharacterRequest
    {
        [JsonProperty("character_id")] public string characterId;
    }
}
