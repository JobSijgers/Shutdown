using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WireMenu : MonoBehaviour
{
    [SerializeField] private bool isReplayable = false;
    [SerializeField] private float checkTime = 1.5f;
    [SerializeField] private float minDistance = 5;
    [SerializeField] private AnimationCurve curve;
    [Space(10)]
    [SerializeField] private RectTransform limeStart;
    [SerializeField] private RectTransform redStart;
    [SerializeField] private RectTransform purpleStart;
    [SerializeField] private RectTransform yellowStart;
    [Space(10)]
    [SerializeField] private RectTransform limeEnd;
    [SerializeField] private RectTransform redEnd;
    [SerializeField] private RectTransform purpleEnd;
    [SerializeField] private RectTransform yellowEnd;
    [Space(10)]
    [SerializeField] private GameObject limeWire;
    [SerializeField] private GameObject redWire;
    [SerializeField] private GameObject purpleWire;
    [SerializeField] private GameObject yellowWire;
    [Space(10)] 
    [SerializeField] private GameObject[] buttons = new GameObject[4];
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject player;
    [SerializeField] private BombTimer bomb;
    [SerializeField] private GameObject gameParent;
    [SerializeField] private Image checkCircle;

    private Vector3[] endPositions;
    private GameObject[] drawnWires = new GameObject[4];

    private Vector3 limeStartPos;
    private Vector3 redStartPos;
    private Vector3 purpleStartPos;
    private Vector3 yellowStartPos;
    
    private Vector3 limeEndPos;
    private Vector3 redEndPos;
    private Vector3 purpleEndPos;
    private Vector3 yellowEndPos;

    private Coroutine gameCoroutine;
    private Coroutine inputCoroutine;


    private void OnTriggerEnter(Collider other)
    {
        inputCoroutine = StartCoroutine(WaitForInteraction(other.gameObject));
    }

    private void OnTriggerExit(Collider other)
    {
        if (inputCoroutine != null)
        {
            StopCoroutine(inputCoroutine);
            inputCoroutine = null;
        }
    }

    private IEnumerator WaitForInteraction(GameObject collision)
    {
        while (true)
        {
            if (collision.layer != player.layer) break;

            if (gameCoroutine == null && Input.GetKeyDown(KeyCode.E))
            {
                menu.SetActive(true);
                GameManager.Instance.PauseMovement(true);
                gameCoroutine = StartCoroutine(WireGame());
            }
            yield return null;
        }
        inputCoroutine = null;
    }


    private IEnumerator WireGame()
    {
        RunSetup();
        while (!HasAllWires())
        {
            yield return null;
        }

        foreach (GameObject button in buttons)
        {
            button.SetActive(true);
        }

        foreach (GameObject wire in drawnWires)
        {
            Destroy(wire);
        }


        bomb.StopTimer();

        yield return ShowCheck();

        menu.SetActive(false);
        GameManager.Instance.PauseMovement(false);
        gameCoroutine = null;
        if (!isReplayable)
        { Destroy(this); }
    }
    
    
    private void RunSetup()
    {
        RectTransform[] endLines = { limeEnd, redEnd, purpleEnd, yellowEnd };
        Vector3[] oldEndLines = { limeEnd.position, redEnd.position, purpleEnd.position, yellowEnd.position };
        endLines = endLines.OrderBy(_ => Random.value).ToArray();

        for (int i = 0; i < endLines.Length; i++)
        {
            endLines[i].position = oldEndLines[i];
        }
        
        
        limeStartPos = SetPos(limeStart, false);
        redStartPos = SetPos(redStart, false);
        purpleStartPos = SetPos(purpleStart, false);
        yellowStartPos = SetPos(yellowStart, false);
        
        limeEndPos = SetPos(limeEnd, true);
        redEndPos = SetPos(redEnd, true);
        purpleEndPos = SetPos(purpleEnd, true);
        yellowEndPos = SetPos(yellowEnd, true);

        endPositions = new Vector3[4]
        {
            limeEndPos, redEndPos, purpleEndPos, yellowEndPos
        };
    }

    private Vector3 SetPos(RectTransform wireTransform, bool flipSide)
    {
        Vector3 pos = wireTransform.position;
        float multiplier = flipSide ? -1 : 1;
        pos.x += wireTransform.sizeDelta.x / 2 * multiplier;
        return pos;
    }

    public void SelectLimeWire(GameObject usedButton)
    {
        StartCoroutine(DragWire(limeWire, limeStartPos, limeEndPos, usedButton, 0));
    }
    
    public void SelectRedWire(GameObject usedButton)
    {
        StartCoroutine(DragWire(redWire, redStartPos, redEndPos, usedButton, 1));
    }
    
    public void SelectPurpleWire(GameObject usedButton)
    {
        StartCoroutine(DragWire(purpleWire, purpleStartPos, purpleEndPos, usedButton, 2));
    }

    public void SelectYellowWire(GameObject usedButton)
    {
        StartCoroutine(DragWire(yellowWire, yellowStartPos, yellowEndPos, usedButton, 3));
    }
    
    
    
    private IEnumerator DragWire(GameObject wire, Vector3 startPos, Vector3 endPos, GameObject button, int wireIndex)
    {
        Destroy(drawnWires[wireIndex]); 
        GameObject wireObj = Instantiate(wire, button.transform.parent);
        Image wireSprite = wireObj.GetComponent<Image>();
        while (Input.GetMouseButton(0))
        {
            StretchImage(wireObj, wireSprite, startPos, Input.mousePosition);
            yield return null;
        }

        Vector3 position = GetNearestWire();
        if (position != Vector3.zero /* && possi*/)
        {
            StretchImage(wireObj, wireSprite, startPos, position);
            drawnWires[wireIndex] = wireObj;
            if (position == endPos)
            {
                button.SetActive(false);
            }
        }
        else
        {
            Destroy(wireObj);
        }
    }

    private Vector3 GetNearestWire()
    {
        foreach (Vector3 pos in endPositions)
        {
            if (Vector3.Distance(Input.mousePosition, pos) < minDistance) return pos;
        }
        return Vector3.zero;
    }
        
        
    private void StretchImage(GameObject spriteObj, Image sprite, Vector3 pos1, Vector3 pos2)
    {
        Transform spriteTransform = spriteObj.transform;
        Vector3 centerPos = Vector3.Lerp(pos1, pos2, 0.5f);
        spriteTransform.position = centerPos;
        Vector3 direction = pos2 - pos1;
        direction = Vector3.Normalize(direction);
        spriteTransform.right = direction;
        float rectHeight = sprite.rectTransform.sizeDelta.y;
        float distance = Vector3.Distance(pos1, pos2);
        sprite.rectTransform.sizeDelta = new Vector2(distance, rectHeight);
    }

    private bool HasAllWires()
    {
        foreach (GameObject button in buttons)
        {
            if (button.activeSelf) return false;
        }
        return true;
    }

    private IEnumerator ShowCheck()
    {
        gameParent.SetActive(false);
        Transform circleObject = checkCircle.transform;
        circleObject.parent.gameObject.SetActive(true); // really messy but I can't be bothered 

        Quaternion startRotation = circleObject.rotation;
        Vector3 endVector = startRotation.eulerAngles;
        endVector.z += 180;
        Quaternion endRotation = Quaternion.Euler(endVector);

        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / checkTime;
            checkCircle.fillAmount = curve.Evaluate(Mathf.Clamp(progress * 2, 0, 1));
            circleObject.rotation = Quaternion.Lerp(startRotation, endRotation, curve.Evaluate(progress));
            yield return null;
        }
    }


}
