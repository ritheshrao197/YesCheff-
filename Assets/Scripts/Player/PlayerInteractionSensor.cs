using UnityEngine;
using YesChef.Interfaces;

namespace YesChef.Player
{
    public class PlayerInteractionSensor
    {
        private readonly Transform _origin;
        private readonly Collider[] _hits;
        private readonly LayerMask _interactionMask;
        private readonly float _interactionRadius;

        public PlayerInteractionSensor(Transform origin, float interactionRadius, LayerMask interactionMask, int maxHits = 16)
        {
            _origin = origin;
            _interactionRadius = interactionRadius;
            _interactionMask = interactionMask;
            _hits = new Collider[maxHits];
        }

        public IInteractable FindNearestInteractable()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(
                _origin.position,
                _interactionRadius,
                _hits,
                _interactionMask);

            IInteractable nearest = null;
            float nearestSqrDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hits[i];
                _hits[i] = null;

                if (hit == null || !hit.TryGetComponent(out IInteractable interactable))
                {
                    continue;
                }

                float sqrDistance = (hit.transform.position - _origin.position).sqrMagnitude;
                if (sqrDistance >= nearestSqrDistance)
                {
                    continue;
                }

                nearestSqrDistance = sqrDistance;
                nearest = interactable;
            }

            return nearest;
        }
    }
}
