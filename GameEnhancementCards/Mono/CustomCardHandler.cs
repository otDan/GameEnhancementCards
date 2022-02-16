﻿using GameEnhancementCards.Utils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace GameEnhancementCards.Mono
{
    class CustomCardHandler : MonoBehaviour
    {
        private static readonly Color firstColor = new Color(1f, 0f, 0.914f, 0.5f);
        private static readonly Color secondColor = new Color(1f, 0f, 0.014f, 1f);
        private static readonly Color triangleColor = new Color(0.90f, 0.90f, 0.90f, 0.75f);
        private static readonly Color textColor = new Color(1f, 1f, 1f, 0.92f);

        private static readonly Color legendaryColor = new Color(1f, 0.8f, 0f, 1f);

        private float _timeLeft;
        private Color _targetColor;
        private Color _currentColor;

        private TextMeshProUGUI _cardTextObject;
        private List<Image> _images;
        private List<Component> _ballImageObjects;
        private HashSet<Image> _triangleImages;

        private void Awake()
        {
            var generalObject = gameObject;
            if (generalObject.transform.parent != null)
            {
                generalObject = transform.parent.gameObject;
            }

            _cardTextObject = CardController.FindObjectInChildren(generalObject, "Text_Name").GetComponent<TextMeshProUGUI>(); //this.gameObject.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponentInChildren(typeof(TextMeshProUGUI), true));
            _images = generalObject.GetComponentsInChildren<Image>(true).Where(x => x.gameObject.name != "Background" && x.gameObject.transform.parent.name == "Front" || x.gameObject.transform.parent.name == "Back" || x.gameObject.transform.parent.name.Contains("EdgePart") || x.gameObject.transform.name.Contains("FRAME")).ToList();

            foreach (var frameObject in CardController.FindObjectsInChildren(generalObject, "FRAME", true))
            {
                frameObject.SetActive(false);
            }

            _ballImageObjects = new List<Component>();
            foreach (GameObject ballObject in CardController.FindObjectsInChildren(generalObject, "SmallBall", false))
            {
                _ballImageObjects.AddRange(ballObject.gameObject.GetComponentsInChildren(typeof(ProceduralImage), true).Where(x => x.gameObject.transform.name.Contains("SmallBall")).ToList());
            }

            var edgesObject = CardController.FindObjectInChildren(generalObject, "Edges");
            if (edgesObject != null)
            {
                SetEdgeInAnimation(edgesObject, 0, new Vector3(230, -970));
                SetEdgeInAnimation(edgesObject, 1, new Vector3(-115, -735));
                SetEdgeInAnimation(edgesObject, 2, new Vector3(115, -735));
                SetEdgeInAnimation(edgesObject, 3, new Vector3(-230, -970));
            }

            _triangleImages = new HashSet<Image>();
            var triangleObjects = CardController.FindObjectsInChildren(generalObject, "Triangle", true);
            if (triangleObjects != null)
            {
                foreach (var triangleObject in triangleObjects)
                {
                    var triangleObjectImage = triangleObject.GetComponent<Image>();
                    if (triangleObjectImage != null)
                    {
                        _triangleImages.Add(triangleObjectImage);
                    }
                }
            }

            var _extraTextObj = new GameObject("ExtraCardText", typeof(TextMeshProUGUI));
            RectTransform[] allChildrenRecursive = generalObject.GetComponentsInChildren<RectTransform>();
            GameObject BottomLeftCorner = allChildrenRecursive.Where(obj => obj.gameObject.name == "EdgePart (1)").FirstOrDefault().gameObject;
            GameObject creatorNameObject = Instantiate(_extraTextObj, BottomLeftCorner.transform.position, BottomLeftCorner.transform.rotation, BottomLeftCorner.transform);
            creatorNameObject.transform.Rotate(0f, 0f, 135f);
            creatorNameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            creatorNameObject.transform.localPosition = new Vector3(-50f, -50f, 0f);

            TextMeshProUGUI creatorName = creatorNameObject.GetComponent<TextMeshProUGUI>();
            creatorName.text = "otDan";
            creatorName.enableWordWrapping = false;
            creatorName.alignment = TextAlignmentOptions.Bottom;
            creatorName.alpha = 1f;
            creatorName.fontSize = 70;
            creatorName.color = new Color(0.5f, 0f, 1f, 1f);
        }

        private void Update()
        {
            if (gameObject.activeInHierarchy)
            {
                if (_timeLeft <= Time.deltaTime)
                {
                    CardColorChange(_targetColor);
                    _timeLeft = 1f;

                    if (_targetColor == firstColor)
                    {
                        _targetColor = secondColor;
                        return;
                    }

                    _targetColor = firstColor;
                }
                else
                {
                    CardColorChange(Color.Lerp(_currentColor, _targetColor, Time.deltaTime / _timeLeft));
                    _timeLeft -= Time.deltaTime;
                }

                if (gameObject.GetComponent<Legendary>() != null)
                {
                    foreach (var triangleImage in _triangleImages)
                    {
                        triangleImage.color = legendaryColor;
                    }
                }
            }
        }

        private void CardColorChange(Color newColor)
        {
            _cardTextObject.color = textColor;

            foreach (Image image in _images)
            {
                if (image != null)
                {
                    image.color = newColor;
                }
            }

            foreach (ProceduralImage image in _ballImageObjects)
            {
                if (image != null)
                {
                    image.color = triangleColor;
                }
            }

            _currentColor = newColor;
        }

        private static void SetEdgeInAnimation(GameObject edgesObject, int child, Vector3 position)
        {
            var edgeObject = edgesObject.transform.GetChild(child);
            var triangleObject = edgeObject.GetChild(3);
            var animObject = triangleObject.GetChild(0);
            var edgeAnimation = animObject.gameObject.GetComponent<CurveAnimation>();
            edgeAnimation.animations[0].animDirection = position;
            edgeAnimation.animations[0].speed = 3f;
            edgeAnimation.animations[0].inCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
    }
}
