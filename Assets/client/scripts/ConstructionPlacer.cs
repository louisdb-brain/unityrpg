using UnityEngine;

public class ConstructionPlacer : MonoBehaviour
{
    public static ConstructionPlacer Instance;
    public GameObject prefab;
    public GameObject basePrefab;
    private GameObject _ghost;
    private GameObject _toPlacePrefab;

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    public void StartPlacing(Sprite image,bool isBase)
    {
        CancelPlacing();
        if (isBase)
        {
            _toPlacePrefab = basePrefab;
        }
        else
        {
            _toPlacePrefab = prefab;
        }
        _ghost = Instantiate(prefab);
        _ghost.GetComponent<SpriteRenderer>().sprite = image;
        _toPlacePrefab.GetComponent<SpriteRenderer>().sprite=image;
        MakeTransparent(_ghost);
    }

    void Update()
    {
        if (_ghost == null) return;

        if (TryGetBuildboardHit(out Vector3 hitPoint))
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _ghost.transform.position = hitPoint;
            }
            else
            {
                _ghost.transform.position = Snap(hitPoint);
            }

            if (Input.GetMouseButtonDown(0)) // Left click to confirm
            {
                Instantiate(_toPlacePrefab, _ghost.transform.position, Quaternion.identity);
                CancelPlacing();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            CancelPlacing();
    }

    private bool TryGetBuildboardHit(out Vector3 point)
    {
        Debug.Log("hitting the raytrace");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            if (hit.collider.CompareTag("buildboard"))
            {
                point = hit.point;
                return true;
            }
        }

        point = Vector3.zero;
        return false;
    }

    private Vector3 Snap(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x),
            Mathf.Round(pos.y),
            Mathf.Round(pos.z)
        );
    }

    private void MakeTransparent(GameObject go)
    {
        foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>())
        {
            var color = sr.color;
            color.a = 0.5f;
            sr.color = color;
        }
    }

    private void CancelPlacing()
    {
        if (_ghost) Destroy(_ghost);
        _ghost = null;
        _toPlacePrefab = null;
    }
}