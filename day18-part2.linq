<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"

private static Plane<char> Board;
private static int TotalKeys;

struct State {
	public int x0; public int y0;
	public int x1; public int y1;
	public int x2; public int y2;
	public int x3; public int y3;
	public int active;
	
    public int lastkeybits;
	public int keybits;
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

    private bool HasKey(char c) => (this.keybits >> ((c | 32) - 'a') & 1) == 1;

	public IEnumerable<State> GetNext() {
		var ch = Board[this[active].x, this[active].y];
		for (int i = 0; i < 4; i++) {
			if (i != active && this.lastkeybits == this.keybits) continue;

			var (x, y) = this[i];
			(int x, int y)[] cand = { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };

			foreach (var c in cand) {
                char tch = Board[c.x, c.y];
                if (tch == '#') continue;
		        if (tch >= 'A' && tch <= 'Z' && !HasKey(tch)) continue;
            
				State next = this;
                next.lastkeybits = this.keybits;
				next.active = i;
				next[i] = (c.x, c.y);
				next.moves += 1;
				if (tch >= 'a' && tch <= 'z' && !HasKey(tch)) {
                    next.keybits |= 1 << tch - 'a';
					next.keycount += 1;
				}
				yield return next;
			}
		}
	}
}  

(int x, int y) GetStart() {
	for (int y = 0; y < Board.Height; y++) {
		for (int x = 0; x < Board.Width; x++) {
			if (Board[x,y] == '@') return (x, y);
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
	Board[x,y] = '#';
	Board[x, y - 1] = Board[x, y + 1] = Board[x - 1, y] = Board[x + 1, y] = '#';
	Board[x - 1, y - 1] = Board[x - 1, y + 1] = Board[x + 1, y - 1] = Board[x + 1, y + 1] = '@';
    Board.Dump();

	var startState = new State { 
		x0 = x-1, y0 = y-1, 
		x1 = x+1, y1 = y-1, 
		x2 = x-1, y2 = y+1, 
		x3 = x+1, y3 = y+1,
	};
	
	var frontier = new Queue<State>();
	for (int i = 0; i < 4; i++) {
		startState.active = i;
		frontier.Enqueue(startState);
	}

	var seen = new HashSet<(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3, int keyset)>();
	
	int mostKeysSeen = 0;
	var mostContainer = new DumpContainer(mostKeysSeen).Dump("Most Keys Seen");
	var frontierSize = new DumpContainer().Dump("Frontier Size");
	var seenSize = new DumpContainer().Dump("Seen size");
	var movesContainer = new DumpContainer().Dump("Moves depth");

    while (frontier.Any()) {
		var curr = frontier.Dequeue();
		var mapkey = (curr.x0, curr.y0, curr.x1, curr.y1, curr.x2, curr.y2, curr.x3, curr.y3, curr.keybits);
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
        
		foreach (var e in curr.GetNext()) frontier.Enqueue(e);
	}
}