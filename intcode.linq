<Query Kind="Program">
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load ".\helpers.linq"

void Main() {
}

public class IntCodeMachine {
	public event Action<long>? Outputting;
	public event Action<long>? UnhandledInput;

	public long[] InitialState { get; }
	public bool Terminated { get; private set; } = true;
	public string? Name { get; private set; }
	public Func<long>? NextInput { private get; set; } = null;
    public bool BlockOnInput { get; set; } = false;

	private BlockingCollection<long> Input = new BlockingCollection<long>();
	private DefaultDictionary<long, long> Memory = new DefaultDictionary<long, long>(_ => 0);
	private long IP;
	private long RelativeBase;
	private bool GettingOutput = false;
	private bool SuppressingOutputEvent = false;
	private long? Output = default;

	public IntCodeMachine() : this(GetAocLongs()) { 
		if (Memory.Count == 0) throw new Exception("No IntCode program found.");
	}

	public IntCodeMachine(IReadOnlyList<long> initialState, string? name = default) {
		InitialState = initialState.ToArray();
		Name = name;
		Reset();
	}

	public void Reset() {
		RelativeBase = IP = 0;
        while (Input.TryTake(out _)) { }
		Memory.Clear();
		for (int i = 0; i < InitialState.Length; i++) if (InitialState[i] != 0) Memory[i] = InitialState[i];
		Terminated = false;
	}

	public void TakeInput(long num) {
		if (Terminated) UnhandledInput?.Invoke(num);
		else Input.Add(num);
	}
	
	public void TakeInput(params long[] nums) {
		foreach (long num in nums) TakeInput(num);
	}
	
	public void TakeInput(string s) {
		foreach (var c in s) TakeInput(c);
	}
	
	public void TakeInputLine(string s) => TakeInput(s + '\n');
	
	public void Poke(long address, long value) => Memory[address] = value;
	public long Peek(long address) => Memory[address];
	
	public long? GetOutput(bool suppressEvent = true) {
		GettingOutput = true;
		SuppressingOutputEvent = suppressEvent;
		Output = default;
		do if (!this.Tick()) return null;
		while (GettingOutput);
		return Output;
	}

    public void Run() {
		while (Tick()) { }
    }

	/// <summary>returns true iff work was done</summary>
	public bool Tick() {
		if (Terminated) return false;

		long GetResolvedAddress(int idx) => 
            (Memory[IP] / (int)Pow(10, idx + 1) % 10) switch { 
                0 => Memory[IP + idx], 
                1 => IP + idx, 
                2 => RelativeBase + Memory[IP + idx],
                _ => throw new Exception($"Opcode modifier for {Memory[IP]} out of range"),
            };
            
        long Read(int idx) => Memory[GetResolvedAddress(idx)];
		void Write(int idx, long value) => Memory[GetResolvedAddress(idx)] = value;
		
		switch ((int)(Memory[IP] % 100)) {
			case 1: // add
				Write(3, Read(1) + Read(2));
				IP += 4;
				return true;
			case 2: // mul
				Write(3, Read(1) * Read(2));
				IP += 4;
				return true;
			case 3: // in
                if (BlockOnInput) Write(1, Input.Take());
				else if (Input.TryTake(out var input)) Write(1, input);
				else if (NextInput != null) Write(1, NextInput());
                else return false; // work later
				IP += 2;
				return true;
            case 4: // out
				var output = Read(1);
				if (!SuppressingOutputEvent) Outputting?.Invoke(output);
				Output = output;
				SuppressingOutputEvent = GettingOutput = false;
				IP += 2;
				return true;
			case 5: // bnz
				if (Read(1) != 0) IP = Read(2);
				else IP += 3;
				return true;
			case 6: // bez
				if (Read(1) == 0) IP = Read(2);
				else IP += 3;
				return true;
			case 7: // lt
				Write(3, Read(1) < Read(2) ? 1 : 0);
				IP += 4;
				return true;
			case 8: // eq
				Write(3, Read(1) == Read(2) ? 1 : 0);
				IP += 4;
				return true;
			case 9: // set rel
				RelativeBase += Read(1);
				IP += 2;
				return true;
			case 99: // halt
				Terminated = true;
				if (UnhandledInput == null) while (Input.TryTake(out _)) { }
				else while (Input.TryTake(out var input)) UnhandledInput?.Invoke(input);
				return false; // done

			default: throw new ArgumentOutOfRangeException("opcode", Memory[IP], "never heard of it");
		}
	}
	
	public IntCodeMachine Clone() {
		var result = new IntCodeMachine(InitialState, Name) {
			Terminated = Terminated,
			NextInput = NextInput,
			BlockOnInput = BlockOnInput,
			IP = IP,
			RelativeBase = RelativeBase,
		};
		foreach (var item in Input) result.Input.Add(item);
		if (this.Outputting != null) 
			foreach (Action<long> handler in this.Outputting.GetInvocationList())
				result.Outputting += handler;
		if (this.UnhandledInput != null)
			foreach (Action<long> handler in this.UnhandledInput.GetInvocationList()) 
				result.UnhandledInput += handler;
		return result;
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
    
    public void RunConcurrently() {
        var tasks = new Task[Machines.Count];
        for (int i = 0; i < Machines.Count; i++) {
            var machine = Machines[i];
            machine.BlockOnInput = true;
            tasks[i] = Task.Run(machine.Run);
        }
        Task.WaitAll(tasks);
    }

	public IntCodeMachine this[Index index] => Machines[index];

	public int Count => Machines.Count;
	
	public IntCodeCluster Clone() 
		=> new IntCodeCluster(this.Machines.Select(m => m.Clone()));
}

public class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey: notnull 
{
    private Func<TKey, TValue> DefaultFactory;
    private IDictionary<TKey, TValue> State = new Dictionary<TKey, TValue>();
    
    public DefaultDictionary(Func<TKey, TValue> defaultFactory) => DefaultFactory = defaultFactory;
    
    public TValue this[TKey key] { 
        get => State.TryGetValue(key, out var result) ? result : State[key] = DefaultFactory(key);
        set => State[key] = value;
    }

    public ICollection<TKey> Keys => State.Keys;
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    public ICollection<TValue> Values => State.Values;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
    public int Count => State.Count;
    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value) => State.Add(key, value);
    public void Add(KeyValuePair<TKey, TValue> item) => State.Add(item);
    public void Clear() => State.Clear();

    public bool Contains(KeyValuePair<TKey, TValue> item) => State.ContainsKey(item.Key)
        ? object.Equals(State[item.Key], item.Value)
        : object.Equals(DefaultFactory(item.Key), item.Value);

    public bool ContainsKey(TKey key) => true;
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => State.CopyTo(array, arrayIndex);

    public bool Remove(TKey key) => State.Remove(key);
    public bool Remove(KeyValuePair<TKey, TValue> item) => State.Remove(item);
    
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
        value = State.TryGetValue(key, out var result) ? result : State[key] = DefaultFactory(key);
        return true;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => State.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
}