using System.Collections.Generic;
using GraveyardHunter.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] private int _collectedTreasures;

        private int _requiredTreasures;
        private List<BoosterType> _activeBoosters = new List<BoosterType>();

        public void Initialize(int requiredTreasures)
        {
            _requiredTreasures = requiredTreasures;
            _collectedTreasures = 0;
            _activeBoosters.Clear();
        }

        public void CollectTreasure(TreasureType type)
        {
            _collectedTreasures++;

            EventBus.Publish(new TreasureCollectedEvent
            {
                Type = type,
                CurrentCount = _collectedTreasures,
                RequiredCount = _requiredTreasures
            });

            if (HasEnoughTreasures())
            {
                EventBus.Publish(new AllTreasuresCollectedEvent());
            }
        }

        public bool HasEnoughTreasures()
        {
            return _collectedTreasures >= _requiredTreasures;
        }

        public int GetCollectedCount()
        {
            return _collectedTreasures;
        }

        public void AddBooster(BoosterType booster)
        {
            if (!_activeBoosters.Contains(booster))
            {
                _activeBoosters.Add(booster);
            }
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
            _activeBoosters.Clear();
        }
    }
}
