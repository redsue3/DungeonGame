using System.Collections.Generic;
using System.Linq;

// 층(floor)별로 나뉜 노드들을 간선(next)으로 연결한 분기형 던전 맵.
// 예전엔 9x7 네모 그리드에 인접칸 이동이었지만, 지금은 실제로 이어진 노드로만 이동 가능하다.
public class DungeonMap
{
    public int Layer      { get; }
    public int FloorCount { get; }
    public List<MapNode> Nodes { get; } = new List<MapNode>();

    // -1 = 아직 입장 전. 0층 노드 중 하나를 선택해야 런이 시작된다.
    public int CurrentNodeId { get; private set; } = -1;

    public DungeonMap(int layer, int floorCount)
    {
        Layer      = layer;
        FloorCount = floorCount;
    }

    public MapNode GetNode(int id) => Nodes.FirstOrDefault(n => n.id == id);
    public MapNode CurrentNode     => CurrentNodeId >= 0 ? GetNode(CurrentNodeId) : null;

    public IEnumerable<MapNode> NodesOnFloor(int floor) => Nodes.Where(n => n.floor == floor);

    // 지금 위치에서 다음으로 갈 수 있는 노드들 (입장 전이면 0층 전체가 선택지)
    public IEnumerable<MapNode> ReachableNodes()
        => CurrentNodeId < 0 ? NodesOnFloor(0) : CurrentNode.next.Select(GetNode);

    public bool CanMoveTo(int nodeId) => ReachableNodes().Any(n => n.id == nodeId);

    public MapNode TryMoveTo(int nodeId)
    {
        if (!CanMoveTo(nodeId)) return null;
        CurrentNodeId = nodeId;
        return CurrentNode;
    }
}
