<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

const bool animate = true;

enum Direction { North = 1, South = 2, West = 3, East = 4 }
Direction[] AllDirections = { Direction.North, Direction.South, Direction.West, Direction.East };

Direction Left(Direction d) => d switch {
	Direction.North => Direction.West,
	Direction.West => Direction.South,
	Direction.South => Direction.East,
	Direction.East => Direction.North,
	_ => throw new ArgumentOutOfRangeException(),
};
Direction Right(Direction d) => d switch {
	Direction.North => Direction.East,
	Direction.East => Direction.South,
	Direction.South => Direction.West,
	Direction.West => Direction.North,
	_ => throw new ArgumentOutOfRangeException(),
};

struct Position {
	public int X;
	public int Y;
	public Position(int x, int y) => (X, Y) = (x, y);
	public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);
	public Position[] GetNeighbors() => new[] {
		new Position(X, Y - 1),
		new Position(X, Y + 1),
		new Position(X - 1, Y),
		new Position(X + 1, Y),
	};
	
	public Position GetNeighbor(Direction d) => d switch {
		Direction.North => new Position(X, Y - 1),
		Direction.South => new Position(X, Y + 1),
		Direction.West => new Position(X - 1, Y),
		Direction.East => new Position(X + 1, Y),
		_ => throw new ArgumentOutOfRangeException(),
	};
	
	public static readonly Position Origin = default;
	
	public override string ToString() => $"({X}, {Y})";
}

void Main() {
 	// part 1
	var steps = new Dictionary<Position, int> {[Position.Origin] = 0};
	var machine = new IntCodeMachine();
	var plane = new Plane<char>(' ') { [0, 0] = 'D' };
	
	var planeContainer = animate ? new DumpContainer(plane).Dump() : null;
	Position pos = Position.Origin, oxpos = default;
	
	Direction? GetMove() {
		var visited = new HashSet<Position> { pos };
		var frontier = new Queue<(Direction firstStep, Position position)>();

		foreach (var dir in AllDirections)
			frontier.Enqueue((dir, pos.GetNeighbor(dir)));
			
		while (frontier.Any()) {
			var (firstStep, pos) = frontier.Dequeue();
			if (plane[pos.X, pos.Y] == '#' || visited.Contains(pos)) continue;
			visited.Add(pos);
			if (plane[pos.X, pos.Y] == ' ') return firstStep;
			foreach (var dir in AllDirections)
				frontier.Enqueue((firstStep, pos.GetNeighbor(dir)));
		}
		return null;
	}
	
	while (true) {
		var dir = GetMove();
		if (dir == null) break;
		machine.TakeInput((int)dir);
		Position lastPos = pos, target = pos.GetNeighbor(dir.Value);
		switch (machine.GetOutput() ?? throw new Exception("halted?")) {
			case 0: // failed to move
				plane[target.X, target.Y] = '#';
				break;
			case 1: // regular move
				if (plane[pos.X, pos.Y] == 'D') plane[pos.X, pos.Y] = '.';
				pos = target;
				plane[pos.X, pos.Y] = 'D';
				if (!steps.ContainsKey(pos)) steps[pos] = steps[lastPos] + 1;
				break;
			case 2: // moved to o2
				plane[pos.X, pos.Y] = '.';
				plane[target.X, target.Y] = 'O';
				oxpos = pos = target;
				if (!steps.ContainsKey(pos))
					WriteLine($"Oxygen@{pos} steps:{ steps[pos] = steps[lastPos] + 1 }");
				break;
		}
		
		planeContainer?.Refresh();
		if (animate) Thread.Sleep(10);
	}

	// part 2
	plane[pos.X, pos.Y] = plane[oxpos.X, oxpos.Y] = '.';
	int minutes = -2;
	for (List<Position> frontier = new List<Position>{ oxpos }, newFrontier; frontier.Any(); minutes++) {
		newFrontier = new List<Position>();
		foreach (var fpos in frontier) {
			if (plane[fpos.X, fpos.Y] != '.') continue;
			plane[fpos.X, fpos.Y] = 'O';
			foreach (var dir in AllDirections)
				newFrontier.Add(fpos.GetNeighbor(dir));
		}
		frontier = newFrontier;
		planeContainer?.Refresh();
		if (animate) Thread.Sleep(10);
	}
	WriteLine($"Total O2 in {minutes} minutes");
}