using GameEnhancementCards.Util;
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
        private static readonly Color FirstColor = new Color(1f, 0f, 0.914f, 0.5f);
        private static readonly Color SecondColor = new Color(1f, 0f, 0.014f, 1f);
        private static readonly Color TriangleColor = new Color(0.90f, 0.90f, 0.90f, 0.75f);
        private static readonly Color TextColor = new Color(1f, 1f, 1f, 0.92f);

        private static readonly Color LegendaryColor = new Color(1f, 0.8f, 0f, 1f);

        private float _timeLeft;
        private Color _targetColor;
        private Color _currentColor;

        private TextMeshProUGUI _cardTextObject;
        private List<Image> _images;
        private List<Component> _ballImageObjects;
        private HashSet<Image> _rarityImages;
        private HashSet<Image> _triangleImages;

        private bool _legendary = true;

        private void Awake()
        {
            var generalObject = gameObject;
            if (generalObject.transform.parent != null)
            {
                generalObject = transform.parent.gameObject;
            }

            _cardTextObject = CardController.FindObjectInChildren(generalObject, "Text_Name")
                .GetComponent<
                    TextMeshProUGUI>(); //this.gameObject.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponentInChildren(typeof(TextMeshProUGUI), true));
            _images = generalObject.GetComponentsInChildren<Image>(true).Where(x =>
            {
                GameObject foundGameObject = x.gameObject;
                Transform gameObjectTransform = foundGameObject.transform;
                Transform gameObjectParentTransform = gameObjectTransform.parent;
                return foundGameObject.name != "Background" && gameObjectParentTransform.name == "Front" ||
                       gameObjectParentTransform.name == "Back" ||
                       gameObjectTransform.name.Contains("FRAME");
            }).ToList();

            foreach (var frameObject in CardController.FindObjectsInChildren(generalObject, "FRAME", true))
            {
                frameObject.SetActive(false);
            }

            _ballImageObjects = new List<Component>();
            foreach (GameObject ballObject in CardController.FindObjectsInChildren(generalObject, "SmallBall", false))
            {
                _ballImageObjects.AddRange(ballObject.gameObject.GetComponentsInChildren(typeof(ProceduralImage), true)
                    .Where(x => x.gameObject.transform.name.Contains("SmallBall")).ToList());
            }

            var edgesObject = CardController.FindObjectInChildren(generalObject, "Edges");
            if (edgesObject != null)
            {
                SetEdgeInAnimation(edgesObject, 0, new Vector3(230, -970));
                SetEdgeInAnimation(edgesObject, 1, new Vector3(-115, -735));
                SetEdgeInAnimation(edgesObject, 2, new Vector3(115, -735));
                SetEdgeInAnimation(edgesObject, 3, new Vector3(-230, -970));
            }

            _rarityImages = new HashSet<Image>();
            var triangleObjects = CardController.FindObjectsInChildren(generalObject, "Triangle", true);
            if (triangleObjects != null)
            {
                foreach (var triangleObjectImage in triangleObjects.Select(triangleObject => triangleObject.GetComponent<Image>()).Where(triangleObjectImage => triangleObjectImage != null))
                {
                    _rarityImages.Add(triangleObjectImage);
                }
            }

            _triangleImages = new HashSet<Image>();
            foreach (var triangleImage in _images.Where(triangleImage => triangleImage.transform.parent != null).Where(triangleImage => triangleImage.transform.parent.name == "Triangle"))
            {
                // UnityEngine.Debug.Log($"Triangle found: {triangleImage.transform.parent.name}");
                _triangleImages.Add(triangleImage);
            }

            var extraTextObj = new GameObject("ExtraCardText", typeof(TextMeshProUGUI));
            RectTransform[] allChildrenRecursive = generalObject.GetComponentsInChildren<RectTransform>();
            GameObject bottomLeftCorner = allChildrenRecursive
                .FirstOrDefault(obj => obj.gameObject.name == "EdgePart (1)")?.gameObject;
            if (bottomLeftCorner != null)
            {
                GameObject creatorNameObject = Instantiate(extraTextObj, bottomLeftCorner.transform.position,
                    bottomLeftCorner.transform.rotation, bottomLeftCorner.transform);
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

            if (gameObject.GetComponent<Legendary>() == null)
            {
                _legendary = false;
            }
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;

            if (_timeLeft <= Time.deltaTime)
            {
                CardColorChange(_targetColor);
                _timeLeft = 1f;

                if (_targetColor == FirstColor)
                {
                    _targetColor = SecondColor;
                    return;
                }

                _targetColor = FirstColor;
            }
            else
            {
                CardColorChange(Color.Lerp(_currentColor, _targetColor, Time.deltaTime / _timeLeft));
                _timeLeft -= Time.deltaTime;
            }

            if (!_legendary) return;

            foreach (var triangleImage in _rarityImages)
            {
                triangleImage.color = LegendaryColor;
            }
        }

        private void CardColorChange(Color newColor)
        {
            _cardTextObject.color = TextColor;

            foreach (Image image in _images)
            {
                if (image != null)
                {
                    image.color = newColor;
                }
            }

            foreach (var image in _ballImageObjects.Cast<ProceduralImage>().Where(image => image != null))
            {
                image.color = TriangleColor;
            }

            foreach (Image triangleImage in _triangleImages)
            {
                triangleImage.color = TriangleColor;
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
