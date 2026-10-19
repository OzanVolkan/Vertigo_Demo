using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Vertigo.Core.Interfaces;

namespace Vertigo.Core.Save
{
    public static class SaveService
    {
        public const string GoldId = "Reward_Gold";
        public const string CashId = "Reward_Cash";
        private const string LegacyBankPrefix = "PlayerBank_";

        public static string FilePath => Path.Combine(Application.persistentDataPath, "player_save.json");
        private static PlayerSaveData _data;

        public static PlayerSaveData Data => EnsureLoaded();

        private static PlayerSaveData EnsureLoaded()
        {
            if (_data != null) return _data;

            string path = FilePath;
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    _data = JsonUtility.FromJson<PlayerSaveData>(json) ?? new PlayerSaveData();
                    return _data;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveService] Error reading save file: {ex.Message}");
                    _data = new PlayerSaveData();
                    return _data;
                }
            }

            _data = MigrateFromPlayerPrefs();
            Save();
            return _data;
        }

        public static void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(EnsureLoaded(), true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Error writing save file: {ex.Message}");
            }
        }

        public static int GetAmount(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            var data = EnsureLoaded();

            if (id == GoldId) return data.gold;
            if (id == CashId) return data.cash;

            var entry = data.items.Find(x => x.id == id);
            return entry.amount;
        }

        public static void AddAmount(string id, int amount)
        {
            if (string.IsNullOrEmpty(id) || amount <= 0) return;
            AddAmountInternal(id, amount);
            Save();
        }

        public static void BankRewards(IEnumerable<KeyValuePair<string, (IRewardData reward, int amount)>> sessionItems)
        {
            if (sessionItems == null) return;
            EnsureLoaded();

            foreach (var kvp in sessionItems)
            {
                if (kvp.Value.amount > 0)
                {
                    AddAmountInternal(kvp.Key, kvp.Value.amount);
                }
            }
            Save();
        }

        public static bool TrySpendGold(int cost)
        {
            if (cost <= 0) return true;
            var data = EnsureLoaded();
            if (data.gold < cost) return false;

            data.gold -= cost;
            Save();
            return true;
        }

        public static void Reset()
        {
            _data = new PlayerSaveData();
            Save();
        }

        private static void AddAmountInternal(string id, int amount)
        {
            var data = EnsureLoaded();
            if (id == GoldId)
            {
                data.gold += amount;
            }
            else if (id == CashId)
            {
                data.cash += amount;
            }
            else
            {
                int index = data.items.FindIndex(x => x.id == id);
                if (index >= 0)
                {
                    var entry = data.items[index];
                    entry.amount += amount;
                    data.items[index] = entry;
                }
                else
                {
                    data.items.Add(new RewardSaveEntry(id, amount));
                }
            }
        }

        private static PlayerSaveData MigrateFromPlayerPrefs()
        {
            var data = new PlayerSaveData();
            string goldKey = $"{LegacyBankPrefix}{GoldId}";
            string cashKey = $"{LegacyBankPrefix}{CashId}";

            if (PlayerPrefs.HasKey(goldKey))
            {
                data.gold = PlayerPrefs.GetInt(goldKey, 0);
                PlayerPrefs.DeleteKey(goldKey);
            }

            if (PlayerPrefs.HasKey(cashKey))
            {
                data.cash = PlayerPrefs.GetInt(cashKey, 0);
                PlayerPrefs.DeleteKey(cashKey);
            }

            PlayerPrefs.Save();
            return data;
        }
    }
}
