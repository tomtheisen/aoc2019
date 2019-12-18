<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

private static char[][] Board = GetAocLines().Select(l => l.ToCharArray()).ToArray();
private static int Width = Board[0].Length;
private static int Height = Board.Length;
private static int TotalKeys = GetAocInput().RegexFindAll("[a-z]").Count;

struct State {
	public int x;
	public int y;
	public string keys;
	public int moves;
	
	public IEnumerable<State> GetNext() {
		(int x, int y)[] cand = {(x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)};
		
		var self = this;
		return from c in cand
			let ch = Board[c.y][c.x]
			where ch != '#'
			where ch < 'A' || ch > 'Z' || self.keys.Contains(ch, StringComparison.InvariantCultureIgnoreCase)
			let isNewKey = ch >= 'a' && ch <= 'z' && !self.keys.Contains(ch)
			select new State { 
				x = c.x, y = c.y, 
				keys = isNewKey ? self.keys + ch : self.keys,
				moves = self.moves + 1,
			};
	}
}  

(int x, int y) GetStart() {
	for (int y = 0; y < Height; y++) {
		for (int x = 0; x < Width; x++) {
			if (Board[y][x] == '@') return (x, y);
		}
	}
	throw new Exception();
}

void SealDeadEnds() {
	while (true) {
		bool didwork = false;
		for (int x = 1; x < Width - 1; x++) {
			for (int y = 1; y < Height - 1; y++) {
				bool dead =  (Board[y][x] == '.' || Board[y][x] >= 'A' && Board[y][x] <= 'Z')
					&& ("" + Board[y - 1][x] + Board[y + 1][x] + Board[y][x - 1] + Board[y][x + 1]).Count(c => c == '#') >= 3;
				if (dead) Board[y][x] = '#';
				didwork |= dead;
			}
		}
		if (!didwork) break;
	}
}

void Main() {
	SealDeadEnds();
	foreach (var row in Board) WriteLine(string.Concat(row));
	var (x, y) = GetStart();
	var startState = new State{ x = x, y = y, keys = "" };
	
	var frontier = new Queue<State>();
	frontier.Enqueue(startState);
	var seen = new HashSet<(int x, int y, string keys)>();
	
	int mostKeysSeen = 0;
	var mostContainer = new DumpContainer(mostKeysSeen).Dump("Most Keys Seen");
	var frontierSize = new DumpContainer().Dump("Frontier Size");
	var seenSize = new DumpContainer().Dump("Seen size");
	var movesContainer = new DumpContainer().Dump("Moves depth");
	
	while (frontier.Any()) {
		var curr = frontier.Dequeue();

		if (seen.Contains((curr.x, curr.y, curr.keys))) continue;
		seen.Add((curr.x, curr.y, curr.keys));
		
		//experimental pruning
		if (curr.keys.Length < mostKeysSeen - 1) continue;

		mostContainer.Content = mostKeysSeen = Max(mostKeysSeen, curr.keys.Length);
		frontierSize.Content = frontier.Count;
		seenSize.Content = seen.Count;
		movesContainer.Content = curr.moves;
		
		if (curr.keys.Length == TotalKeys) {
			curr.Dump();
			return;
		}
		
		foreach (var e in curr.GetNext()) {
			frontier.Enqueue(e);			
		}
	}
}
