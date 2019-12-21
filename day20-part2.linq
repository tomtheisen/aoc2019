<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

public struct State {
	public DepthPosition Position;
	public int Moves;
	
	public State(Point position, int depth = 0, int moves = 0) {
		Position = new DepthPosition(position, depth);
		Moves = moves;
	}
}

public struct DepthPosition : IEquatable<DepthPosition> {
	public Point Position;
	public int Depth;
	
	public DepthPosition(Point position, int depth = 0) {
		Position = position;
		Depth = depth;
	}

	public override int GetHashCode() => Position.X + Position.Y * 987 + Depth * 98765;
	public bool Equals(DepthPosition other)
		=> Position.X == other.Position.X && Position.Y == other.Position.Y && Depth == other.Depth;
}

class RecursivePlutoMaze : BreadthFirst<State, DepthPosition> {
	private DepthPosition Target;
	private Dictionary<Point, Point> Warps = new Dictionary<Point, Point>();
	private Plane<char> Plane;

	private bool IsLetter(int x, int y) => Plane[x, y] is char c && c >= 'A' && c <= 'Z';

	private bool IsOuter(DepthPosition pos) => pos.Position is var (x, y) 
		&& (x == 2 || y == 2 || x == Plane.Width - 3 || y == Plane.Height - 3);

	public RecursivePlutoMaze(Plane<char> plane) {
		Plane = plane;

		var spots = new List<(string name, Point position)>();
		for (int x = 0; x < plane.Width; x++) {
			for (int y = 0; y < plane.Height; y++) {
				if (plane[x, y] == '.') {
					if (IsLetter(x - 1, y) && IsLetter(x - 2, y))
						spots.Add(("" + plane[x - 2, y] + plane[x - 1, y], new Point(x, y)));
					else if (IsLetter(x, y - 1) && IsLetter(x, y - 2))
						spots.Add(("" + plane[x, y - 2] + plane[x, y - 1], new Point(x, y)));
					else if (IsLetter(x + 1, y) && IsLetter(x + 2, y))
						spots.Add(("" + plane[x + 1, y] + plane[x + 2, y], new Point(x, y)));
					else if (IsLetter(x, y + 1) && IsLetter(x, y + 2))
						spots.Add(("" + plane[x, y + 1] + plane[x, y + 2], new Point(x, y)));
				}
			}
		}

		var nameLookup = spots.ToLookup(s => s.name, s => s.position);

		foreach (var g in nameLookup) {
			if (g.Count() != 2) continue;
			Warps[g.First()] = g.Last();
			Warps[g.Last()] = g.First();
		}

		Target = new DepthPosition(nameLookup["ZZ"].Single());
		var start = new State(nameLookup["AA"].Single());
		Frontier.Enqueue(start);
	}

	protected override DepthPosition GetKey(State state) => state.Position;

	protected override bool IsGoal(State state) => state.Position.Equals(Target);

	protected override IEnumerable<State> NextStates(State state) {
		if (Warps.TryGetValue(state.Position.Position, out var t)) {
			if (!IsOuter(state.Position)) yield return new State(t, state.Position.Depth + 1, state.Moves + 1);
			else if (state.Position.Depth > 0) yield return new State(t, state.Position.Depth - 1, state.Moves + 1);
		}

		foreach (var next in state.Position.Position.Neighbors)
			if (Plane[next] == '.')
				yield return new State(next, state.Position.Depth, state.Moves + 1);
	}
}

void Main() {
	var maze = new RecursivePlutoMaze(GetAocCharPlane());
	maze.Search().Moves.Dump();
}