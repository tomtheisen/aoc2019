<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var board = new Dictionary<(int x, int y), int>();
int maxBlocks = 0, blocks = 0, ballx = 0, padx = 0;

var score = new DumpContainer(0).Dump("Score");
var blocksContainer = new DumpContainer().Dump("Blocks");
var boardContainer = new DumpContainer().Dump("Board");
void ShowBoard() {
	var height = board.Keys.Max(k => k.y) + 1;
	var width = board.Keys.Max(k => k.x) + 1;
	var result = Enumerable.Range(0, height).Select(_ => new string(' ',width).ToCharArray()).ToArray();
	foreach (var ((x,y),t) in board) result[y][x] = " ▓◘═○"[t];
	boardContainer.Content = string.Join('\n', result.Select(line => string.Concat(line)));
    Thread.Sleep(150);
}

var machine = new IntCodeMachine { NextInput = () => Sign(ballx - padx)};
machine.Poke(0, 2);
while (true) {
	var x = machine.GetOutput();
	var y = machine.GetOutput();
	var tile = machine.GetOutput();
	if (tile == null) break;

	var pos = ((int)x, (int)y);
	if (pos == (-1, 0)) {
		score.Content = tile;
		continue;
	}
	switch ((int)tile) {
		case 0: {
			if (board.TryGetValue(pos, out var exist) && exist == 2) blocksContainer.Content = $"{--blocks} / {maxBlocks}";
			break;
		}
		case 2: {
			if (!board.TryGetValue(pos, out var exist) || exist != 2) blocksContainer.Content = $"{++blocks} / {maxBlocks = Max(blocks, maxBlocks)}";
			break;
		}
		case 3:
			padx = (int)x;
	        board[pos] = 3;
            ShowBoard();
			break;
		case 4:
			ballx = (int)x;
			break;
	}
	board[pos] = (int)tile;
}
ShowBoard();
