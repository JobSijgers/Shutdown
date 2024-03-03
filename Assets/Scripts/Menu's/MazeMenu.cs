using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class MazeMenu : MonoBehaviour
{
    public bool isTracing = false;
    
    [SerializeField, Range(0, 1)] private float dotChance = 0.4f;
    [SerializeField, Range(0, 1)] private float breakChance = 0.4f;
    [SerializeField] private float minDistance = 15;
    [SerializeField] private bool isReplayable = false;
    [SerializeField] private float checkTime = 1.5f;

    [Space(30)]
    [SerializeField] private Vector2Int endPos = new(4, 2);
    [SerializeField] private Vector2Int startPos = new Vector2Int(0, 2);
    [SerializeField] private int width, height;
    [SerializeField] private Vector3 topLeftPos;
    [SerializeField] private float pointOffset;

    [Space(30)]
    [SerializeField] private GameObject cross;
    [SerializeField] private GameObject endCross;
    [SerializeField] private GameObject hexagon;
    [SerializeField] private GameObject line;

    [Space(30)]
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject player;
    [SerializeField] private LaserGate gate;
    [SerializeField] private Transform tilesParent;
    [SerializeField] private RectTransform button;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private GameObject gameParent;
    [SerializeField] private Image checkCircle;

    private Vector3[,] points;
    private GameObject[,] tiles;
    private List<GameObject> lineParts = new();
    private List<Vector2Int> tracedPoints = new();
    private List<Vector2Int> routePoints = new();
    private List<Vector2Int> dots = new();

    float imageWidth;
    float imageHeight;
    

    private Vector2Int lastPointIndex;
    private Coroutine inputCoroutine;
    private Coroutine gameCoroutine;



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
            if (collision.layer != player.layer || collision == null) break;

            if (gameCoroutine == null && Input.GetKeyDown(KeyCode.E))
            {
                menu.SetActive(true);
                GameManager.Instance.PauseMovement(true);
                gameCoroutine = StartCoroutine(MazeGame());
            }
            yield return null;
        }
        inputCoroutine = null;
    }





    private IEnumerator MazeGame()
    {
        tilesParent.gameObject.SetActive(false);
        button.gameObject.SetActive(false);
        RunSetup();

        lastPointIndex = startPos;
        tracedPoints.Add(lastPointIndex);
        routePoints.Add(lastPointIndex);

        loadingText.SetActive(true);

        yield return GenerateMaze();

        loadingText.SetActive(false);
        tilesParent.gameObject.SetActive(true);
        button.gameObject.SetActive(true);

        yield return WaitForButton();

        yield return PlayMaze();

        // remove all tiles
        for (int i = 0; i < tilesParent.childCount; i++)
        {
            Destroy(tilesParent.GetChild(i).gameObject);
        }

        yield return ShowCheck();

        tracedPoints.Clear();
        routePoints.Clear();
        lineParts.Clear();
        dots.Clear();

        gate.CompleteMaze();
        menu.SetActive(false);
        GameManager.Instance.PauseMovement(false);
        gameCoroutine = null;
        if (!isReplayable)
        { Destroy(this); }
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


    private void RunSetup()
    {
        points = new Vector3[height, width];
        tiles = new GameObject[height, width];

        Vector2 goalPos = startPos + new Vector2((width) * pointOffset, (height) * pointOffset);
        imageWidth = (goalPos.x - startPos.x) / width;
        imageHeight = (goalPos.y - startPos.y) / height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 currentPos = topLeftPos;
                currentPos.x += pointOffset * x;
                currentPos.y -= pointOffset * y;
                points[y, x] = currentPos;
                GameObject image;
                if (new Vector2(x, y) != endPos)
                {
                    image = Instantiate(cross, currentPos, Quaternion.Euler(Vector3.zero), tilesParent);
                }
                else
                {
                    image = Instantiate(endCross, currentPos, Quaternion.Euler(Vector3.zero), tilesParent);
                }
                
                image.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(imageWidth, imageHeight);
                tiles[y, x] = image;
            }
        }
        
        button.sizeDelta = new Vector2(imageWidth, imageHeight);
        button.position = points[startPos.y, startPos.x];
    }



    private IEnumerator GenerateMaze()
    {
        while (routePoints[^1] != endPos || routePoints.Count < minDistance)
        {
            Vector2Int nextPos = routePoints[^1];
            nextPos = PickDirection(nextPos);

            if (routePoints.Contains(nextPos))
            {
                int index = routePoints.IndexOf(nextPos);
                routePoints.RemoveRange(index, routePoints.Count - index);
            }
            
            routePoints.Add(nextPos);
            yield return null;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (routePoints.Contains(new Vector2Int(x, y)))
                {
                    if (!(Random.Range(0f, 1f) <= dotChance)) continue;
                    
                    dots.Add(new Vector2Int(x, y));
                    Vector3 position = points[y, x];
                    position += new Vector3(width/2f, height/2f, 0);
                    Instantiate(hexagon, position, Quaternion.Euler(Vector3.zero), tilesParent);
                }
                else
                {
                    if (!(Random.Range(0f, 1f) <= breakChance)) continue;
                    
                    Destroy(tiles[y, x]);
                    points[y, x] = Vector3.zero;
                }
            }
        }
    }


    private IEnumerator WaitForButton()
    {
        button.gameObject.SetActive(true);
        
        while (true)
        {
            if (isTracing)
            {
                isTracing = false;
                break;
            }
            yield return null;
        }
    }

    public void GetButtonInput()
    {
        isTracing = true;
        button.gameObject.SetActive(false);
    }
    
    private IEnumerator PlayMaze()
    {
        while (tracedPoints[^1] != endPos || !HasAllDots())
        {
            GetNearestPoint();
            yield return new WaitForSeconds(0.1f);
        }
    }



    private void GetNearestPoint() 
    {
        float shortestDistance = float.MaxValue;
        Vector2Int nearestPointIndex = Vector2Int.zero;

        int yMinValue = Mathf.Clamp(lastPointIndex.y - 1, 0, height - 1);
        int yMaxValue = Mathf.Clamp(lastPointIndex.y + 1, 0, height - 1);

        int xMinValue = Mathf.Clamp(lastPointIndex.x - 1, 0, width - 1);
        int xMaxValue = Mathf.Clamp(lastPointIndex.x + 1, 0, width - 1);

        for (int y = yMinValue; y <= yMaxValue; y++)
        {
            for (int x = xMinValue; x <= xMaxValue; x++)
            {
                float distance = Vector3.Distance(Input.mousePosition, points[y, x]);
                Vector2Int index = new Vector2Int(x, y);
                
                if (!(distance < shortestDistance)) continue;

                if (points[index.y, index.x] == Vector3.zero) continue;
                
                if ((x == lastPointIndex.x - 1 || x == lastPointIndex.x + 1) && 
                    (y == lastPointIndex.y - 1 || y == lastPointIndex.y + 1)) continue;

                shortestDistance = distance;
                nearestPointIndex = index;
            }
        }

        if (nearestPointIndex == tracedPoints[^1]) return;

        if (tracedPoints.Count >= 2 && nearestPointIndex == tracedPoints[^2])
        {
            lastPointIndex = tracedPoints[^2];
            Destroy(lineParts[^1]);
            lineParts.RemoveAt(lineParts.Count - 1);
            tracedPoints.RemoveAt(tracedPoints.Count - 1);
            return;
        }

        if (tracedPoints.Contains(nearestPointIndex)) return;

        Vector3 rotation = Vector3.zero;
        if (nearestPointIndex.y != tracedPoints[^1].y)
        { rotation.z = 90; }

        Vector3 position = Vector3.Lerp(points[lastPointIndex.y, lastPointIndex.x], points[nearestPointIndex.y, nearestPointIndex.x], 0.5f);
        position += new Vector3(imageWidth/50f, imageHeight/50f, 0);
        GameObject image = Instantiate(line, position, Quaternion.Euler(rotation), tilesParent);
        image.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(imageWidth * 2, imageHeight);
        lineParts.Add(image);

        lastPointIndex = nearestPointIndex;
        tracedPoints.Add(nearestPointIndex);
    }


    private Vector2Int PickDirection(Vector2Int currentPos)
    {
        Vector2Int[] directions =
        {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0)
        };

        directions = directions.OrderBy(_ => Random.value).ToArray();
        foreach (Vector2Int direction in directions)
        {
            Vector2Int newPosition = currentPos + direction;
            if (newPosition.x < 0 || newPosition.x >= width ||
                newPosition.y < 0 || newPosition.y >= height ||
                (routePoints.Count >= 2 && newPosition == routePoints[^2]))
                continue;
            return newPosition;

        }
        
        Debug.LogError("Failed to pick a direction");
        return Vector2Int.zero;
    }

    private bool HasAllDots()
    {
        foreach (Vector2Int dot in dots)
        {
            if (!tracedPoints.Contains(dot)) return false;
        }
        return true;
    }

    
    private void OnDrawGizmos()
    {
        if (gameCoroutine == null) return;

        Gizmos.color = Color.red;
        foreach (Vector2Int pos in routePoints)
        {
            Gizmos.DrawSphere(points[pos.y, pos.x], 45);
        }


        Gizmos.color = Color.white;
        foreach (Vector3 pos in points)
        {
            Gizmos.DrawSphere(pos, 30);
        }

        Gizmos.color = new Color32(255, 100, 0, 255);
        foreach (Vector2Int pos in tracedPoints)
        {
            Gizmos.DrawSphere(points[pos.y, pos.x], 30f);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(points[tracedPoints[^1].y, tracedPoints[^1].x], 35f);

    }

}
