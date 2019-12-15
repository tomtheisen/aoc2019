<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

enum Direction { North = 1, South = 2, West = 3, East = 4 }
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
	var steps = new Dictionary<Position, int> {[Position.Origin] = 0};
	var machine = new IntCodeMachine();
	var plane = new Plane<char>(' ') { [0, 0] = 'D' };
	
	var planeContainer = new DumpContainer(plane).Dump();
	var pos = Position.Origin;
	var dir = Direction.North;
	var oxygen = new HashSet<Position>();
	
	for (int staleness = 0; staleness < 5_000; staleness++) {
		machine.TakeInput((int)dir);
		Position lastPos = pos, target = pos.GetNeighbor(dir);
		switch (machine.GetOutput()!) {
			case 0: // failed to move
				plane[target.X, target.Y] = '#';
				dir = Right(dir);
				break;
			case 1: // regular move
				if (plane[pos.X, pos.Y] == ' ') staleness = 0;
				if (plane[pos.X, pos.Y] == 'D') plane[pos.X, pos.Y] = '.';
				pos = target;
				plane[pos.X, pos.Y] = 'D';
				if (!steps.ContainsKey(pos)) steps[pos] = steps[lastPos] + 1;
				dir = Left(dir);
				break;
			case 2: // moved to o2
				plane[pos.X, pos.Y] = '.';
				plane[target.X, target.Y] = 'O';
				oxygen.Add(pos = target);
				if (!steps.ContainsKey(pos))
					WriteLine($"Oxygen {pos} steps { steps[pos] = steps[lastPos] + 1 }");
				break;
		}
		planeContainer.Refresh();
		// Thread.Sleep(1); // for sweet animation
	}
	plane[pos.X, pos.Y] = '.';
	
	int minutes = -1, startCount;
	do {
		var neighbors = (
			from o in oxygen
			from n in o.GetNeighbors()
			where plane[n.X, n.Y] == '.'
			select n
		).ToList();
			
		startCount = oxygen.Count;
		oxygen.UnionWith(neighbors);
		foreach (var (nx, ny) in oxygen) plane[nx, ny] = 'O';	
		minutes += 1;
		planeContainer.Refresh();
	} while (oxygen.Count > startCount);
	minutes.Dump();
}