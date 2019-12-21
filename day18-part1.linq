<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
</Query>

#load ".\helpers.linq"

struct StateKey : IEquatable<StateKey> {
	public Point Position;
	public int keyset;

	public override int GetHashCode() => Position.GetHashCode() ^ keyset;
	public bool Equals([AllowNull] StateKey other)
		=> keyset == other.keyset && Position.Equals(other.Position);
}

struct State {
	public Point Position;
	public int keyset;
	public int keycount;
	public int moves;
}  

class NeptuneVault : BreadthFirst<State, StateKey> {
	private int TotalKeys;
	private Plane<char> Board;

	protected override StateKey GetKey(State state) 
		=> new StateKey { Position = state.Position, keyset = state.keyset };
	
	protected override bool IsGoal(State state) 
		=> state.keycount == TotalKeys;

	protected override IEnumerable<State> NextStates(State state) {
		foreach (var n in state.Position.Neighbors) {
			var ch = Board[n];
			if (ch == '#') continue;
			bool haveKey = (state.keyset >> (char.ToLower(ch) - 'a') & 1) == 1;
			if (ch >= 'A' && ch <= 'Z' && !haveKey) continue;
			bool isNewKey = ch >= 'a' && ch <= 'z' && !haveKey;

			yield return new State {
				Position = n,
				keyset = isNewKey
					? state.keyset | (1 << ch - 'a')
					: state.keyset,
				keycount = state.keycount + (isNewKey ? 1 : 0),
				moves = state.moves + 1,
			};
		}
	}

	private void SealDeadEnds() {
		while (true) {
			bool didwork = false;
			for (int x = 1; x < Board.Width - 1; x++) {
				for (int y = 1; y < Board.Height - 1; y++) {
					var pt = new Point(x, y);
					
					bool dead = (Board[pt] == '.' || Board[pt] >= 'A' && Board[pt] <= 'Z')
						&& pt.Neighbors.Select(p => Board[p]).Count(c => c == '#') >= 3;
					if (dead) Board[pt] = '#';
					didwork |= dead;
				}
			}
			if (!didwork) break;
		}
	}

	public NeptuneVault(Plane<char> board) {
		Board = board;
		TotalKeys = board.Values.Count(c => c >= 'a' && c <= 'z');
		SealDeadEnds();
		var start = Board.Find(c => c == '@').Single();
		Frontier.Enqueue(new State { Position = start });
	}
}

void Main() {
	var maze = new NeptuneVault(GetAocCharPlane())
		//.Dump()
		;
	maze.Search().Dump(1);
}
