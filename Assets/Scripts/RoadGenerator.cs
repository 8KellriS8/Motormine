using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    [Header("Road Prefabs")]
    [SerializeField] private GameObject straightRoad;
    [SerializeField] private GameObject leftTurn;
    [SerializeField] private GameObject rightTurn;

    [Header("Generation")]
    [SerializeField] private Transform startPoint;

    private const int SegmentCount = 10;

    // Текущая точка, откуда создаём следующий сегмент
    private Transform currentEnd;

    // Сколько прямых дорог подряд уже создано
    private int straightCount = 0;

    // Последний тип поворота:
    // 0 = нет
    // 1 = Left
    // 2 = Right
    private int lastTurn = 0;

    // Сколько раз подряд встретился один и тот же поворот
    private int sameTurnCount = 0;

    private void Start()
    {
        currentEnd = startPoint;

        GenerateRoad();
    }

    private void GenerateRoad()
    {
        for (int i = 0; i < SegmentCount; i++)
        {
            bool isLastSegment = i == SegmentCount - 1;

            GameObject roadPrefab = ChooseRoadPiece(isLastSegment);

            GameObject road = Instantiate(
                roadPrefab,
                Vector3.zero,
                Quaternion.identity,
                transform
            );

            // Находим точки нового сегмента
            Transform newStart = road.transform.Find("StartPoint");
            Transform newEnd = road.transform.Find("EndPoint");

            if (newStart == null || newEnd == null)
            {
                Debug.LogError(
                    $"У префаба {road.name} нет StartPoint или EndPoint!"
                );

                Destroy(road);
                return;
            }

            // Совмещаем StartPoint нового сегмента
            // с текущим EndPoint
            AlignRoad(road.transform, newStart, currentEnd);

            // Новый EndPoint становится точкой
            // для следующего сегмента
            currentEnd = newEnd;
        }
    }

    private GameObject ChooseRoadPiece(bool isLastSegment)
    {
        // Последний сегмент всегда поворот
        if (isLastSegment)
        {
            return ChooseTurn();
        }

        // Если уже 5 прямых подряд —
        // следующий обязательно поворот
        if (straightCount >= 5)
        {
            return ChooseTurn();
        }

        // 30% шанс поворота
        if (Random.value < 0.3f)
        {
            return ChooseTurn();
        }

        // Иначе прямая
        straightCount++;

        return straightRoad;
    }

    private GameObject ChooseTurn()
    {
        // После поворота сбрасываем количество прямых
        straightCount = 0;

        int turn;

        // Если один и тот же поворот уже был 2 раза подряд,
        // третий такой же запрещён
        if (sameTurnCount >= 1)
        {
            turn = lastTurn == 1 ? 2 : 1;
        }
        else
        {
            // Случайный левый или правый
            turn = Random.Range(0, 2) == 0 ? 1 : 2;
        }

        // Обновляем информацию о поворотах
        if (turn == lastTurn)
        {
            sameTurnCount++;
        }
        else
        {
            lastTurn = turn;
            sameTurnCount = 1;
        }

        if (turn == 1)
        {
            return leftTurn;
        }

        return rightTurn;
    }

    private void AlignRoad(
        Transform road,
        Transform start,
        Transform target
    )
    {
        // Сначала поворачиваем весь сегмент так,
        // чтобы его StartPoint смотрел так же,
        // как текущий EndPoint
        Quaternion rotationOffset =
            target.rotation * Quaternion.Inverse(start.rotation);

        road.rotation =
            rotationOffset * road.rotation;

        // После поворота перемещаем StartPoint
        // точно в позицию EndPoint предыдущего сегмента
        Vector3 positionOffset =
            target.position - start.position;

        road.position += positionOffset;
    }
}