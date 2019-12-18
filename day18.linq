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
static UserQuery() {
	
}

struct State {
	public int x0;
	public int y0;
	public int x1;
	public int y1;
	public int x2;
	public int y2;
	public int x3;
	public int y3;
	public int active;
	
	public int keyset;
	public int keycount;
	public int moves;
	
	public (int x, int y) this[int i]{
		get => i switch { 0 => (x0, y0), 1 => (x1, y1), 2 => (x2, y2), 3 => (x3, y3) };
		set {
			switch (i) {
				case 0: (x0, y0) = value; break;
				case 1: (x1, y1) = value; break;
				case 2: (x2, y2) = value; break;
				case 3: (x3, y3) = value; break;
			}
		}
	}

	public IEnumerable<State> GetNext() {
		var ch = Board[this[active].y][this[active].x];
		if (ch == '#') yield break;
		bool haveKey = (this.keyset >> (char.ToLower(ch) - 'a') & 1) == 1;
		if (ch >= 'A' && ch <= 'Z' && !haveKey) yield break;
		bool isNewKey = ch >= 'a' && ch <= 'z' && !haveKey;

		for (int i = 0; i < 4; i++) {
			if (i != active && !isNewKey) continue;

			var (x, y) = this[i];
			(int x, int y)[] cand = { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };

			foreach (var c in cand) {
				State next = this;
				next[i] = (c.x, c.y);
				next.moves += 1;
				if (isNewKey) {
					next.active = i;
					next.keyset |= 1 << ch - 'a';
					next.keycount += 1;
				}
				yield return next;
			}
		}
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
	var (x, y) = GetStart();
	Board[y][x] = '#';
	Board[y - 1][x] = Board[y + 1][x] = Board[y][x - 1] = Board[y][x + 1] = '#';
	Board[y - 1][x - 1] = Board[y + 1][x - 1] = Board[y - 1][x + 1] = Board[y + 1][x + 1] = '@';
	
	foreach (var row in Board) WriteLine(string.Concat(row));

	var startState = new State{ 
		x0 = x-1, y0 = y-1, 
		x1 = x-1, y1 = y+1, 
		x2 = x+1, y2 = y-1, 
		x3 = x+1, y3 = y+1, 
	};
	
	var frontier = new Queue<State>();
	for (int i = 0; i < 4; i++) {
		startState.active = i;
		frontier.Enqueue(startState);
	}

	var seen = new HashSet<(int active, int x, int y, int keyset)>();
	
	int mostKeysSeen = 0;
	var mostContainer = new DumpContainer(mostKeysSeen).Dump("Most Keys Seen");
	var frontierSize = new DumpContainer().Dump("Frontier Size");
	var seenSize = new DumpContainer().Dump("Seen size");
	var movesContainer = new DumpContainer().Dump("Moves depth");
	
	while (frontier.Any()) {
		var curr = frontier.Dequeue();

		var mapkey = (curr.active, curr[curr.active].x, curr[curr.active].y, curr.keyset);
		if (seen.Contains(mapkey)) continue;
		seen.Add(mapkey);
		
		mostContainer.Content = mostKeysSeen = Max(mostKeysSeen, curr.keycount);
		frontierSize.Content = frontier.Count;
		seenSize.Content = seen.Count;
		movesContainer.Content = curr.moves;
		
		if (curr.keycount == TotalKeys) {
			curr.Dump();
			return;
		}
		
		foreach (var e in curr.GetNext()) {
			frontier.Enqueue(e);			
		}
	}
}