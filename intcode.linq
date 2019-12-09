<Query Kind="Program">
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"

void Main() {
}

public class IntCodeMachine {
	public event Action<long> Outputting;
	public event Action<long> UnhandledInput;

	public long[] InitialState { get; }
	public bool Terminated { get; private set; } = true;
	public string Name { get; private set; }

	private Queue<long> Input = new Queue<long>();
	private Dictionary<long, long> Memory = new Dictionary<long, long>();
	private long IP;
	private long RelativeBase;

	public IntCodeMachine() : this(GetAocLongs()) { }

	public IntCodeMachine(IReadOnlyList<long> initialState, string name = default) {
		InitialState = initialState.ToArray();
		Name = name;
		Reset();
	}

	public void Reset() {
		RelativeBase = IP = 0;
		Input.Clear();
		Memory.Clear();
		for (int i = 0; i < InitialState.Length; i++) Memory[i] = InitialState[i];
		Terminated = false;
	}

	public void TakeInput(long num) {
		if (Terminated) this.UnhandledInput?.Invoke(num);
		else Input.Enqueue(num);
	}

	public IReadOnlyDictionary<long, long> Run(long? noun = null, long? verb = null) {
		if (noun.HasValue) Memory[1] = noun.Value;
		if (verb.HasValue) Memory[2] = verb.Value;
		while (this.Tick()) { }
		return this.Memory;
	}

	/// <summary>returns true iff work was done</summary>
	public bool Tick() {
		if (Terminated) return false;

		long ReadParameter(int idx) {
			long op = Memory[IP] / 10;
			for (long i = 0; i < idx; i++) op /= 10;
			bool immediate = op % 10 == 1;
			bool relative = op % 10 == 2;

			var finalAddress = immediate ? IP + idx
				: (relative ? RelativeBase : 0) + Memory[IP + idx];

			return Memory.TryGetValue(finalAddress, out var result) ? result : 0;
		}
		void WriteParamter(int idx, long value) {
			long op = Memory[IP] / 10;
			for (long i = 0; i < idx; i++) op /= 10;
			bool relative = op % 10 == 2;
			
			var finalAddress = (relative ? RelativeBase : 0) + Memory[IP + idx];
			
			Memory[finalAddress] = value;
		}
		
		switch ((int)(Memory[IP] % 100)) {
			case 1: // add
				WriteParamter(3, ReadParameter(1) + ReadParameter(2));
				IP += 4;
				return true;
			case 2: // mul
				WriteParamter(3, ReadParameter(1) * ReadParameter(2));
				IP += 4;
				return true;
			case 3: // in
				if (Input.Count == 0) return false; // more later
				WriteParamter(1, Input.Dequeue());
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
				WriteParamter(3, ReadParameter(1) < ReadParameter(2) ? 1 : 0);
				IP += 4;
				return true;
			case 8: // eq
				WriteParamter(3, ReadParameter(1) == ReadParameter(2) ? 1 : 0);
				IP += 4;
				return true;
			case 9: // set rel
				RelativeBase += ReadParameter(1);
				IP += 2;
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
		while (Tick()) { }
	}

	public IntCodeMachine this[Index index] => Machines[index];

	public int Count => Machines.Count;
}