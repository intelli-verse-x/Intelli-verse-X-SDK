using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IntelliVerseX.AI
{
    #region Quest & narrative templates

    /// <summary>
    /// Describes constraints and hints for procedural quest generation.
    /// </summary>
    [Serializable]
    public sealed class IVXQuestTemplate
    {
        /// <summary>Genre label (e.g. fantasy, sci-fi, horror).</summary>
        public string Genre;

        /// <summary>Difficulty band: easy, medium, or hard.</summary>
        public string Difficulty;

        /// <summary>Required gameplay or narrative elements (combat, puzzle, dialogue, etc.).</summary>
        public string[] RequiredElements;

        /// <summary>Approximate length in minutes.</summary>
        public int EstimatedDurationMinutes;

        /// <summary>Additional free-form instructions for the generator.</summary>
        public string CustomPrompt;
    }

    /// <summary>
    /// A generated quest definition returned from the content API.
    /// </summary>
    [Serializable]
    public sealed class IVXGeneratedQuest
    {
        /// <summary>Stable quest identifier from the backend.</summary>
        public string QuestId;

        /// <summary>Short player-facing title.</summary>
        public string Title;

        /// <summary>Longer description or briefing text.</summary>
        public string Description;

        /// <summary>Ordered objective strings.</summary>
        public string[] Objectives;

        /// <summary>Reward descriptions or IDs.</summary>
        public string[] Rewards;

        /// <summary>Difficulty label.</summary>
        public string Difficulty;

        /// <summary>Estimated play time in minutes.</summary>
        public int EstimatedDurationMinutes;

        /// <summary>Optional hook line for UI or VO.</summary>
        public string NarrativeHook;
    }

    /// <summary>
    /// A generated short story or narrative block.
    /// </summary>
    [Serializable]
    public sealed class IVXGeneratedStory
    {
        /// <summary>Story identifier from the backend.</summary>
        public string StoryId;

        /// <summary>Title of the piece.</summary>
        public string Title;

        /// <summary>Full story body.</summary>
        public string Content;

        /// <summary>Genre label.</summary>
        public string Genre;

        /// <summary>Approximate word count.</summary>
        public int WordCount;
    }

    /// <summary>
    /// A generated item definition with stats and flavor text.
    /// </summary>
    [Serializable]
    public sealed class IVXGeneratedItem
    {
        /// <summary>Display name of the item.</summary>
        public string ItemName;

        /// <summary>Item category (weapon, consumable, etc.).</summary>
        public string ItemType;

        /// <summary>Rarity tier label.</summary>
        public string Rarity;

        /// <summary>Short flavor line.</summary>
        public string FlavorText;

        /// <summary>Longer mechanical or lore description.</summary>
        public string Description;

        /// <summary>Optional numeric stats keyed by name.</summary>
        public Dictionary<string, float> Stats;
    }

    /// <summary>
    /// A single line in a generated dialogue script.
    /// </summary>
    [Serializable]
    public sealed class IVXDialogueLine
    {
        /// <summary>Speaking character name or id.</summary>
        public string Character;

        /// <summary>Spoken text.</summary>
        public string Text;

        /// <summary>Emotional tone or expression hint.</summary>
        public string Emotion;

        /// <summary>Stage direction or animation hint.</summary>
        public string Action;
    }

    /// <summary>
    /// A full multi-line dialogue generated for a scenario.
    /// </summary>
    [Serializable]
    public sealed class IVXGeneratedDialogue
    {
        /// <summary>Scenario or beat identifier.</summary>
        public string ScenarioId;

        /// <summary>Ordered dialogue lines.</summary>
        public List<IVXDialogueLine> Lines;
    }

    #endregion

    #region API wire models

    /// <summary>
    /// Request body for <c>/content/generate</c>.
    /// </summary>
    public sealed class IVXContentGenRequest
    {
        /// <summary>Content kind: quest, story, item, dialogue, template, etc.</summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>Primary user or system prompt.</summary>
        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        /// <summary>Optional structured template (quest template, etc.).</summary>
        [JsonProperty("template")]
        public object Template { get; set; }

        /// <summary>Additional context (player state, world, etc.).</summary>
        [JsonProperty("context")]
        public string Context { get; set; }

        /// <summary>Maximum tokens to generate.</summary>
        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }

        /// <summary>Sampling temperature.</summary>
        [JsonProperty("temperature")]
        public float Temperature { get; set; }
    }

    /// <summary>
    /// Response from <c>/content/generate</c>; <see cref="Content"/> holds JSON for the concrete model.
    /// </summary>
    public sealed class IVXContentGenResponse
    {
        /// <summary>JSON string payload for the requested content type.</summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>Content type echo.</summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>Tokens consumed on the server.</summary>
        [JsonProperty("tokens_used")]
        public int TokensUsed { get; set; }
    }

    #endregion
}
