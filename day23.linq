<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var machines = new List<IntCodeMachine>();

// how many times has this machine requested non-existent input
bool[] underflows = new bool[50];
for (int i = 0; i < 50; i++) {
	int i_ = i;
	var machine = new IntCodeMachine { 
		NextInput = () => {
			underflows[i_] = true;
			return -1;
		},
		BlockOnInput = false,
	};
	machine.TakeInput(i);
	machines.Add(machine);
}

var sw = Stopwatch.StartNew();
var cluster = new IntCodeCluster(machines);
long? natx = null, naty = null;
int tick = 0;
for (int i = 0; i < 50; i++) {
	var machine = cluster[i];
	int i_ = i;
	int address = 0, tosend = 0;
	long x=0, y=0;
	machine.Outputting += input => {
		Array.Clear(underflows, 0, underflows.Length);
		switch (tosend) {
			case 0:
				tosend = 2;
				address = (int)input;
				break;
			case 2:
				tosend = 1;
				x = input;
				break;
			case 1:
				tosend = 0;
				y = input;
				WriteLine($"[{tick}: {sw.Elapsed}] From {i_} to {address} X={x} Y={y}");

				if (address == 255) (natx, naty) = (x, y);
				else machines[address].TakeInput(x, y);
				break;
		}
	};
}

long deliveredy = long.MinValue;
for (; ; tick++) {
	cluster.Tick();
	if (underflows.Any(u => !u) || natx == null) continue;

	WriteLine($"[{tick}: {sw.Elapsed}] Idle network: From nat to 0 X={natx} Y={naty}");

	cluster[0].TakeInput(natx.Value, naty!.Value);
	Array.Clear(underflows, 0, underflows.Length);

	if (naty == deliveredy) {
		WriteLine(naty);
		break;
	}
	deliveredy = naty.Value;
}
