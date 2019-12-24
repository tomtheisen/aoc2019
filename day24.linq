<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"

{ // part 1 
	Plane<char> MakeEmpty() => new Plane<char>('.');

	var board = GetAocCharPlane();
	var seenbio = new HashSet<int>();
	
	for (int gen = 0; ; gen++) {
		//board.Dump();
		//Console.WriteLine();
	
		var next = MakeEmpty();
		
		int bio = 0;
		for (int x = 0; x < board.Width; x++) {
			for (int y = 0; y < board.Height; y++) {
				var pos = new Point(x, y);
				int adjacentbugs = 0;
				foreach (var neighbor in pos.Neighbors) {
					if (board[neighbor] == '#') adjacentbugs += 1;
				}
				if (board[pos] == '#') {
					bio |= 1 << (y * 5) + x;
					next[pos] = adjacentbugs == 1 ? '#' : '.';
				}
				else {
					next[pos] = adjacentbugs == 1 || adjacentbugs == 2 ? '#' : '.';
				}
			}
		}
		
		if (seenbio.Contains(bio)) {
			WriteLine(bio);
			break;
		}
		seenbio.Add(bio);
		
		board = next;
	}	
}

{ // part 2
	Plane<char> MakeEmpty() {
		var result = new Plane<char>('.');
		result[2, 2] = '?';
		return result;
	}
	int BugCount(char c) => c == '#' ? 1 : 0;
	
	var boards = new List<Plane<char>> { GetAocCharPlane() };
	
	for (int gen = 0; gen < 200; gen++) {
		boards.Dump($"Gen {gen}", 0);
	
		boards.Add(MakeEmpty());
		boards.Add(MakeEmpty());
		boards.Insert(0, MakeEmpty());
		boards.Insert(0, MakeEmpty());
		
		var next = new List<Plane<char>>();
		
		for (int depth = 1; depth < boards.Count - 1; depth++) {
			var newboard = MakeEmpty();
			next.Add(newboard);
			
			for (int x = 0; x < 5; x++) {
				for (int y = 0; y < 5; y++) {
					if (x == 2 && y == 2) continue;
					var pos = new Point(x, y);
					int adjacentbugs = 0;
					foreach (var neighbor in pos.Neighbors)
						adjacentbugs += BugCount(boards[depth][neighbor]);
					if (x == 0)
						adjacentbugs += BugCount(boards[depth - 1][1, 2]);
					if (x == 4)
						adjacentbugs += BugCount(boards[depth - 1][3, 2]);
					if (y == 0)
						adjacentbugs += BugCount(boards[depth - 1][2, 1]);
					if (y == 4)
						adjacentbugs += BugCount(boards[depth - 1][2, 3]);
					var deeper = boards[depth + 1];
					if (x == 2 && y == 1)
						adjacentbugs +=
							+BugCount(deeper[0, 0])
							+ BugCount(deeper[1, 0])
							+ BugCount(deeper[2, 0])
							+ BugCount(deeper[3, 0])
							+ BugCount(deeper[4, 0]);
					if (x == 2 && y == 3)
						adjacentbugs +=
							+BugCount(deeper[0, 4])
							+ BugCount(deeper[1, 4])
							+ BugCount(deeper[2, 4])
							+ BugCount(deeper[3, 4])
							+ BugCount(deeper[4, 4]);
					if (x == 1 && y == 2)
						adjacentbugs +=
							+BugCount(deeper[0, 0])
							+ BugCount(deeper[0, 1])
							+ BugCount(deeper[0, 2])
							+ BugCount(deeper[0, 3])
							+ BugCount(deeper[0, 4]);
					if (x == 3 && y == 2)
						adjacentbugs +=
							+BugCount(deeper[4, 0])
							+ BugCount(deeper[4, 1])
							+ BugCount(deeper[4, 2])
							+ BugCount(deeper[4, 3])
							+ BugCount(deeper[4, 4]);
					
					if (boards[depth][pos] == '#')
						newboard[pos] = adjacentbugs == 1 ? '#' : '.';
					if (boards[depth][pos] == '.')
						newboard[pos] = adjacentbugs == 1 || adjacentbugs == 2 ? '#' : '.';
				}
			}
		}
	
		while (next[0].Values.Sum(BugCount) == 0) next.RemoveAt(0);
		while (next[^1].Values.Sum(BugCount) == 0) next.RemoveAt(^1);
		boards = next;
	}
	
	WriteLine(boards.SelectMany(b => b.Values).Sum(BugCount));
}