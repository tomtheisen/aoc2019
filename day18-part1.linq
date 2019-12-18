<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"

private static Plane<char> Board;
private static int TotalKeys;

struct State {
	public int x;
	public int y;
	public int keyset;
	public int keycount;
	public int moves;
	
	public IEnumerable<State> GetNext() {
		(int x, int y)[] cand = {(x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)};
		
		foreach (var c in cand) {
			var ch = Board[c.x, c.y];
			if (ch == '#') continue;
			bool haveKey = (this.keyset >> (char.ToLower(ch) - 'a') & 1) == 1;
			if (ch >= 'A' && ch <= 'Z' && !haveKey) continue;
			bool isNewKey = ch >= 'a' && ch <= 'z' && !haveKey;
			
			yield return new State {
				x = c.x,
				y = c.y,
				keyset = isNewKey
					? this.keyset | (1 << ch - 'a')
					: this.keyset,
				keycount = isNewKey ? this.keycount + 1 : this.keycount,
				moves = this.moves + 1,
			};
		}
	}
}  

(int x, int y) GetStart() {
	for (int y = 0; y < Board.Height; y++) {
		for (int x = 0; x < Board.Width; x++) {
			if (Board[x, y] == '@') return (x, y);
		}
	}
	throw new Exception();
}

void SealDeadEnds() {
	while (true) {
		bool didwork = false;
		for (int x = 1; x < Board.Width - 1; x++) {
			for (int y = 1; y < Board.Height - 1; y++) {
				bool dead =  (Board[x, y] == '.' || Board[x, y] >= 'A' && Board[x, y] <= 'Z')
					&& ("" + Board[x, y - 1] + Board[x, y + 1] + Board[x - 1, y] + Board[x + 1, y]).Count(c => c == '#') >= 3;
				if (dead) Board[x, y] = '#';
				didwork |= dead;
			}
		}
		if (!didwork) break;
	}
}

void Main() {
    Board = GetAocCharPlane();
    TotalKeys = GetAocInput().RegexFindAll("[a-z]").Count;
	SealDeadEnds();
	var (x, y) = GetStart();
    Board.Dump();

    var startState = new State{ x = x, y = y, };
	
	var frontier = new Queue<State>();
	frontier.Enqueue(startState);
	var seen = new HashSet<(int x, int y, int keyset)>();
	
	int mostKeysSeen = 0;
	var mostContainer = new DumpContainer(mostKeysSeen).Dump("Most Keys Seen");
	var frontierSize = new DumpContainer().Dump("Frontier Size");
	var seenSize = new DumpContainer().Dump("Seen size");
	var movesContainer = new DumpContainer().Dump("Moves depth");
	
	while (frontier.Any()) {
		var curr = frontier.Dequeue();
        if (seen.Contains((curr.x, curr.y, curr.keyset))) continue;
		seen.Add((curr.x, curr.y, curr.keyset));
		
		mostContainer.Content = mostKeysSeen = Max(mostKeysSeen, curr.keycount);

		frontierSize.Content = frontier.Count;
		seenSize.Content = seen.Count;
		movesContainer.Content = curr.moves;
		
		if (curr.keycount == TotalKeys) {
			curr.Dump();
			return;
		}
		
		foreach (var e in curr.GetNext()) frontier.Enqueue(e);
	}
}
