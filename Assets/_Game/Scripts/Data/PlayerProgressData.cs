using UnityEngine;

namespace GraveyardHunter.Data
{
    public static class PlayerProgressData
    {
        public static int GetCurrentLevel()
        {
            return PlayerPrefs.GetInt("CurrentLevel", 0);
        }

        public static void SetCurrentLevel(int level)
        {
            PlayerPrefs.SetInt("CurrentLevel", level);
            PlayerPrefs.Save();
        }

        public static int GetHighScore(int levelIndex)
        {
            return PlayerPrefs.GetInt($"HighScore_{levelIndex}", 0);
        }

        public static void SaveLevelProgress(int levelIndex, int score, int stars)
        {
            int currentHighScore = GetHighScore(levelIndex);
            if (score > currentHighScore)
            {
                PlayerPrefs.SetInt($"HighScore_{levelIndex}", score);
            }

            int currentStars = GetStars(levelIndex);
            if (stars > currentStars)
            {
                PlayerPrefs.SetInt($"Stars_{levelIndex}", stars);
            }

            PlayerPrefs.Save();
        }

        public static int GetStars(int levelIndex)
        {
            return PlayerPrefs.GetInt($"Stars_{levelIndex}", 0);
        }

        public static int GetTotalScore()
        {
            int total = 0;
            int currentLevel = GetCurrentLevel();
            for (int i = 0; i <= currentLevel; i++)
            {
                total += GetHighScore(i);
            }
            return total;
        }

        public static float GetSFXVolume()
        {
            return PlayerPrefs.GetFloat("SFXVolume", 1f);
        }

        public static void SetSFXVolume(float vol)
        {
            PlayerPrefs.SetFloat("SFXVolume", vol);
            PlayerPrefs.Save();
        }

        public static float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat("MusicVolume", 1f);
        }

        public static void SetMusicVolume(float vol)
        {
            PlayerPrefs.SetFloat("MusicVolume", vol);
            PlayerPrefs.Save();
        }

        public static int GetEquippedSkin()
        {
            return PlayerPrefs.GetInt("EquippedSkin", 0);
        }

        public static void SetEquippedSkin(int index)
        {
            PlayerPrefs.SetInt("EquippedSkin", index);
            PlayerPrefs.Save();
        }

        public static bool IsSkinUnlocked(int index)
        {
            if (index == 0) return true;
            return PlayerPrefs.GetInt($"Skin_{index}", 0) == 1;
        }

        public static void UnlockSkin(int index)
        {
            PlayerPrefs.SetInt($"Skin_{index}", 1);
            PlayerPrefs.Save();
        }

        public static bool IsTutorialCompleted()
        {
            return PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
        }

        public static void SetTutorialCompleted(bool completed)
        {
            PlayerPrefs.SetInt("TutorialCompleted", completed ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void ResetAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
