<Query Kind="Program" />

void Main() {	
}

public class IntCodeMachine {
	public event Action<int> Outputting;
	public event Action<int> UnhandledInput;

	public int[] InitialState { get; }
	public bool Terminated { get; private set; } = true;
	public string Name { get; private set; }

	private Queue<int> Input = new Queue<int>();
	private int[] Memory;
	private int IP;

	public IntCodeMachine(IReadOnlyList<int> initialState, string name = default) {
		InitialState = initialState.ToArray();
		Name = name;
		Reset();
	}

	public void Reset() {
		IP = 0;
		Input.Clear();
		Memory = InitialState[..];
		Terminated = false;
	}

	public void TakeInput(int num) {
		if (Terminated) this.UnhandledInput?.Invoke(num);
		else Input.Enqueue(num);
	}

	public int[] Run(int? noun = null, int? verb = null) {
		if (noun.HasValue) Memory[1] = noun.Value;
		if (verb.HasValue) Memory[2] = verb.Value;
		while (this.Tick()) { }
		return this.Memory;
	}

	/// <summary>returns true iff work was done</summary>
	public bool Tick() {
		if (Terminated) return false;

		int ReadParameter(int idx) {
			int op = Memory[IP] / 10;
			for (int i = 0; i < idx; i++) op /= 10;
			bool immediate = op % 10 > 0;
			return Memory[immediate ? IP + idx : Memory[IP + idx]];
		}
		switch (Memory[IP] % 100) {
			case 1: // add
				Memory[Memory[IP + 3]] = ReadParameter(1) + ReadParameter(2);
				IP += 4;
				return true;
			case 2: // mul
				Memory[Memory[IP + 3]] = ReadParameter(1) * ReadParameter(2);
				IP += 4;
				return true;
			case 3: // in
				if (Input.Count == 0) return false; // more later
				Memory[Memory[IP + 1]] = Input.Dequeue();
				IP += 2;
				return true;
			case 4: // out
				var output = ReadParameter(1);
				this.Outputting?.Invoke(output);
				IP += 2;
				return true;
			case 5: // bnz
				if (ReadParameter(1) != 0) IP = ReadParameter(2);
				else IP += 3;
				return true;
			case 6: // bez
				if (ReadParameter(1) == 0) IP = ReadParameter(2);
				else IP += 3;
				return true;
			case 7: // lt
				Memory[Memory[IP + 3]] = ReadParameter(1) < ReadParameter(2) ? 1 : 0;
				IP += 4;
				return true;
			case 8: // eq
				Memory[Memory[IP + 3]] = ReadParameter(1) == ReadParameter(2) ? 1 : 0;
				IP += 4;
				return true;
			case 99: // halt
				Terminated = true;
				if (this.UnhandledInput == null) Input.Clear();
				else while (Input.Any())
						this.UnhandledInput?.Invoke(Input.Dequeue());
				return false; // done

			default: throw new ArgumentOutOfRangeException("opcode");
		}
	}
}

public class IntCodeCluster {
	public IReadOnlyList<IntCodeMachine> Machines { get; private set; }
	
	public IntCodeCluster(params IntCodeMachine[] machines) => Machines = machines;
	
	public IntCodeCluster(IEnumerable<IntCodeMachine> machines) => Machines = machines.ToArray();
	
	public void Reset() {
		foreach (var machine in Machines) machine.Reset();
	}
	
	public bool Tick() 
		=> Machines.Aggregate(false, (didwork, machine) => didwork |= machine.Tick());
	
	public void Run() {
		while (Tick()) {}
	}
	
	public IntCodeMachine this[Index index] => Machines[index];
	
	public int Count => Machines.Count;
}
