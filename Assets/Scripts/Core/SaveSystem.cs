using UnityEngine;

namespace Riverforge
{
    public static class SaveSystem
    {
        private const string Key = "riverforge_save_v1";

        public static void Save(DamGameState state)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(state));
            PlayerPrefs.Save();
        }

        public static bool TryLoad(out DamGameState state)
        {
            if (!PlayerPrefs.HasKey(Key))
            {
                state = null;
                return false;
            }

            state = JsonUtility.FromJson<DamGameState>(PlayerPrefs.GetString(Key));
            return state != null;
        }

        public static void Clear() => PlayerPrefs.DeleteKey(Key);
    }
}
