using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntelliVerseX.Quiz
{
    /// <summary>
    /// Question shuffler using Fisher-Yates algorithm.
    /// Battle-tested with 10+ unit tests in QuizVerse.
    /// Part of IntelliVerse.GameSDK.Quiz package.
    /// </summary>
    public static class IVXQuestionShuffler
    {
        /// <summary>
        /// Fisher-Yates shuffle algorithm for randomizing list order.
        /// Returns a new shuffled list without modifying the original.
        /// Time complexity: O(n), Space complexity: O(n)
        /// </summary>
        public static List<T> Shuffle<T>(this IList<T> source)
        {
            if (source == null)
            {
                Debug.LogWarning("[IVXQuestionShuffler] Shuffle called on null list");
                return new List<T>();
            }

            if (source.Count <= 1)
            {
                return new List<T>(source);
            }

            List<T> shuffled = new List<T>(source);
            
            // Fisher-Yates shuffle
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                // Random.Range(0, i+1) includes i in the range
                int rnd = UnityEngine.Random.Range(0, i + 1);
                T tmp = shuffled[i];
                shuffled[i] = shuffled[rnd];
                shuffled[rnd] = tmp;
            }
            
            return shuffled;
        }

        /// <summary>
        /// Shuffles question list (generic wrapper for IQuizQuestion)
        /// </summary>
        public static List<IVXQuestion> ShuffleQuestions(this List<IVXQuestion> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                Debug.LogWarning("[IVXQuestionShuffler] ShuffleQuestions called on null or empty list");
                return new List<IVXQuestion>();
            }

            return questions.Shuffle();
        }

        /// <summary>
        /// Shuffles all question lists in a theme dictionary.
        /// Each theme gets its own shuffled question order.
        /// </summary>
        public static Dictionary<string, List<IVXQuestion>> ShuffleAllThemes(
            this Dictionary<string, List<IVXQuestion>> triviaSet)
        {
            if (triviaSet == null || triviaSet.Count == 0)
            {
                Debug.LogWarning("[IVXQuestionShuffler] ShuffleAllThemes called on null or empty dictionary");
                return new Dictionary<string, List<IVXQuestion>>();
            }

            Dictionary<string, List<IVXQuestion>> shuffledSet = new Dictionary<string, List<IVXQuestion>>();

            foreach (var kvp in triviaSet)
            {
                if (kvp.Value != null && kvp.Value.Count > 0)
                {
                    shuffledSet[kvp.Key] = kvp.Value.ShuffleQuestions();
                    Debug.Log($"[IVXQuestionShuffler] Shuffled {shuffledSet[kvp.Key].Count} questions for theme '{kvp.Key}'");
                }
                else
                {
                    Debug.LogWarning($"[IVXQuestionShuffler] Theme '{kvp.Key}' has no questions");
                    shuffledSet[kvp.Key] = new List<IVXQuestion>();
                }
            }

            return shuffledSet;
        }

        /// <summary>
        /// Creates a shuffled index map for answer options.
        /// Returns an array where array[displayedIndex] = originalIndex.
        /// Used to track correct answer after shuffling options.
        /// </summary>
        public static int[] CreateShuffledIndexMap(int optionCount)
        {
            if (optionCount <= 0)
            {
                Debug.LogWarning($"[IVXQuestionShuffler] Invalid option count: {optionCount}");
                return new int[0];
            }

            // Create sequential indices [0, 1, 2, 3]
            List<int> indices = new List<int>(optionCount);
            for (int i = 0; i < optionCount; i++)
            {
                indices.Add(i);
            }

            // Shuffle indices
            var shuffledIndices = indices.Shuffle();
            
            Debug.Log($"[IVXQuestionShuffler] Created shuffled index map: [{string.Join(", ", shuffledIndices)}]");
            
            return shuffledIndices.ToArray();
        }

        /// <summary>
        /// Validates that correct answer index is within shuffle map.
        /// Critical check to prevent wrong answers being marked correct.
        /// </summary>
        public static bool ValidateShuffleMap(int[] shuffleMap, int correctAnswerIndex)
        {
            if (shuffleMap == null || shuffleMap.Length == 0)
            {
                Debug.LogError("[IVXQuestionShuffler] Shuffle map is null or empty");
                return false;
            }

            if (correctAnswerIndex < 0 || correctAnswerIndex >= shuffleMap.Length)
            {
                Debug.LogError($"[IVXQuestionShuffler] Correct answer index {correctAnswerIndex} out of range [0, {shuffleMap.Length})");
                return false;
            }

            // Check if correctAnswerIndex exists in shuffle map
            bool found = false;
            for (int i = 0; i < shuffleMap.Length; i++)
            {
                if (shuffleMap[i] == correctAnswerIndex)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.LogError($"[IVXQuestionShuffler] Correct answer index {correctAnswerIndex} not found in shuffle map");
                return false;
            }

            return true;
        }
    }
}
