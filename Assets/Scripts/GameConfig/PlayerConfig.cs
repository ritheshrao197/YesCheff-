using UnityEngine;

namespace YesChef.Core
{
    [CreateAssetMenu(menuName = "YesChef/Player Config", fileName = "PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Min(0f)]
        public float moveSpeed = 5f;

        [Min(0f)]
        public float interactionRadius = 1.5f;

        public Vector3 holdOffset = new Vector3(0.5f, 0.8f, 0.5f);
    }
}
