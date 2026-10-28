using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ThegameioRacing.Race;

namespace ThegameioRacing.UI
{
    /// <summary>
    /// Displays dramatic, animated 3-2-1-GO! countdown graphics at race start.
    /// </summary>
    public class RaceCountdownUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text countdownText;
        [SerializeField] private GameObject countdownPanel;

        [Header("Colors")]
        [SerializeField] private Color colorReady = new Color(1f, 0.28f, 0.22f); // Vibrant Red
        [SerializeField] private Color colorSet = new Color(1f, 0.82f, 0.12f);   // Amber Gold
        [SerializeField] private Color colorGo = new Color(0.18f, 0.95f, 0.42f);   // Emerald Green

        private AudioSource uiAudioSource;

        private void Awake()
        {
            if (countdownPanel == null) countdownPanel = gameObject;
            EnsureTextElement();

            uiAudioSource = GetComponent<AudioSource>();
            if (uiAudioSource == null) uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.spatialBlend = 0f; // 2D UI sound
        }

        private void Start()
        {
            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnCountdownTick += HandleCountdownTick;
            }
        }

        private void OnDestroy()
        {
            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnCountdownTick -= HandleCountdownTick;
            }
        }

        private void EnsureTextElement()
        {
            if (countdownText == null)
            {
                countdownText = GetComponentInChildren<Text>();
                if (countdownText == null)
                {
                    GameObject tObj = new GameObject("Countdown_Text");
                    tObj.transform.SetParent(transform, false);
                    countdownText = tObj.AddComponent<Text>();
                    countdownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    countdownText.fontSize = 96;
                    countdownText.alignment = TextAnchor.MiddleCenter;
                    countdownText.fontStyle = FontStyle.Bold;

                    RectTransform rt = countdownText.rectTransform;
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(600f, 200f);
                }
            }
        }

        private void HandleCountdownTick(int count)
        {
            EnsureTextElement();
            countdownPanel.SetActive(true);
            StopAllCoroutines();

            if (count > 0)
            {
                countdownText.text = count.ToString();
                countdownText.color = (count > 1) ? colorReady : colorSet;
                StartCoroutine(PunchScale(countdownText.rectTransform));

                if (uiAudioSource != null)
                {
                    uiAudioSource.PlayOneShot(ThegameioRacing.Audio.ProceduralAudio.CreateCountdownBeepClip(440f, 0.22f));
                }
            }
            else
            {
                // GO!
                countdownText.text = "GO!";
                countdownText.color = colorGo;
                StartCoroutine(PunchScale(countdownText.rectTransform));

                if (uiAudioSource != null)
                {
                    uiAudioSource.PlayOneShot(ThegameioRacing.Audio.ProceduralAudio.CreateCountdownBeepClip(880f, 0.40f));
                }

                StartCoroutine(HideAfterDelay(1.2f));
            }
        }

        private IEnumerator PunchScale(RectTransform rt)
        {
            float duration = 0.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float scale = Mathf.Lerp(1.6f, 1.0f, progress);
                rt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            countdownPanel.SetActive(false);
        }
    }
}
