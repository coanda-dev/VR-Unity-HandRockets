using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace HandRockets.Scripts
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class SceneSelect : MonoBehaviour
    {
        [SerializeField]
        private SceneAsset _scene;

        private XRGrabInteractable _interactable;

        private void Awake()
        {
            _interactable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            _interactable.selectEntered.AddListener(LoadScene);
        }

        private void OnDisable()
        {
            _interactable.selectEntered.RemoveListener(LoadScene);
        }

        void LoadScene(SelectEnterEventArgs args)
        {
            SceneManager.LoadScene(_scene.name);
        }
    }
}
