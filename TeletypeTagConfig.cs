using UnityEngine;

namespace TextMeshProExtension {
    [CreateAssetMenu(menuName = "TMP Tags/TeletypeTagConfig")]
    public class TeletypeTagConfig : ScriptableObject {

        [SerializeField] private float speed = 10;
        [SerializeField] private float delay = 0;
        [SerializeField] private int spacing = 5;
        [SerializeField] private bool isFading = false;
    }
}