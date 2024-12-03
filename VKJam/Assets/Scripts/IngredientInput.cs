using DG.Tweening;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Pax.SecondMode
{
    public class IngredientInput : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button submitButton;

        [Header("Animation")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private Vector2 startPosition;
        [SerializeField] private Vector2 endPosition;

        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private Ease animationEase = Ease.OutQuad;

        public void Show()
        {
            gameObject.SetActive(true);

            panel.anchoredPosition = startPosition;
            panel.
                DOAnchorPos(endPosition, animationDuration).
                SetEase(animationEase).
                OnComplete(delegate
                {
                    Action<string> onSubmit = delegate (string input)
                    {
                        // spawn note on table
                        SecondModeManager.Instance.GuessSystem.SendAnswerServerRpc(input, new ServerRpcParams());
                        Clear();
                    };

                    inputField.onSubmit.AddListener(onSubmit.Invoke);
                    submitButton.onClick.AddListener(() => onSubmit.Invoke(inputField.text));
                }).
                Play();
        }

        public void Hide()
        {
            inputField.onSubmit = null;
            submitButton.onClick = null;

            gameObject.SetActive(false);
        }

        public void Clear()
        {
            inputField.text = string.Empty;
        }
    }
}