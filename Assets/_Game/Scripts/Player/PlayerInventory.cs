using System.Collections.Generic;
using GraveyardHunter.Core;
using GraveyardHunter.Level;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] private int _collectedTreasures;

        private int _requiredTreasures;
        private List<BoosterType> _activeBoosters = new();

        // Per-type tracking
        private readonly Dictionary<TreasureType, int> _collectedPerType = new();
        private readonly Dictionary<TreasureType, int> _requiredPerType = new();
        private bool _useTypedRequirements;

        /// <summary>Legacy: initialize with a simple total count.</summary>
        public void Initialize(int requiredTreasures)
        {
            _requiredTreasures = requiredTreasures;
            _collectedTreasures = 0;
            _activeBoosters.Clear();
            _collectedPerType.Clear();
            _requiredPerType.Clear();
            _useTypedRequirements = false;

            PublishTreasureEvent(TreasureType.Gold);
        }

        /// <summary>Initialize with per-type requirements. E.g. 1 Gold + 1 Coin + 1 Diamond.</summary>
        public void Initialize(List<TreasureRequirement> requirements)
        {
            _collectedTreasures = 0;
            _activeBoosters.Clear();
            _collectedPerType.Clear();
            _requiredPerType.Clear();

            if (requirements == null || requirements.Count == 0)
            {
                _useTypedRequirements = false;
                _requiredTreasures = 0;
                PublishTreasureEvent(TreasureType.Gold);
                return;
            }

            _useTypedRequirements = true;
            _requiredTreasures = 0;

            foreach (var req in requirements)
            {
                _requiredPerType[req.Type] = req.Count;
                _collectedPerType[req.Type] = 0;
                _requiredTreasures += req.Count;
            }

            PublishTreasureEvent(TreasureType.Gold);
        }

        public void CollectTreasure(TreasureType type)
        {
            _collectedTreasures++;

            if (_useTypedRequirements)
            {
                if (!_collectedPerType.ContainsKey(type))
                    _collectedPerType[type] = 0;
                _collectedPerType[type]++;
            }

            PublishTreasureEvent(type);

            if (HasEnoughTreasures())
            {
                EventBus.Publish(new AllTreasuresCollectedEvent());
            }
        }

        public bool HasEnoughTreasures()
        {
            if (!_useTypedRequirements)
                return _collectedTreasures >= _requiredTreasures;

            // Check each type meets its requirement
            foreach (var kvp in _requiredPerType)
            {
                int collected = _collectedPerType.ContainsKey(kvp.Key) ? _collectedPerType[kvp.Key] : 0;
                if (collected < kvp.Value)
                    return false;
            }
            return true;
        }

        public int GetCollectedCount()
        {
            return _collectedTreasures;
        }

        public Dictionary<TreasureType, (int collected, int required)> GetTypeStatus()
        {
            var status = new Dictionary<TreasureType, (int, int)>();

            if (_useTypedRequirements)
            {
                foreach (var kvp in _requiredPerType)
                {
                    int collected = _collectedPerType.ContainsKey(kvp.Key) ? _collectedPerType[kvp.Key] : 0;
                    status[kvp.Key] = (collected, kvp.Value);
                }
            }

            return status;
        }

        private void PublishTreasureEvent(TreasureType lastType)
        {
            EventBus.Publish(new TreasureCollectedEvent
            {
                Type = lastType,
                CurrentCount = _collectedTreasures,
                RequiredCount = _requiredTreasures,
                TypeStatus = GetTypeStatus()
            });
        }

        public void AddBooster(BoosterType booster)
        {
            if (!_activeBoosters.Contains(booster))
                _activeBoosters.Add(booster);
        }

        public void RemoveBooster(BoosterType booster)
        {
            _activeBoosters.Remove(booster);
        }

        public bool HasBooster(BoosterType booster)
        {
            return _activeBoosters.Contains(booster);
        }

        public void Reset()
        {
            _collectedTreasures = 0;
            _collectedPerType.Clear();
            _requiredPerType.Clear();
            _activeBoosters.Clear();
        }
    }
}
