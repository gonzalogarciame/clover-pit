using UnityEngine;

public class LeverVisibilityController : MonoBehaviour
{
    [Header("References (drag in Inspector)")]
    [SerializeField] private GameObject leverRoot;
    [SerializeField] private GameObject gamePanel;   // PANEL DEL JUEGO (PLAY)
    [SerializeField] private GameObject menuPanel;   
    [SerializeField] private GameObject endPanel;    

    void Awake()
    {
        Apply();
    }

    void OnEnable()
    {
        Apply();
    }

    void LateUpdate()
    {
        Apply();
    }

    private void Apply()
    {
        if (leverRoot == null || gamePanel == null) return;

        bool gameOn = gamePanel.activeInHierarchy;
        bool menuOn = menuPanel != null && menuPanel.activeInHierarchy;
        bool endOn = endPanel != null && endPanel.activeInHierarchy;

        // Visible si el panel de juego está activo y no hay overlays bloqueando
        bool shouldBeVisible = gameOn && !menuOn && !endOn;

        if (leverRoot.activeSelf != shouldBeVisible)
            leverRoot.SetActive(shouldBeVisible);
    }
}
