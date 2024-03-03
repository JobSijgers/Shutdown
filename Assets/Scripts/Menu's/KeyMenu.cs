using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyMenu : MonoBehaviour
{
    [SerializeField] private bool isReplayable = false;
    [SerializeField] private float checkTime = 1.5f;
    [SerializeField] private int moveCount = 10;
    [SerializeField] private float moveTime = 0.8f;
    [SerializeField] private float moveDelay = 1;
    [SerializeField] private float glowTime = 1;
    [Tooltip("Sets the minimum number of what movement types can be chosen - shouldn't be changed aside from bugfixing")]
    [SerializeField] private int minMovementOption;
    [Tooltip("Sets the maximum (+1) number of what movement types can be chosen - shouldn't be changed aside from bugfixing")]
    [SerializeField] private int maxMovementOption;
    [SerializeField] private float gizmoSize;
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private float arcMultiplier;
    [SerializeField] private AnimationCurve arcCurve;
    [SerializeField] private AnimationCurve glowCurve;
    [SerializeField] private AnimationCurve checkCurve;
    [SerializeField] private Transform keysParent;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject player;
    [SerializeField] private LaserGate gate;
    [SerializeField] private Image checkCircle;

    [SerializeField] private Transform[] keys = new Transform[8];  // serializefield isn't necessary, but it's handy to have easily visible
    private Vector3[] targetPositions = new Vector3[8];

    private bool checkButtons = false;
    private Coroutine inputCoroutine;
    private Coroutine gameCoroutine;
    private Transform correctKey;
    private int up = 1; // 1 is normal, -1 is flipped


    void Start()
    {
        int currentKey = 0;
        for (int i = 0; keys[7] == null; i++) // assigns each of the keys (in order) to the keys[] array. Could be made with a serializefield but I prefer this personally
        {
            Transform child = keysParent.GetChild(i);
            if (child.CompareTag("Key"))
            {
                keys[currentKey] = child;
                currentKey++;
            }
            if (i > 12) break; // failsafe
        }
    }


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
                gameCoroutine = StartCoroutine(LIMBO());
            }
            yield return null;
        }
        inputCoroutine = null;
    }


    /*
                   .
                  / \\
                 /   \ \
                /     \' \
               /       \'' \
              /         \ ' '\
             /           \''   \
            /             \ ' '  \
           /               \   ''  \
          /     • _ •       \' ' ' ' \
         /                   \' ' '  ' \
        /                     \' '   ' ' \
       /                       \  '   ' /
      /                         \   ' /
     /                           \'  /
    /_____________________________\/
            bottom text
    */

    private IEnumerator LIMBO()
    {
        up = 1;
        int n = 0;
        foreach (Transform key in keys)
        {
            targetPositions[n] = key.position; // saves the positions for movements (if I take the current position of the target key itself, they get offset over time)
            n++;
        }
        yield return new WaitForSeconds(moveDelay);
        int keyNum = Random.Range(0, 8);
        correctKey = keys[keyNum];
        for (int i = 0; i < 8; i++)
        {
            keys[i].GetComponent<Button>().onClick.RemoveAllListeners();
            bool selectKey = (i == keyNum);
            keys[i].GetComponent<Button>().onClick.AddListener(() => SelectKey(selectKey));
        }
        yield return ShowKey(correctKey);

        for (int i = 0; i < moveCount; i++)
        {
            switch(Random.Range(minMovementOption, maxMovementOption)) // select a movement type
            {
                case 1:
                    DiagonalSwap();
                    yield return new WaitForSeconds(moveTime);
                    break;

                case 2:
                    SquareRotate();
                    yield return new WaitForSeconds(moveTime);
                    (keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7]) = // swaps the transforms so that the top left key is in keys[0], top right in keys[1], etc
                    (keys[2], keys[0], keys[3], keys[1], keys[6], keys[4], keys[7], keys[5]);
                    break;

                case 3:
                    SquareSwap();
                    yield return new WaitForSeconds(moveTime);
                    (keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7]) = 
                    (keys[4], keys[5], keys[6], keys[7], keys[0], keys[1], keys[2], keys[3]);
                    break;

                case 4:
                    StackUp();
                    yield return new WaitForSeconds(moveTime);
                    (keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7]) = 
                    (keys[2], keys[3], keys[4], keys[5], keys[6], keys[7], keys[0], keys[1]);
                    break;

                case 5:
                    StartCoroutine(FlipScreen());
                    yield return new WaitForSeconds(moveTime * 2.5f);
                    (targetPositions[0], targetPositions[1], targetPositions[2], targetPositions[3], targetPositions[4], targetPositions[5], targetPositions[6], targetPositions[7]) =
                    (targetPositions[7], targetPositions[6], targetPositions[5], targetPositions[4], targetPositions[3], targetPositions[2], targetPositions[1], targetPositions[0]);
                    break;

                case 6:
                    FullRotate();
                    yield return new WaitForSeconds(moveTime);
                    (keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7]) =
                    (keys[2], keys[0], keys[4], keys[1], keys[6], keys[3], keys[7], keys[5]);
                    break;

                case 7:
                    InvertSquare();
                    yield return new WaitForSeconds(moveTime);
                    break;


                default:
                    Debug.LogError("random selector error");
                    break;
            }
            yield return new WaitForSeconds(moveDelay);
        }
        checkButtons = true; // start waiting for the player to click on a key
    }


    /// <summary>
    /// Displays which key is the correct one by turning it green
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private IEnumerator ShowKey(Transform key)
    {
        Image image = key.GetComponent<Image>();
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / glowTime;
            image.color = new Color(1, 1, 1, glowCurve.Evaluate(progress)); // lowers the opacity, showing the green key behind (not achievable by changing the image colour as that is additive)
            yield return null;
        }
    }

    private void SelectKey(bool pickedCorrectly)
    {
        if (checkButtons)
        { 
            StartCoroutine(EndGame(pickedCorrectly));
            checkButtons = false;
        }
    }

    private IEnumerator EndGame(bool pickedCorrectly)
    {
        if (pickedCorrectly)
        {
            yield return ShowCheck();
            gate.CompleteKeys(); // flag the minigame as completed
            if (!isReplayable)
            { Destroy(this); }
        }
        else
        {
            yield return ShowKey(correctKey);
        }
        gameCoroutine = null;
        menu.SetActive(false);

        if (keysParent.rotation.eulerAngles.z != 0)
        {
            keysParent.rotation = Quaternion.Euler(Vector3.zero);
            up = 1;
        }
        GameManager.Instance.PauseMovement(false);
    }


    private IEnumerator ShowCheck()
    {
        keysParent.gameObject.SetActive(false);
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
            checkCircle.fillAmount = checkCurve.Evaluate(Mathf.Clamp(progress * 2, 0, 1));
            circleObject.rotation = Quaternion.Lerp(startRotation, endRotation, checkCurve.Evaluate(progress));
            yield return null;
        }
    }


    private void DiagonalSwap()
    {
        StartCoroutine(SwapKeys(1, 2));
        StartCoroutine(SwapKeys(3, 4));
        StartCoroutine(SwapKeys(5, 6));
    }

    private void SquareRotate()
    {
        StartCoroutine(MoveKey(0, 1));
        StartCoroutine(MoveKey(1, 3));
        StartCoroutine(MoveKey(3, 2));
        StartCoroutine(MoveKey(2, 0));

        StartCoroutine(MoveKey(4, 5));
        StartCoroutine(MoveKey(5, 7));
        StartCoroutine(MoveKey(7, 6));
        StartCoroutine(MoveKey(6, 4));
    }
    
    private void SquareSwap()
    {
        StartCoroutine(MoveKey(0, 4));
        StartCoroutine(MoveKey(1, 5));
        StartCoroutine(MoveKey(2, 6));
        StartCoroutine(MoveKey(3, 7));

        StartCoroutine(MoveKeyCurved(4, 0, -1));
        StartCoroutine(MoveKeyCurved(5, 1, -1));
        StartCoroutine(MoveKeyCurved(6, 2, -1));
        StartCoroutine(MoveKeyCurved(7, 3, -1));
    }

    private void StackUp()
    {
        StartCoroutine(MoveKey(7, 5));
        StartCoroutine(MoveKey(6, 4));
        StartCoroutine(MoveKey(5, 3));
        StartCoroutine(MoveKey(4, 2));
        StartCoroutine(MoveKey(3, 1));
        StartCoroutine(MoveKey(2, 0));

        StartCoroutine(MoveKeyCurved(1, 7, 0.8f * up));
        StartCoroutine(MoveKeyCurved(0, 6, -0.8f * up));
    }

    private void FullRotate()
    {
        StartCoroutine(MoveKey(0, 1));
        StartCoroutine(MoveKey(1, 3));
        StartCoroutine(MoveKey(3, 5));
        StartCoroutine(MoveKey(5, 7));
        StartCoroutine(MoveKey(7, 6));
        StartCoroutine(MoveKey(6, 4));
        StartCoroutine(MoveKey(4, 2));
        StartCoroutine(MoveKey(2, 0));
    }

    private void InvertSquare()
    {
        StartCoroutine(SwapKeys(0, 3));
        StartCoroutine(SwapKeys(1, 2));
        StartCoroutine(SwapKeys(4, 7));
        StartCoroutine(SwapKeys(5, 6));
    }


    /// <summary>
    /// Moves a key from position a to position b
    /// </summary>
    /// <param name="index"></param>
    /// <param name="targetIndex"></param>
    /// <returns></returns>
    private IEnumerator MoveKey(int index, int targetIndex)
    {
        Vector3 startPos = targetPositions[index];
        Vector3 endPos = targetPositions[targetIndex];
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / moveTime;
            keys[index].position = Vector3.Lerp(startPos, endPos, speedCurve.Evaluate(progress));
            yield return null;
        }
    }

    /// <summary>
    /// Swaps the positions of two keys and also swaps their index
    /// </summary>
    /// <param name="index1"></param>
    /// <param name="index2"></param>
    /// <returns></returns>
    private IEnumerator SwapKeys(int index1, int index2)
    {
        Vector3 pos1 = targetPositions[index1];
        Vector3 pos2 = targetPositions[index2];
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / moveTime;
            keys[index1].position = Vector3.Lerp(pos1, pos2, speedCurve.Evaluate(progress));
            keys[index2].position = Vector3.Lerp(pos2, pos1, speedCurve.Evaluate(progress));
            yield return null;
        }
        (keys[index1], keys[index2]) = (keys[index2], keys[index1]);
    }


    /// <summary>
    /// Moves a key from position a to position b in an arc, which is multiplied by a value (negative numbers make it go the other way)
    /// </summary>
    /// <param name="index"></param>
    /// <param name="targetIndex"></param>
    /// <param name="multiplier"></param>
    /// <returns></returns>
    private IEnumerator MoveKeyCurved(int index, int targetIndex, float multiplier)
    {
        Vector3 startPos = targetPositions[index];
        Vector3 endPos = targetPositions[targetIndex];
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / moveTime;
            float arcDistance = arcCurve.Evaluate(progress) * multiplier * arcMultiplier;
            Vector3 arc = new Vector3(arcDistance, 0, 0);
            keys[index].position = Vector3.Lerp(startPos, endPos, speedCurve.Evaluate(progress)) + arc;
            yield return null;
        }
    }

    private IEnumerator FlipScreen()
    {
        Quaternion startRotation = keysParent.transform.rotation;
        Vector3 rot = startRotation.eulerAngles;
        rot.z += 180;
        Quaternion endRotation = Quaternion.Euler(rot);
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / (moveTime * 2.5f);
            keysParent.rotation = Quaternion.Lerp(startRotation, endRotation, speedCurve.Evaluate(progress));
            yield return null;
        }
        up *= -1;
    }


    private void OnDrawGizmos()
    {
        if (gameCoroutine != null)
        {
            int i = 0;
            foreach (Transform key in keys)
            {
                Gizmos.color = Color.HSVToRGB((float)i * 1/8, 1, 1); // set a different colour for each of the eight keys by dividing the hue circle in eight parts
                Gizmos.DrawSphere(key.position, gizmoSize); // 
                Gizmos.DrawSphere(targetPositions[i] + new Vector3(gizmoSize * 2, 0, 0), gizmoSize * 0.6f);
                i++;
            }
        }
    }
}
